using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StockSense.Domain.Entities;
using StockSense.Application.DTOs;
using StockSense.Domain.Interfaces;

namespace StockSense.Web.Server.Controllers
{
    [Route("api/builds")]
    [ApiController]
    public class BuildsController : ControllerBase
    {
        private readonly IBuildRequestRepository _buildRepo;
        private readonly IProductRepository _productRepo;

        public BuildsController(IBuildRequestRepository buildRepo, IProductRepository productRepo)
        {
            _buildRepo = buildRepo;
            _productRepo = productRepo;
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
            var build = await _buildRepo.GetByIdAsync(id);
            if (build == null) return NotFound();

            if (newStatus == "Completed" && build.Status != "Completed")
            {
                if (!string.IsNullOrEmpty(build.SelectedPartsJson))
                {
                    try
                    {
                        var usedParts = JsonSerializer.Deserialize<List<Product>>(
                            build.SelectedPartsJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (usedParts != null)
                        {
                            foreach (var part in usedParts)
                            {
                                var dbProduct = await _productRepo.GetByIdAsync(part.Id);
                                if (dbProduct != null && dbProduct.CurrentStock > 0)
                                {
                                    dbProduct.CurrentStock -= 1;
                                    _productRepo.Update(dbProduct);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to deduct inventory: {ex.Message}");
                    }
                }
            }

            build.Status = newStatus;
            await _buildRepo.SaveChangesAsync();

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
