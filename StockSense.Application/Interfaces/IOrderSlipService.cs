using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IOrderSlipService
{
    Task<int> GetPendingCountAsync();
    Task<List<OrderSlipDto>> GenerateSuggestedOrderSlipsAsync();
    Task<List<OrderSlipDto>> GenerateSingleProductSlipAsync(int productId);
    Task<List<OrderSlipDto>> GetSavedOrderSlipsAsync();

    Task<bool> SaveOrderSlipToDbAsync(SaveOrderSlipCommand command);
    Task<bool> MarkAsReceivedAsync(MarkAsReceivedCommand command);
    Task<byte[]> GeneratePdfBytesAsync(OrderSlipDto slip);
    Task ApplyProductToItemAsync(OrderSlipItemDto item, int productId);

    Task<bool> DeleteOrderSlipAsync(int id);
    Task<bool> RemoveItemFromSlipAsync(int itemId);
    Task<bool> SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber);
    Task<List<OrderSlipDto>?> GetPinnedSlipsAsync(string userId);
    Task SavePinnedSlipsAsync(string userId, List<OrderSlipDto> slips);
}
