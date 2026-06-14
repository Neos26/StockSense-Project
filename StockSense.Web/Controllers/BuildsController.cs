using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers;

[Route("api/builds")]
[ApiController]
public class BuildsController : ControllerBase
{
    private readonly IBuildService _buildService;

    public BuildsController(IBuildService buildService)
    {
        _buildService = buildService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBuild([FromBody] CreateBuildRequestDto dto)
    {
        if (dto == null) return BadRequest(ApiResponse.Error("Request is empty."));

        var result = await _buildService.CreateBuildAsync(dto);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<BuildRequestDto>>> GetAllBuilds()
    {
        var builds = await _buildService.GetAllBuildsAsync();
        return Ok(builds);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var updated = await _buildService.UpdateStatusAsync(id, newStatus);
        if (!updated) return NotFound(ApiResponse.NotFound("Build"));
        return Ok();
    }

    [HttpGet("customer/{userName}")]
    public async Task<ActionResult<List<BuildRequestDto>>> GetCustomerBuilds(string userName)
    {
        if (string.IsNullOrEmpty(userName)) return BadRequest(ApiResponse.Error("User name is required."));
        var builds = await _buildService.GetCustomerBuildsAsync(userName);
        return Ok(builds);
    }
}
