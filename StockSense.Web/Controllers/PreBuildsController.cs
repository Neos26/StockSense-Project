using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSense.Application.DTOs;
using StockSense.Domain.Entities;
using StockSense.Infrastructure.Data.Repositories;

namespace StockSense.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PreBuildsController : ControllerBase
{
    private readonly PreBuildRepository _repo;

    public PreBuildsController(PreBuildRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<PreBuildPackageDto>>> GetMatchingPackages(
        [FromQuery] string brand, [FromQuery] string model,
        [FromQuery] string cc, [FromQuery] decimal minBudget, [FromQuery] decimal maxBudget)
    {
        var allPackages = await _repo.GetAllAsync();
        var matching = allPackages
            .Where(p => p.CompatibleBrand == brand && p.CompatibleModel == model && p.TargetCC == cc && p.IsActive)
            .Where(p => p.TotalPrice >= minBudget && p.TotalPrice <= maxBudget)
            .Select(MapToDto).ToList();
        return Ok(matching);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<PreBuildPackageDto>>> GetAllPackages()
    {
        var packages = await _repo.GetAllAsync();
        return Ok(packages.Select(MapToDto).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> CreatePreBuild([FromBody] CreatePreBuildDto dto)
    {
        if (dto.SelectedProductIds == null || !dto.SelectedProductIds.Any())
            return BadRequest(ApiResponse.Error("A package must contain at least one product."));

        var selectedProducts = await _repo.GetProductsByIdsAsync(dto.SelectedProductIds);
        var package = new PreBuildPackage
        {
            Name = dto.Name, Description = dto.Description, CompatibleBrand = dto.CompatibleBrand,
            CompatibleModel = dto.CompatibleModel, TargetCC = dto.TargetCC,
            EstimatedAddedCC = dto.EstimatedAddedCC, IsActive = true,
            IncludedProducts = selectedProducts
        };

        await _repo.AddAsync(package);
        return Ok(MapToDto(package));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePreBuild(int id, [FromBody] CreatePreBuildDto dto)
    {
        var package = await _repo.GetByIdAsync(id);
        if (package == null) return NotFound(ApiResponse.NotFound("Package"));

        package.Name = dto.Name; package.Description = dto.Description;
        package.CompatibleBrand = dto.CompatibleBrand; package.CompatibleModel = dto.CompatibleModel;
        package.TargetCC = dto.TargetCC; package.EstimatedAddedCC = dto.EstimatedAddedCC;
        package.IncludedProducts = await _repo.GetProductsByIdsAsync(dto.SelectedProductIds);

        await _repo.UpdateAsync(package);
        return Ok(MapToDto(package));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreBuild(int id)
    {
        var package = await _repo.GetByIdAsync(id);
        if (package == null) return NotFound(ApiResponse.NotFound("Package"));
        await _repo.DeleteAsync(id);
        return Ok(ApiResponse.Success("Package deleted."));
    }

    [HttpPatch("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var package = await _repo.GetByIdAsync(id);
        if (package == null) return NotFound(ApiResponse.NotFound("Package"));
        package.IsActive = !package.IsActive;
        await _repo.UpdateAsync(package);
        return Ok(ApiResponse.Success("Package toggled."));
    }

    private static PreBuildPackageDto MapToDto(PreBuildPackage p) => new()
    {
        Id = p.Id, Name = p.Name, Description = p.Description,
        CompatibleBrand = p.CompatibleBrand, CompatibleModel = p.CompatibleModel,
        TargetCC = p.TargetCC, EstimatedAddedCC = p.EstimatedAddedCC,
        IsActive = p.IsActive, TotalPrice = p.TotalPrice,
        IncludedProducts = p.IncludedProducts.Select(prod => new PreBuildProductDto
        {
            Id = prod.Id, Name = prod.Name, Brand = prod.Brand, Price = prod.Price
        }).ToList()
    };
}
