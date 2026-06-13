using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Domain.Interfaces;

namespace StockSense.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IStoreServiceRepository _serviceRepo;
        private readonly IProductRepository _productRepo;

        public ServicesController(IStoreServiceRepository serviceRepo, IProductRepository productRepo)
        {
            _serviceRepo = serviceRepo;
            _productRepo = productRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var services = await _serviceRepo.GetAllWithProductsAsync();
            var dto = services.Select(s => new StoreServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Category = s.Category,
                EstimatedMinutes = s.EstimatedMinutes,
                Status = s.Status,
                RequiredProducts = s.RequiredProducts.Select(p => new ProductDto(
                    p.Id, p.Name, p.Category, p.Brand, p.Price,
                    p.CurrentStock, p.ReorderTarget, p.SupplierId,
                    p.Supplier?.Name ?? ""
                )).ToList()
            }).ToList();
            return Ok(dto);
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var inventory = await _productRepo.GetAllProductsAsync();
            var dto = inventory.Select(p => new ProductDto(
                p.Id, p.Name, p.Category, p.Brand, p.Price,
                p.CurrentStock, p.ReorderTarget, p.SupplierId,
                p.Supplier?.Name ?? ""
            )).ToList();
            return Ok(dto);
        }

        [HttpPost("update-products")]
        public async Task<IActionResult> UpdateServiceProducts([FromBody] UpdateServiceProductsDto dto)
        {
            var service = await _serviceRepo.GetByIdWithProductsAsync(dto.ServiceId);
            if (service == null) return NotFound("Service not found");

            var selectedProducts = await _productRepo.GetByIdsAsync(dto.ProductIds);

            service.RequiredProducts = selectedProducts;

            await _serviceRepo.SaveChangesAsync();
            return Ok();
        }
    }
}
