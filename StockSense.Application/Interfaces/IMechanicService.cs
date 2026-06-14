using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IMechanicService
{
    Task<List<MechanicDto>> GetActiveAsync();
    Task<List<MechanicDto>> GetAllAsync();
    Task<MechanicDto?> GetByIdAsync(int id);
    Task<MechanicDto> CreateAsync(MechanicDto dto);
    Task<bool> UpdateAsync(int id, MechanicDto dto);
    Task<bool> DeleteAsync(int id);
}
