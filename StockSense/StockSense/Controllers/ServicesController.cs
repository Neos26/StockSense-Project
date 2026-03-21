
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using StockSense.shared;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ServicesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetServices()
    {
        // Fetches all services from the database and sends them to the client
        var services = await _db.StoreServices.ToListAsync();
        return Ok(services);
    }
}