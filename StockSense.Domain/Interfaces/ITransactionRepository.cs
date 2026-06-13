using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Product?> GetProductByIdAsync(int productId);
    void UpdateProduct(Product product);
    void AddSalesHistory(SalesHistory history);
    void AddTransaction(Transaction transaction);
    Task SaveChangesAsync();
}
