using Microsoft.AspNetCore.Mvc;
using StockSense.Domain.Entities;
using StockSense.Application.DTOs;
using StockSense.Domain.Interfaces;

namespace StockSense.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreBuildsController : ControllerBase
    {
        private readonly IPreBuildRepository _repo;

        public PreBuildsController(IPreBuildRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<PreBuildPackage>>> GetMatchingPackages(
            [FromQuery] string brand,
            [FromQuery] string model,
            [FromQuery] string cc,
            [FromQuery] decimal minBudget,
            [FromQuery] decimal maxBudget)
        {
            var allPackages = await _repo.GetAllPackagesAsync();
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
        public async Task<ActionResult<List<PreBuildPackage>>> GetAllPackages()
        {
            var packages = await _repo.GetAllPackagesAsync();
            return Ok(packages);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePreBuild([FromBody] CreatePreBuildDto dto)
        {
            if (dto.SelectedProductIds == null || !dto.SelectedProductIds.Any())
            {
                return BadRequest("A package must contain at least one product.");
            }

            var productsToInclude = await _repo.GetProductsByIdsAsync(dto.SelectedProductIds);

            if (!productsToInclude.Any())
            {
                return BadRequest("None of the selected products were found in the database.");
            }

            var newPackage = new PreBuildPackage()
            {
                Name = dto.Name,
                Description = dto.Description,
                CompatibleBrand = dto.CompatibleBrand,
                CompatibleModel = dto.CompatibleModel,
                TargetCC = dto.TargetCC,
                EstimatedAddedCC = dto.EstimatedAddedCC,
                IncludedProducts = productsToInclude
            };

            await _repo.AddPackageAsync(newPackage);
            return Ok(newPackage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePreBuild(int id, [FromBody] CreatePreBuildDto dto)
        {
            var pkg = await _repo.GetPackageByIdAsync(id);
            if (pkg == null) return NotFound();

            pkg.Name = dto.Name;
            pkg.Description = dto.Description;
            pkg.CompatibleBrand = dto.CompatibleBrand;
            pkg.CompatibleModel = dto.CompatibleModel;
            pkg.TargetCC = dto.TargetCC;
            pkg.EstimatedAddedCC = dto.EstimatedAddedCC;
            pkg.IncludedProducts = await _repo.GetProductsByIdsAsync(dto.SelectedProductIds);

            await _repo.UpdatePackageAsync(pkg);
            return Ok(pkg);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePreBuild(int id)
        {
            await _repo.DeletePackageAsync(id);
            return Ok();
        }

        public class ToggleActiveDto
        {
            public bool IsActive { get; set; }
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] ToggleActiveDto dto)
        {
            var pkg = await _repo.GetPackageByIdAsync(id);
            if (pkg == null) return NotFound();

            pkg.IsActive = dto.IsActive;
            await _repo.UpdatePackageAsync(pkg);
            return Ok();
        }
    }
}
