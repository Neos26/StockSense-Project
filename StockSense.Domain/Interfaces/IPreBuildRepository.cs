using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IPreBuildRepository
{
    Task<List<PreBuildPackage>> GetAllPackagesAsync();
    Task<PreBuildPackage?> GetPackageByIdAsync(int id);
    Task AddPackageAsync(PreBuildPackage package);
    Task UpdatePackageAsync(PreBuildPackage package);
    Task DeletePackageAsync(int id);
    Task<List<Product>> GetProductsByIdsAsync(List<int> productIds);
}
