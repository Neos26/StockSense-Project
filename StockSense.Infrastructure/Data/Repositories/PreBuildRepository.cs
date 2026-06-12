using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Interfaces;
using StockSense.Domain.Entities;

namespace StockSense.Infrastructure.Data.Repositories;

public class PreBuildRepository : IPreBuildRepository
{
    private readonly ApplicationDbContext _context;

    public PreBuildRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PreBuildPackage>> GetAllPackagesAsync()
    {
        return await _context.PreBuildPackages
            .Include(p => p.IncludedProducts) 
            .ToListAsync();
    }

    public async Task<PreBuildPackage?> GetPackageByIdAsync(int id)
    {
        return await _context.PreBuildPackages
            .Include(p => p.IncludedProducts)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddPackageAsync(PreBuildPackage package)
    {
        _context.PreBuildPackages.Add(package);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePackageAsync(PreBuildPackage package)
    {
        _context.PreBuildPackages.Update(package);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePackageAsync(int id)
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