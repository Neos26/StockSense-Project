using StockSense.Domain.Entities;

namespace StockSense.Application.Interfaces;

public interface ITransactionRepository
{
    Task<Product?> GetProductByIdAsync(int productId);
    void UpdateProduct(Product product);
    void AddSalesHistory(SalesHistory history);
    void AddTransaction(Transaction transaction);
    
    // This allows us to save all the stock deductions, history, and receipts in one single transaction!
    Task SaveChangesAsync(); 
}