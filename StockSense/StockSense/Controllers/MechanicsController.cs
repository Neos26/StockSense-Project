using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using StockSense.shared;

namespace StockSense.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MechanicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MechanicsController(ApplicationDbContext context) { _context = context; }

        // Used by Customers booking an appointment
        [HttpGet]
        public async Task<ActionResult<List<Mechanic>>> GetActiveMechanics() =>
            await _context.Mechanics.Where(m => m.IsActive).ToListAsync();

        // Used by Admin Management Page
        [HttpGet("all")]
        public async Task<ActionResult<List<Mechanic>>> GetAllMechanics() =>
            await _context.Mechanics.ToListAsync();

        [HttpPost]
        public async Task<IActionResult> CreateMechanic([FromBody] Mechanic mechanic)
        {
            _context.Mechanics.Add(mechanic);
            await _context.SaveChangesAsync();
            return Ok(mechanic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMechanic(int id, [FromBody] Mechanic updatedMech)
        {
            var existing = await _context.Mechanics.FindAsync(id);
            if (existing == null) return NotFound();

            existing.IsActive = updatedMech.IsActive;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
