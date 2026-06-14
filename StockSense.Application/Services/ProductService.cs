using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Application.Mappings;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllProductsAsync();
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product?.ToDto();
    }

    public async Task<List<ProductDto>> GetByIdsAsync(List<int> ids)
    {
        var products = await _repository.GetByIdsAsync(ids);
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<bool> UpdateProductAsync(UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(dto.Id);
        if (product == null) return false;

        product.Price = dto.Price;
        product.ReorderTarget = dto.ReorderTarget;

        _repository.Update(product);
        await _repository.SaveChangesAsync();
        return true;
    }
}
