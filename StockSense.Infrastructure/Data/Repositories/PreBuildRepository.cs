using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Entities;

namespace StockSense.Infrastructure.Data.Repositories;

public class PreBuildRepository
{
    private readonly ApplicationDbContext _context;

    public PreBuildRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PreBuildPackage>> GetAllAsync()
    {
        return await _context.PreBuildPackages
            .Include(p => p.IncludedProducts) 
            .ToListAsync();
    }

    public async Task<PreBuildPackage?> GetByIdAsync(int id)
    {
        return await _context.PreBuildPackages
            .Include(p => p.IncludedProducts)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(PreBuildPackage package)
    {
        _context.PreBuildPackages.Add(package);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PreBuildPackage package)
    {
        _context.PreBuildPackages.Update(package);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var package = await _context.PreBuildPackages.FindAsync(id);
        if (package != null)
        {
            _context.PreBuildPackages.Remove(package);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Product>> GetProductsByIdsAsync(List<int> productIds)
    {
        return await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();
    }
}