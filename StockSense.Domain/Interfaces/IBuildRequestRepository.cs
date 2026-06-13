using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IBuildRequestRepository
{
    Task<List<BuildRequest>> GetAllAsync();
    Task<BuildRequest?> GetByIdAsync(int id);
    Task<List<BuildRequest>> GetByCustomerNameAsync(string customerName);
    void Add(BuildRequest request);
    Task SaveChangesAsync();
}
