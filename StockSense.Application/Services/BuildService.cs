using System.Text.Json;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Application.Mappings;
using StockSense.Domain.Entities;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class BuildService : IBuildService
{
    private readonly IBuildRequestRepository _buildRepo;
    private readonly IProductRepository _productRepo;

    public BuildService(IBuildRequestRepository buildRepo, IProductRepository productRepo)
    {
        _buildRepo = buildRepo;
        _productRepo = productRepo;
    }

    public async Task<BuildRequestDto> CreateBuildAsync(CreateBuildRequestDto dto)
    {
        var request = new BuildRequest
        {
            CustomerName = dto.CustomerName,
            BuildName = dto.BuildName,
            SelectedPartsJson = dto.SelectedPartsJson,
            TotalPrice = dto.TotalPrice,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        _buildRepo.Add(request);
        await _buildRepo.SaveChangesAsync();
        return request.ToDto();
    }

    public async Task<List<BuildRequestDto>> GetAllBuildsAsync()
    {
        var builds = await _buildRepo.GetAllAsync();
        return builds.Select(b => b.ToDto()).ToList();
    }

    public async Task<List<BuildRequestDto>> GetCustomerBuildsAsync(string userName)
    {
        var builds = await _buildRepo.GetByCustomerNameAsync(userName);
        return builds.Select(b => b.ToDto()).ToList();
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatus)
    {
        var build = await _buildRepo.GetByIdAsync(id);
        if (build == null) return false;

        if (newStatus == "Completed" && build.Status != "Completed")
        {
            await DeductInventoryAsync(build);
        }

        build.Status = newStatus;
        await _buildRepo.SaveChangesAsync();
        return true;
    }

    private async Task DeductInventoryAsync(BuildRequest build)
    {
        if (string.IsNullOrEmpty(build.SelectedPartsJson)) return;

        try
        {
            var usedParts = JsonSerializer.Deserialize<List<BuildPartDto>>(
                build.SelectedPartsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (usedParts != null)
            {
                foreach (var part in usedParts)
                {
                    var dbProduct = await _productRepo.GetByIdAsync(part.Id);
                    if (dbProduct != null)
                    {
                        dbProduct.DeductStock(1);
                        _productRepo.Update(dbProduct);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deduct inventory: {ex.Message}");
        }
    }
}
