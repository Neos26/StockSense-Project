using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IMechanicRepository
{
    Task<List<Mechanic>> GetAllAsync();
    Task<List<Mechanic>> GetActiveAsync();
    Task<Mechanic?> GetByIdAsync(int id);
    void Add(Mechanic mechanic);
    void Update(Mechanic mechanic);
    void Delete(Mechanic mechanic);
    Task SaveChangesAsync();
}
