using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

// Make sure you implement the interface from your tree!
public class TransactionService : ITransactionService 
{
    private readonly ITransactionRepository _repository;

    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Transaction> ProcessSaleAsync(List<CartItem> items)
    {
        var receipt = new Transaction
        {
            InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            TransactionDate = DateTime.Now,
            TotalAmount = items.Sum(i => i.Price * i.Quantity),
            Items = new List<TransactionItem>()
        };

        foreach (var item in items)
        {
            // Use Repo instead of _context
            var product = await _repository.GetProductByIdAsync(item.ProductId); 
            if (product != null)
            {
                // Deduct Stock
                product.CurrentStock -= item.Quantity;
                _repository.UpdateProduct(product);

                // Add to SalesHistory
                _repository.AddSalesHistory(new SalesHistory
                {
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    ProductID = product.Id.ToString(),
                    ProductName = product.Name,
                    Brand = product.Brand,
                    Category = product.Category,
                    QtySold = (float)item.Quantity,
                    UnitPrice = (float)product.Price,
                    TotalSales = (float)(item.Quantity * product.Price),
                    MonthNum = (float)DateTime.Now.Month
                });

                // Add to Receipt
                receipt.Items.Add(new TransactionItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }
        }

        // Save the Receipt
        _repository.AddTransaction(receipt);

        // Commit everything to the database at once
        await _repository.SaveChangesAsync();

        return receipt;
    }
}