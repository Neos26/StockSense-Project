namespace StockSense.Application.DTOs;

public class UpdateProductDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int ReorderTarget { get; set; }
}
