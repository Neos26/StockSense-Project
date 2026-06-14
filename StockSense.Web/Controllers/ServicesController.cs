using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IStoreServiceService _storeServiceService;
    private readonly IProductService _productService;

    public ServicesController(IStoreServiceService storeServiceService, IProductService productService)
    {
        _storeServiceService = storeServiceService;
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetServices()
    {
        var services = await _storeServiceService.GetAllWithProductsAsync();
        return Ok(services);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpPost("update-products")]
    public async Task<IActionResult> UpdateServiceProducts([FromBody] UpdateServiceProductsDto dto)
    {
        var updated = await _storeServiceService.UpdateServiceProductsAsync(dto);
        if (!updated) return NotFound(ApiResponse.NotFound("Service"));
        return Ok();
    }
}
