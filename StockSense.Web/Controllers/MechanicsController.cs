using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MechanicsController : ControllerBase
{
    private readonly IMechanicService _mechanicService;

    public MechanicsController(IMechanicService mechanicService)
    {
        _mechanicService = mechanicService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MechanicDto>>> GetActiveMechanics()
    {
        var mechanics = await _mechanicService.GetActiveAsync();
        return Ok(mechanics);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<MechanicDto>>> GetAllMechanics()
    {
        var mechanics = await _mechanicService.GetAllAsync();
        return Ok(mechanics);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMechanic([FromBody] MechanicDto dto)
    {
        var result = await _mechanicService.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMechanic(int id, [FromBody] MechanicDto dto)
    {
        var updated = await _mechanicService.UpdateAsync(id, dto);
        if (!updated) return NotFound(ApiResponse.NotFound("Mechanic"));
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMechanic(int id)
    {
        var deleted = await _mechanicService.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse.NotFound("Mechanic"));
        return Ok(ApiResponse.Success("Mechanic deleted successfully"));
    }
}
