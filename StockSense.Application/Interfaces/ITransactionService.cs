using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface ITransactionService
{
    Task<ReceiptDto> ProcessSaleAsync(List<CartItemDto> items);
}
