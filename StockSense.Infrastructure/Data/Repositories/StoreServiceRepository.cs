using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Interfaces;
using StockSense.Domain.Entities;

namespace StockSense.Infrastructure.Data.Repositories;

public class StoreServiceRepository : IStoreServiceRepository
{
    private readonly ApplicationDbContext _context;

    public StoreServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StoreService>> GetAllWithProductsAsync()
    {
        return await _context.StoreServices
            .Include(s => s.RequiredProducts)
            .ToListAsync();
    }

    public async Task<StoreService?> GetByIdWithProductsAsync(int id)
    {
        return await _context.StoreServices
            .Include(s => s.RequiredProducts)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
