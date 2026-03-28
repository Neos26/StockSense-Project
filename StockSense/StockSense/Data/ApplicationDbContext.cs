using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockSense.shared;
using StockSense.Shared;

namespace StockSense.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<StoreService> StoreServices { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BuildRequest> BuildRequests { get; set; }
        public DbSet<OrderSlip> OrderSlips { get; set; }
        public DbSet<OrderSlipItem> OrderSlipItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Mechanic> Mechanics { get; set; }
    }
}
