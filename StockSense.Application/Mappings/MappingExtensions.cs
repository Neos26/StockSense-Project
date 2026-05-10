using StockSense.Domain.Entities;
using StockSense.Application.DTOs;

namespace StockSense.Application.Mappings;

public static class MappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Category,
            product.Brand,
            product.Price,
            product.CurrentStock,
            product.ReorderTarget,
            product.Supplier?.Name ?? "No Supplier Assigned"
        );
    }

    public static SupplierDto ToDto(this Supplier supplier)
    {
        return new SupplierDto(
            supplier.Id,
            supplier.Name,
            supplier.Email ?? ""
        );
    }

    public static OrderSlipItemDto ToDto(this OrderSlipItem item)
    {
        return new OrderSlipItemDto
        {
            Id = item.Id,
            ProductName = item.ProductName,
            Brand = item.Brand,
            Category = item.Category ?? "Uncategorized",
            CurrentStock = item.CurrentStock,
            ReorderTarget = item.ReorderTarget,
            Quantity = item.Quantity,
            ReceivedQuantity = item.ReceivedQuantity
        };
    }

    public static OrderSlipDto ToDto(this OrderSlip slip)
    {
        return new OrderSlipDto
        {
            Id = slip.Id,
            SlipNumber = slip.SlipNumber,
            DateGenerated = slip.DateGenerated,
            SupplierId = slip.SupplierId,
            SupplierName = slip.Supplier?.Name ?? "Unknown Supplier",
            SupplierEmail = slip.Supplier?.Email ?? "",
            IsReceived = slip.IsReceived,
            Items = slip.Items.Select(item => item.ToDto()).ToList()
        };
    }
}