using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StockSense.Web.Data;

namespace StockSense.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // 1. Fetch all products to calculate top-level totals
            var allProducts = await _context.Products.AsNoTracking().ToListAsync();

            // 2. Fetch specific low stock products AND include their Supplier info
            // This prevents the 'Supplier' property from being null on the Client side
            var lowStockList = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.CurrentStock <= p.ReorderTarget)
                .OrderBy(p => p.CurrentStock)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            // 3. Create the DTO
            var dto = new
            {
                TotalProducts = allProducts.Count,
                LowStockCount = allProducts.Count(p => p.CurrentStock <= p.ReorderTarget),
                TotalValue = allProducts.Sum(p => p.Price * p.CurrentStock),
                PendingOrders = await _context.OrderSlips.CountAsync(s => !s.IsReceived),
                LowStockProducts = lowStockList
            };

            return Ok(dto);
        }
    }
}
