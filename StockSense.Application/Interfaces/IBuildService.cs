using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IBuildService
{
    Task<BuildRequestDto> CreateBuildAsync(CreateBuildRequestDto dto);
    Task<List<BuildRequestDto>> GetAllBuildsAsync();
    Task<List<BuildRequestDto>> GetCustomerBuildsAsync(string userName);
    Task<bool> UpdateStatusAsync(int id, string newStatus);
}
