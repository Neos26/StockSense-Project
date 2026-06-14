using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<List<ProductDto>> GetByIdsAsync(List<int> ids);
    Task<bool> UpdateProductAsync(UpdateProductDto dto);
}
