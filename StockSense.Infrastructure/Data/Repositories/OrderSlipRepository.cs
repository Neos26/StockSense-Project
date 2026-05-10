using Microsoft.EntityFrameworkCore;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;
using StockSense.Infrastructure.Data;

namespace StockSense.Infrastructure.Data.Repositories;

public class OrderSlipRepository : IOrderSlipRepository
{
    private readonly ApplicationDbContext _context;

    public OrderSlipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .Where(p => p.CurrentStock < p.ReorderTarget)
            .ToListAsync();
    }

    public async Task<List<OrderSlip>> GetSavedSlipsAsync()
    {
        return await _context.OrderSlips
            .Include(s => s.Supplier)
            .Include(s => s.Items)
            .OrderByDescending(s => s.DateGenerated)
            .ToListAsync();
    }

    public async Task<OrderSlip?> GetSlipByIdAsync(int id)
    {
        return await _context.OrderSlips
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Product?> GetProductByNameAndBrandAsync(string name, string brand)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.Brand == brand);
    }

    public async Task AddSlipAsync(OrderSlip slip)
    {
        await _context.OrderSlips.AddAsync(slip);
    }

    public async Task UpdateSlipAsync(OrderSlip slip)
    {
        _context.OrderSlips.Update(slip);
        await Task.CompletedTask;
    }

    public async Task DeleteSlipAsync(int id)
    {
        var slip = await _context.OrderSlips.FindAsync(id);
        if (slip != null) _context.OrderSlips.Remove(slip);
    }

    public async Task RemoveItemAsync(int itemId)
    {
        var item = await _context.OrderSlipItems.FindAsync(itemId);
        if (item != null) _context.OrderSlipItems.Remove(item);
    }

    public async Task AddProductAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}