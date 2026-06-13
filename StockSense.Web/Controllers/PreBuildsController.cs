using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreBuildsController : ControllerBase
    {
        private readonly IPreBuildService _service;

        public PreBuildsController(IPreBuildService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<PreBuildPackageDto>>> GetMatchingPackages(
            [FromQuery] string brand,
            [FromQuery] string model,
            [FromQuery] string cc,
            [FromQuery] decimal minBudget,
            [FromQuery] decimal maxBudget)
        {
            var allPackages = await _service.GetAllPackagesAsync();
            var matchingPackages = allPackages
                .Where(p => p.CompatibleBrand == brand &&
                            p.CompatibleModel == model &&
                            p.TargetCC == cc &&
                            p.IsActive == true)
                .ToList();

            var affordablePackages = matchingPackages
                .Where(p => p.TotalPrice >= minBudget && p.TotalPrice <= maxBudget)
                .ToList();

            return Ok(affordablePackages);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<PreBuildPackageDto>>> GetAllPackages()
        {
            var packages = await _service.GetAllPackagesAsync();
            return Ok(packages);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePreBuild([FromBody] CreatePreBuildDto dto)
        {
            if (dto.SelectedProductIds == null || !dto.SelectedProductIds.Any())
            {
                return BadRequest("A package must contain at least one product.");
            }

            var result = await _service.CreatePackageAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePreBuild(int id, [FromBody] CreatePreBuildDto dto)
        {
            var result = await _service.UpdatePackageAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePreBuild(int id)
        {
            await _service.DeletePackageAsync(id);
            return Ok();
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _service.TogglePackageActiveStatusAsync(id);
            return Ok();
        }
    }
}
