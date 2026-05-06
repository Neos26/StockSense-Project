using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// Points to AppDbContext
using StockSense.Domain.Entities;
using StockSense.Application.DTOs;
using StockSense.Web.Data; // Points to BuildRequest model


namespace StockSense.Web.Server.Controllers
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

            // --- INVENTORY DEDUCTION LOGIC ---
            // Only deduct stock if the status is changing TO "Completed" for the first time
            if (newStatus == "Completed" && build.Status != "Completed")
            {
                if (!string.IsNullOrEmpty(build.SelectedPartsJson))
                {
                    try
                    {
                        // 1. Read the JSON string back into a list of Products
                        var usedParts = JsonSerializer.Deserialize<List<Product>>(
                            build.SelectedPartsJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (usedParts != null)
                        {
                            foreach (var part in usedParts)
                            {
                                // 2. Find the actual product in the database
                                var dbProduct = await _context.Products.FindAsync(part.Id);

                                // 3. Deduct the stock (ensuring it doesn't go below 0)
                                if (dbProduct != null && dbProduct.CurrentStock > 0)
                                {
                                    dbProduct.CurrentStock -= 1;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to deduct inventory: {ex.Message}");
                        // You can decide if you want to abort the status change here, 
                        // but usually, it's safe to just log it and proceed.
                    }
                }
            }

            // Update the status and save all changes (both the build status AND the product stock)
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
