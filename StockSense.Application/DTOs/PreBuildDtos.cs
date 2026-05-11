namespace StockSense.Application.DTOs;

public class PreBuildPackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompatibleBrand { get; set; } = string.Empty;
    public string CompatibleModel { get; set; } = string.Empty;
    public string TargetCC { get; set; } = string.Empty;
    public int EstimatedAddedCC { get; set; }
    public bool IsActive { get; set; }
    public decimal TotalPrice { get; set; } 
    
    public List<PreBuildProductDto> IncludedProducts { get; set; } = new();
}

public class PreBuildProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CreatePreBuildDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompatibleBrand { get; set; } = string.Empty;
    public string CompatibleModel { get; set; } = string.Empty;
    public string TargetCC { get; set; } = string.Empty;
    public int EstimatedAddedCC { get; set; }
    
    public List<int> SelectedProductIds { get; set; } = new(); 
}