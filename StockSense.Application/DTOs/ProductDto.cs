namespace StockSense.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Category,
    string Brand,
    decimal Price,
    int CurrentStock,
    int ReorderTarget,
    int SupplierId,
    string SupplierName
);