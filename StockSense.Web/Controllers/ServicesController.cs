using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Entities;
using StockSense.Application.DTOs;
using StockSense.Web.Data; // For the Update DTO

namespace StockSense.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            // Eagerly load RequiredProducts so the frontend knows which parts are needed
            var services = await _db.StoreServices
                .Include(s => s.RequiredProducts)
                .ToListAsync();
            return Ok(services);
        }

        // --- NEW: FOR ADMIN MODAL ---
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            // Fetches all products so the admin can pick them in the Manage Parts modal
            var inventory = await _db.Products.ToListAsync();
            return Ok(inventory);
        }

        // --- NEW: SAVE LINKED PRODUCTS ---
        [HttpPost("update-products")]
        public async Task<IActionResult> UpdateServiceProducts([FromBody] UpdateServiceProductsDto dto)
        {
            // 1. Fetch the service and its current list of products
            var service = await _db.StoreServices
                .Include(s => s.RequiredProducts)
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);

            if (service == null) return NotFound("Service not found");

            // 2. Fetch the real product objects from your inventory based on the IDs sent from the UI
            var selectedProducts = await _db.Products
                .Where(p => dto.ProductIds.Contains(p.Id))
                .ToListAsync();

            // 3. Update the relationship (Many-to-Many)
            service.RequiredProducts = selectedProducts;

            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
