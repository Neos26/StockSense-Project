using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IPinnedSlipRepository
{
    Task<string?> GetSlipDataAsync(string userId);
    Task SetSlipDataAsync(string userId, string jsonData);
}
