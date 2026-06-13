using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class TransactionService : ITransactionService 
{
    private readonly ITransactionRepository _repository;

    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReceiptDto> ProcessSaleAsync(List<CartItemDto> items)
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
            var product = await _repository.GetProductByIdAsync(item.ProductId); 
            if (product != null)
            {
                product.DeductStock(item.Quantity);
                _repository.UpdateProduct(product);

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

                receipt.Items.Add(new TransactionItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }
        }

        _repository.AddTransaction(receipt);

        await _repository.SaveChangesAsync();

        return new ReceiptDto
        {
            Id = receipt.Id,
            InvoiceNumber = receipt.InvoiceNumber,
            TransactionDate = receipt.TransactionDate,
            TotalAmount = receipt.TotalAmount,
            Items = receipt.Items.Select(i => new ReceiptItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
