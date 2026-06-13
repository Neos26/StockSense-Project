namespace StockSense.Application.DTOs;

public class CreateBuildRequestDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string BuildName { get; set; } = "Custom Build";
    public string SelectedPartsJson { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}
