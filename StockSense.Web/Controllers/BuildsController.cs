using Microsoft.AspNetCore.Mvc;
using StockSense.Domain.Entities;
using StockSense.Application.Interfaces;
using StockSense.Domain.Interfaces;

namespace StockSense.Web.Server.Controllers
{
    [Route("api/builds")]
    [ApiController]
    public class BuildsController : ControllerBase
    {
        private readonly IBuildService _buildService;
        private readonly IBuildRequestRepository _buildRepo;

        public BuildsController(IBuildService buildService, IBuildRequestRepository buildRepo)
        {
            _buildService = buildService;
            _buildRepo = buildRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBuild([FromBody] BuildRequest request)
        {
            if (request == null) return BadRequest("Request is empty.");

            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";

            _buildRepo.Add(request);
            await _buildRepo.SaveChangesAsync();

            return Ok(request);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<BuildRequest>>> GetAllBuilds()
        {
            return await _buildRepo.GetAllAsync();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var updated = await _buildService.UpdateStatusAsync(id, newStatus);
            if (!updated) return NotFound();
            return Ok();
        }

        [HttpGet("customer/{userName}")]
        public async Task<ActionResult<List<BuildRequest>>> GetCustomerBuilds(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return BadRequest("User name is required.");
            return await _buildRepo.GetByCustomerNameAsync(userName);
        }
    }
}
