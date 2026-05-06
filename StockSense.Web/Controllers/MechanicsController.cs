using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Entities;
using StockSense.Application.DTOs;
using StockSense.Web.Data;

namespace StockSense.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MechanicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MechanicsController(ApplicationDbContext context) { _context = context; }

        // --- GET: Active Mechanics (For Customer Booking) ---
        [HttpGet]
        public async Task<ActionResult<List<Mechanic>>> GetActiveMechanics() =>
            await _context.Mechanics.Where(m => m.IsActive).ToListAsync();

        // --- GET: All Mechanics (For Admin Management) ---
        [HttpGet("all")]
        public async Task<ActionResult<List<Mechanic>>> GetAllMechanics() =>
            await _context.Mechanics.ToListAsync();

        // --- POST: Create Mechanic ---
        [HttpPost]
        public async Task<IActionResult> CreateMechanic([FromBody] Mechanic mechanic)
        {
            _context.Mechanics.Add(mechanic);
            await _context.SaveChangesAsync();
            return Ok(mechanic);
        }

        // --- PUT: Update Mechanic (Toggle Status or Rename) ---
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMechanic(int id, [FromBody] Mechanic updatedMech)
        {
            var existing = await _context.Mechanics.FindAsync(id);
            if (existing == null) return NotFound();

            // Update both properties in case the Admin wants to rename them
            existing.Name = updatedMech.Name;
            existing.IsActive = updatedMech.IsActive;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // --- DELETE: Remove Mechanic Permanently ---
        // This is triggered by your ExecuteDelete() function in the Blazor page
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMechanic(int id)
        {
            var mechanic = await _context.Mechanics.FindAsync(id);
            if (mechanic == null) return NotFound();

            _context.Mechanics.Remove(mechanic);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mechanic deleted successfully" });
        }
    }
}
