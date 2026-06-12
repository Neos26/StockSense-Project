using Microsoft.AspNetCore.Mvc;
using StockSense.Domain.Entities;
using StockSense.Application.DTOs;
using StockSense.Domain.Interfaces;

namespace StockSense.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MechanicsController : ControllerBase
    {
        private readonly IMechanicRepository _repo;

        public MechanicsController(IMechanicRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<Mechanic>>> GetActiveMechanics() =>
            await _repo.GetActiveAsync();

        [HttpGet("all")]
        public async Task<ActionResult<List<Mechanic>>> GetAllMechanics() =>
            await _repo.GetAllAsync();

        [HttpPost]
        public async Task<IActionResult> CreateMechanic([FromBody] Mechanic mechanic)
        {
            _repo.Add(mechanic);
            await _repo.SaveChangesAsync();
            return Ok(mechanic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMechanic(int id, [FromBody] Mechanic updatedMech)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = updatedMech.Name;
            existing.IsActive = updatedMech.IsActive;

            _repo.Update(existing);
            await _repo.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMechanic(int id)
        {
            var mechanic = await _repo.GetByIdAsync(id);
            if (mechanic == null) return NotFound();

            _repo.Delete(mechanic);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Mechanic deleted successfully" });
        }
    }
}
