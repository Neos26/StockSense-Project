using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;

namespace StockSense.Application.Services;

public class PreBuildService : IPreBuildService
{
    private readonly IPreBuildRepository _repository;

    public PreBuildService(IPreBuildRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PreBuildPackageDto>> GetAllPackagesAsync()
    {
        var packages = await _repository.GetAllPackagesAsync();
        return packages.Select(MapToDto).ToList();
    }

    public async Task<PreBuildPackageDto?> GetPackageByIdAsync(int id)
    {
        var package = await _repository.GetPackageByIdAsync(id);
        return package == null ? null : MapToDto(package);
    }

    public async Task<PreBuildPackageDto> CreatePackageAsync(CreatePreBuildDto request)
    {
        var selectedProducts = await _repository.GetProductsByIdsAsync(request.SelectedProductIds);

        var package = new PreBuildPackage
        {
            Name = request.Name,
            Description = request.Description,
            CompatibleBrand = request.CompatibleBrand,
            CompatibleModel = request.CompatibleModel,
            TargetCC = request.TargetCC,
            EstimatedAddedCC = request.EstimatedAddedCC,
            IsActive = true,
            IncludedProducts = selectedProducts 
        };

        await _repository.AddPackageAsync(package);
        return MapToDto(package);
    }

    public async Task<PreBuildPackageDto> UpdatePackageAsync(int id, CreatePreBuildDto request)
    {
        var package = await _repository.GetPackageByIdAsync(id);
        if (package == null) throw new Exception("Package not found");

        package.Name = request.Name;
        package.Description = request.Description;
        package.CompatibleBrand = request.CompatibleBrand;
        package.CompatibleModel = request.CompatibleModel;
        package.TargetCC = request.TargetCC;
        package.EstimatedAddedCC = request.EstimatedAddedCC;

        var selectedProducts = await _repository.GetProductsByIdsAsync(request.SelectedProductIds);
        package.IncludedProducts = selectedProducts;

        await _repository.UpdatePackageAsync(package);
        return MapToDto(package);
    }

    public async Task TogglePackageActiveStatusAsync(int id)
    {
        var package = await _repository.GetPackageByIdAsync(id);
        if (package != null)
        {
            package.IsActive = !package.IsActive;
            await _repository.UpdatePackageAsync(package);
        }
    }

    public async Task DeletePackageAsync(int id)
    {
        await _repository.DeletePackageAsync(id);
    }

    private PreBuildPackageDto MapToDto(PreBuildPackage p)
    {
        return new PreBuildPackageDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CompatibleBrand = p.CompatibleBrand,
            CompatibleModel = p.CompatibleModel,
            TargetCC = p.TargetCC,
            EstimatedAddedCC = p.EstimatedAddedCC,
            IsActive = p.IsActive,
            TotalPrice = p.TotalPrice, 
            IncludedProducts = p.IncludedProducts.Select(prod => new PreBuildProductDto
            {
                Id = prod.Id,
                Name = prod.Name,
                Brand = prod.Brand,
                Price = prod.Price
            }).ToList()
        };
    }
}