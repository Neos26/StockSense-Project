using StockSense.Domain.Entities;

namespace StockSense.Application.Interfaces;

public interface IOrderSlipRepository
{
    // Queries
    Task<List<Product>> GetLowStockProductsAsync();
    Task<List<OrderSlip>> GetSavedSlipsAsync();
    Task<OrderSlip?> GetSlipByIdAsync(int id);
    Task<Product?> GetProductByNameAndBrandAsync(string name, string brand);
    Task<Product?> GetProductByIdAsync(int id);
    
    // Commands
    Task AddSlipAsync(OrderSlip slip);
    Task UpdateSlipAsync(OrderSlip slip);
    Task DeleteSlipAsync(int id);
    Task RemoveItemAsync(int itemId);
    Task AddProductAsync(Product product);
    
    // Persistence
    Task SaveChangesAsync();
    
}