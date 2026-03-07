using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using StockSense.Shared;

[Route("api/[controller]")]
[ApiController]
public class BuildsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BuildsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitBuild([FromBody] BuildRequest request)
    {
        request.Status = "Pending"; // Always start as Pending
        _context.BuildRequests.Add(request);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<BuildRequest>>> GetAllBuilds()
    {
        return await _context.BuildRequests.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var build = await _context.BuildRequests.FindAsync(id);
        if (build == null) return NotFound();

        build.Status = newStatus;
        await _context.SaveChangesAsync();
        return Ok();
    }
}