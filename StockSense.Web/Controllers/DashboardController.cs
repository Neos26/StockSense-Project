using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IOrderSlipService _orderSlipService;

    public DashboardController(IProductService productService, IOrderSlipService orderSlipService)
    {
        _productService = productService;
        _orderSlipService = orderSlipService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var allProducts = await _productService.GetAllProductsAsync();

        var dto = new
        {
            TotalProducts = allProducts.Count,
            LowStockCount = allProducts.Count(p => p.CurrentStock <= p.ReorderTarget),
            TotalValue = allProducts.Sum(p => p.Price * p.CurrentStock),
            PendingOrders = await _orderSlipService.GetPendingCountAsync(),
            LowStockProducts = allProducts
                .Where(p => p.CurrentStock <= p.ReorderTarget)
                .OrderBy(p => p.CurrentStock)
                .Take(5)
                .ToList()
        };

        return Ok(dto);
    }
}
