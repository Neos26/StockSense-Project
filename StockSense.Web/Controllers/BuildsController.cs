using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
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
        public async Task<IActionResult> CreateBuild([FromBody] CreateBuildRequestDto dto)
        {
            if (dto == null) return BadRequest("Request is empty.");

            var request = new StockSense.Domain.Entities.BuildRequest
            {
                CustomerName = dto.CustomerName,
                BuildName = dto.BuildName,
                SelectedPartsJson = dto.SelectedPartsJson,
                TotalPrice = dto.TotalPrice,
                CreatedAt = DateTime.Now,
                Status = "Pending"
            };

            _buildRepo.Add(request);
            await _buildRepo.SaveChangesAsync();

            return Ok(new BuildRequestDto
            {
                Id = request.Id,
                CustomerName = request.CustomerName,
                BuildName = request.BuildName,
                SelectedPartsJson = request.SelectedPartsJson,
                TotalPrice = request.TotalPrice,
                CreatedAt = request.CreatedAt,
                Status = request.Status
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<BuildRequestDto>>> GetAllBuilds()
        {
            var builds = await _buildRepo.GetAllAsync();
            return Ok(builds.Select(b => new BuildRequestDto
            {
                Id = b.Id,
                CustomerName = b.CustomerName,
                BuildName = b.BuildName,
                SelectedPartsJson = b.SelectedPartsJson,
                TotalPrice = b.TotalPrice,
                CreatedAt = b.CreatedAt,
                Status = b.Status
            }).ToList());
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var updated = await _buildService.UpdateStatusAsync(id, newStatus);
            if (!updated) return NotFound();
            return Ok();
        }

        [HttpGet("customer/{userName}")]
        public async Task<ActionResult<List<BuildRequestDto>>> GetCustomerBuilds(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return BadRequest("User name is required.");
            var builds = await _buildRepo.GetByCustomerNameAsync(userName);
            return Ok(builds.Select(b => new BuildRequestDto
            {
                Id = b.Id,
                CustomerName = b.CustomerName,
                BuildName = b.BuildName,
                SelectedPartsJson = b.SelectedPartsJson,
                TotalPrice = b.TotalPrice,
                CreatedAt = b.CreatedAt,
                Status = b.Status
            }).ToList());
        }
    }
}
