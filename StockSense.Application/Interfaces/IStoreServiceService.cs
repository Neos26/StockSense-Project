using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IStoreServiceService
{
    Task<List<StoreServiceDto>> GetAllWithProductsAsync();
    Task<bool> UpdateServiceProductsAsync(UpdateServiceProductsDto dto);
}
