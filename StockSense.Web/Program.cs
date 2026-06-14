using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Extensions;
using StockSense.Web.Components;
using StockSense.Web.Components.Account;
using StockSense.Infrastructure.Data;
using StockSense.Infrastructure.Services;
using StockSense.Web.Helpers;
using StockSense.Application.Interfaces;
using StockSense.Application.Services;
using StockSense.Domain.Interfaces;
using StockSense.Infrastructure.Data.Repositories;


var builder = WebApplication.CreateBuilder(args);

// --- 1. CORE SERVICES ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();
builder.Services.AddLocalization();



// --- 2. AUTHENTICATION & COOKIES ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

// --- 3. DATABASE ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- 4. IDENTITY CONFIGURATION ---
// RequireConfirmedAccount = true forces Identity to use the Email Confirmation flow
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.AllowedForNewUsers = true;
});

// --- 5. EMAIL REGISTRATION (FIXED LOCATION) ---
// This MUST come directly after Identity is configured so it overrides the defaults.
builder.Services.AddTransient<StockSense.Application.Interfaces.IEmailSender<ApplicationUser>, EmailSender>();
// Keeping this just in case you inject the concrete class elsewhere (like in a Contact page)
builder.Services.AddTransient<EmailSender>();

// --- 6. RATE LIMITING CONFIGURATION ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(policyName: "login-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(30);
        opt.QueueLimit = 0;
    });

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// --- 7. ADDITIONAL SERVICES ---
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, StockSense.Web.Utility.Security.BCryptPasswordHasher>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


// --- DATA ACCESS (Repositories) ---
builder.Services.AddScoped<IPreBuildRepository, PreBuildRepository>();
builder.Services.AddScoped<IOrderSlipRepository, OrderSlipRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPinnedSlipRepository, PinnedSlipRepository>();
builder.Services.AddScoped<IMechanicRepository, MechanicRepository>();
builder.Services.AddScoped<IBuildRequestRepository, BuildRequestRepository>();
builder.Services.AddScoped<IStoreServiceRepository, StoreServiceRepository>();

// --- INFRASTRUCTURE (External Tools) ---
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IOrderEmailSender, OrderEmailSender>();
builder.Services.AddSingleton<IPdfDownloadCache, PdfDownloadCache>();

// --- APPLICATION (Business Logic) ---
builder.Services.AddScoped<IPreBuildService, PreBuildService>();
builder.Services.AddScoped<IOrderSlipService, OrderSlipService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBuildService, BuildService>();
builder.Services.AddScoped<IMechanicService, MechanicService>();
builder.Services.AddScoped<IStoreServiceService, StoreServiceService>();
builder.Services.AddBlazorBlueprintComponents();
builder.Services.AddBlazorBlueprintPrimitives();
builder.Services.AddHttpClient();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new PhDateTimeConverter());
    });

var app = builder.Build();

// --- 8. PIPELINE CONFIGURATION ---
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Global exception handler for API endpoints — returns JSON instead of stack traces
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
        Console.WriteLine($"API ERROR: {ex.Message}");
    }
});

// --- 9. AUTOMATIC MIGRATION HELPER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.CanConnect())
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("STARTUP ERROR (Migration): " + ex.Message);
    }
}

// --- 10. MIDDLEWARE EXECUTION ---
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(StockSense.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();
app.MapControllers();

app.MapGet("/api/download/{token}", (string token, IPdfDownloadCache cache) =>
{
    var data = cache.Retrieve(token);
    return data is null
        ? Results.NotFound("Download expired or not found.")
        : Results.File(data, "application/pdf");
});

app.Run();