using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using StockSense.Shared;
using System.Text;


[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSender _emailSender;

    public ProductsController(ApplicationDbContext context, EmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    [HttpPost("submit-build")]
    public async Task<IActionResult> SubmitBuild([FromBody] BuildRequest request)
    {
        _context.BuildRequests.Add(request);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // --- THE EMAIL QUOTE ENDPOINT ---
    [HttpPost("send-quote")]
    public async Task<IActionResult> SendQuote([FromBody] EmailQuoteRequest request)
    {
        // 1. Fetch products from DB
        var selectedProducts = await _context.Products
            .Where(p => request.ProductIds.Contains(p.Id))
            .ToListAsync();

        if (!selectedProducts.Any()) return BadRequest("No valid products found.");

        // 2. Calculate total on server (Secure)
        decimal grandTotal = selectedProducts.Sum(p => p.Price);

        // 3. Build HTML
        var sb = new StringBuilder();
        sb.AppendLine("<h1>StockSense Build Quotation</h1>");
        sb.AppendLine($"<p>Hello {request.UserEmail}, here is the quote for your custom build:</p>");
        sb.AppendLine("<table border='1' cellpadding='10' cellspacing='0' style='border-collapse:collapse; width:100%; text-align:left;'>");
        sb.AppendLine("<tr style='background-color:#f2f2f2;'><th>Part Name</th><th>Category</th><th>Price</th></tr>");

        foreach (var p in selectedProducts)
        {
            sb.AppendLine($"<tr><td>{p.Name}</td><td>{p.Category}</td><td>P {p.Price:N2}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<h3>Grand Total: P {grandTotal:N2}</h3>");

        // 4. Send Email using your existing service
        try
        {
            await _emailSender.SendEmailAsync(request.UserEmail, "Custom Build Quote", sb.ToString());
            return Ok(new { message = "Email sent" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    public class EmailQuoteRequest
    {
        public string UserEmail { get; set; } = "";
        public List<int> ProductIds { get; set; } = new();
    }
}
