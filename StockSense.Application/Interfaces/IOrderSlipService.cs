using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IOrderSlipService
{
    Task<List<OrderSlipDto>> GenerateSuggestedOrderSlipsAsync();
    Task<List<OrderSlipDto>> GenerateSingleProductSlipAsync(int productId);
    Task<List<OrderSlipDto>> GetSavedOrderSlipsAsync();

    Task SaveOrderSlipToDbAsync(SaveOrderSlipCommand command);
    Task MarkAsReceivedAsync(MarkAsReceivedCommand command);
    Task<byte[]> GeneratePdfBytesAsync(OrderSlipDto slip);
    Task ApplyProductToItemAsync(OrderSlipItemDto item, int productId);

    Task DeleteOrderSlipAsync(int id);
    Task RemoveItemFromSlipAsync(int itemId);
    Task SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber);
}
