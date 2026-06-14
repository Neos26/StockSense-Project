using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Application.Mappings;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class StoreServiceService : IStoreServiceService
{
    private readonly IStoreServiceRepository _serviceRepo;
    private readonly IProductRepository _productRepo;

    public StoreServiceService(IStoreServiceRepository serviceRepo, IProductRepository productRepo)
    {
        _serviceRepo = serviceRepo;
        _productRepo = productRepo;
    }

    public async Task<List<StoreServiceDto>> GetAllWithProductsAsync()
    {
        var services = await _serviceRepo.GetAllWithProductsAsync();
        return services.Select(s => new StoreServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Price = s.Price,
            Category = s.Category,
            EstimatedMinutes = s.EstimatedMinutes,
            Status = s.Status,
            RequiredProducts = s.RequiredProducts.Select(p => p.ToDto()).ToList()
        }).ToList();
    }

    public async Task<bool> UpdateServiceProductsAsync(UpdateServiceProductsDto dto)
    {
        var service = await _serviceRepo.GetByIdWithProductsAsync(dto.ServiceId);
        if (service == null) return false;

        var selectedProducts = await _productRepo.GetByIdsAsync(dto.ProductIds);
        service.RequiredProducts = selectedProducts;
        await _serviceRepo.SaveChangesAsync();
        return true;
    }
}
