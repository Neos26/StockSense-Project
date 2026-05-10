using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IOrderSlipService
{
    Task<List<OrderSlipDto>> GenerateSuggestedOrderSlipsAsync();
    Task<List<OrderSlipDto>> GenerateSingleProductSlipAsync(int productId);
    Task<List<OrderSlipDto>> GetSavedOrderSlipsAsync();
    
    // Note: Methods that TAKE an OrderSlip need to be updated too!
    Task SaveOrderSlipToDbAsync(OrderSlipDto slip);
    Task MarkAsReceivedAsync(OrderSlipDto slip);
    Task<byte[]> GeneratePdfBytesAsync(OrderSlipDto slip);
    
    Task DeleteOrderSlipAsync(int id);
    Task RemoveItemFromSlipAsync(int itemId);
    Task SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber);
}