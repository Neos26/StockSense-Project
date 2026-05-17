using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

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
        
        // Pass the variables inside ( ) instead of { }
        // Make sure the order here matches the order in your ProductDto!
        return products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Brand,
            p.Category,
            p.Price,
            p.CurrentStock,
            p.ReorderTarget,
            p.SupplierId,
            p.Supplier?.Name ?? "No Supplier Assigned"
        )).ToList();
    }
}
