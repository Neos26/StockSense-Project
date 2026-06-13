using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IOrderSlipRepository
{
    Task<List<OrderSlip>> GetSavedSlipsAsync();
    Task<OrderSlip?> GetSlipByIdAsync(int id);
    Task AddSlipAsync(OrderSlip slip);
    Task UpdateSlipAsync(OrderSlip slip);
    Task DeleteSlipAsync(int id);
    Task RemoveItemAsync(int itemId);
    Task<int> GetPendingCountAsync();
    Task SaveChangesAsync();
}
