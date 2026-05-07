using StockSense.Domain.Entities;

namespace StockSense.Application.Interfaces;

public interface ITransactionService
{
    Task<Transaction> ProcessSaleAsync(List<CartItem> items);
}