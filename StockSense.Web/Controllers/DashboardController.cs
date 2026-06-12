using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockSense.Domain.Interfaces;

namespace StockSense.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly IOrderSlipRepository _slipRepo;

        public DashboardController(IProductRepository productRepo, IOrderSlipRepository slipRepo)
        {
            _productRepo = productRepo;
            _slipRepo = slipRepo;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var allProducts = await _productRepo.GetAllProductsAsync();

            var dto = new
            {
                TotalProducts = allProducts.Count,
                LowStockCount = allProducts.Count(p => p.CurrentStock <= p.ReorderTarget),
                TotalValue = allProducts.Sum(p => p.Price * p.CurrentStock),
                PendingOrders = await _slipRepo.GetPendingCountAsync(),
                LowStockProducts = allProducts
                    .Where(p => p.CurrentStock <= p.ReorderTarget)
                    .OrderBy(p => p.CurrentStock)
                    .Take(5)
                    .ToList()
            };

            return Ok(dto);
        }
    }
}
