using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IStoreServiceRepository
{
    Task<List<StoreService>> GetAllWithProductsAsync();
    Task<StoreService?> GetByIdWithProductsAsync(int id);
    Task<List<StoreService>> GetByNamesAsync(List<string> names);
    Task SaveChangesAsync();
}
