using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<List<MechanicDto>>> GetActiveMechanics()
        {
            var mechanics = await _repo.GetActiveAsync();
            return Ok(mechanics.Select(m => new MechanicDto
            {
                Id = m.Id,
                Name = m.Name,
                IsActive = m.IsActive
            }).ToList());
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<MechanicDto>>> GetAllMechanics()
        {
            var mechanics = await _repo.GetAllAsync();
            return Ok(mechanics.Select(m => new MechanicDto
            {
                Id = m.Id,
                Name = m.Name,
                IsActive = m.IsActive
            }).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMechanic([FromBody] MechanicDto dto)
        {
            var mechanic = new StockSense.Domain.Entities.Mechanic
            {
                Name = dto.Name,
                IsActive = dto.IsActive
            };
            _repo.Add(mechanic);
            await _repo.SaveChangesAsync();
            return Ok(new MechanicDto
            {
                Id = mechanic.Id,
                Name = mechanic.Name,
                IsActive = mechanic.IsActive
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMechanic(int id, [FromBody] MechanicDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = dto.Name;
            existing.IsActive = dto.IsActive;

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
