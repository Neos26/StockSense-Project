using StockSense.Domain.Entities;

namespace StockSense.Application.Interfaces;

public interface IOrderSlipService
{
    Task<List<OrderSlip>> GenerateSuggestedOrderSlipsAsync();
    Task<List<OrderSlip>> GenerateSingleProductSlipAsync(int productId);
    Task SaveOrderSlipToDbAsync(OrderSlip slip);
    Task<List<OrderSlip>> GetSavedOrderSlipsAsync();
    Task DeleteOrderSlipAsync(int id);
    Task RemoveItemFromSlipAsync(int itemId);
    Task MarkAsReceivedAsync(OrderSlip slip);
    Task<byte[]> GeneratePdfBytesAsync(OrderSlip slip);
    Task SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber);
}