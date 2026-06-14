using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class MechanicService : IMechanicService
{
    private readonly IMechanicRepository _repo;

    public MechanicService(IMechanicRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<MechanicDto>> GetActiveAsync()
    {
        var mechanics = await _repo.GetActiveAsync();
        return mechanics.Select(MapToDto).ToList();
    }

    public async Task<List<MechanicDto>> GetAllAsync()
    {
        var mechanics = await _repo.GetAllAsync();
        return mechanics.Select(MapToDto).ToList();
    }

    public async Task<MechanicDto?> GetByIdAsync(int id)
    {
        var mechanic = await _repo.GetByIdAsync(id);
        return mechanic == null ? null : MapToDto(mechanic);
    }

    public async Task<MechanicDto> CreateAsync(MechanicDto dto)
    {
        var mechanic = new Mechanic
        {
            Name = dto.Name,
            IsActive = dto.IsActive
        };
        _repo.Add(mechanic);
        await _repo.SaveChangesAsync();
        return MapToDto(mechanic);
    }

    public async Task<bool> UpdateAsync(int id, MechanicDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Name = dto.Name;
        existing.IsActive = dto.IsActive;

        _repo.Update(existing);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var mechanic = await _repo.GetByIdAsync(id);
        if (mechanic == null) return false;

        _repo.Delete(mechanic);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static MechanicDto MapToDto(Mechanic m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        IsActive = m.IsActive
    };
}
