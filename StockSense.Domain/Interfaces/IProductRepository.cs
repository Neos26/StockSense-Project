using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetByIdsAsync(List<int> ids);
    void Update(Product product);
    Task SaveChangesAsync();
}
