using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Interfaces;
using StockSense.Domain.Entities;

namespace StockSense.Infrastructure.Data.Repositories;

public class MechanicRepository : IMechanicRepository
{
    private readonly ApplicationDbContext _context;

    public MechanicRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Mechanic>> GetAllAsync()
    {
        return await _context.Mechanics.ToListAsync();
    }

    public async Task<List<Mechanic>> GetActiveAsync()
    {
        return await _context.Mechanics.Where(m => m.IsActive).ToListAsync();
    }

    public async Task<Mechanic?> GetByIdAsync(int id)
    {
        return await _context.Mechanics.FindAsync(id);
    }

    public void Add(Mechanic mechanic)
    {
        _context.Mechanics.Add(mechanic);
    }

    public void Update(Mechanic mechanic)
    {
        _context.Mechanics.Update(mechanic);
    }

    public void Delete(Mechanic mechanic)
    {
        _context.Mechanics.Remove(mechanic);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
