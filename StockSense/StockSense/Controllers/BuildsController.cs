using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data; // Points to AppDbContext
using StockSense.Shared;      // Points to BuildRequest model


namespace StockSense.Server.Controllers
{
    // 👇 Forces the URL to be "api/builds" regardless of class name

    [Route("api/builds")]
    [ApiController]
    public class BuildsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BuildsController(ApplicationDbContext context)
        {
            _context = context;
        }

     
        [HttpPost]
        public async Task<IActionResult> CreateBuild([FromBody] BuildRequest request)
        {
            if (request == null) return BadRequest("Request is empty.");

            // Set server-side defaults
            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";

            _context.BuildRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(request);
        }

      
        [HttpGet("all")]
        public async Task<ActionResult<List<BuildRequest>>> GetAllBuilds()
        {
            return await _context.BuildRequests
                                 .OrderByDescending(b => b.CreatedAt) // Newest first
                                 .ToListAsync();
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


        // Add this inside BuildsController class
        // GET: api/builds/customer/{userName}
        [HttpGet("customer/{userName}")]
        public async Task<ActionResult<List<BuildRequest>>> GetCustomerBuilds(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return BadRequest("User name is required.");

            // Fetch only the builds belonging to this specific user
            return await _context.BuildRequests
                                 .Where(b => b.CustomerName == userName)
                                 .OrderByDescending(b => b.CreatedAt)
                                 .ToListAsync();
        }
    }

}