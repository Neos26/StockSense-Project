using System.Text.Json;
using StockSense.Application.Interfaces;
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
            var usedParts = JsonSerializer.Deserialize<List<Product>>(
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
