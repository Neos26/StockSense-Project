using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
}