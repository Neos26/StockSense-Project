using StockSense.Domain.Entities;

namespace StockSense.Application.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
}