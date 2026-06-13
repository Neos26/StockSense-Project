namespace StockSense.Application.Interfaces;

public interface IBuildService
{
    Task<bool> UpdateStatusAsync(int id, string newStatus);
}
