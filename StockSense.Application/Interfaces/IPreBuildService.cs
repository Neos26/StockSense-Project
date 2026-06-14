using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IPreBuildService
{
    Task<List<PreBuildPackageDto>> GetAllPackagesAsync();
    Task<PreBuildPackageDto?> GetPackageByIdAsync(int id);
    Task<PreBuildPackageDto> CreatePackageAsync(CreatePreBuildDto request);
    Task<PreBuildPackageDto> UpdatePackageAsync(int id, CreatePreBuildDto request);
    Task<bool> TogglePackageActiveStatusAsync(int id);
    Task<bool> DeletePackageAsync(int id);
}