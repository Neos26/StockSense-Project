using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Application.Mappings;
using StockSense.Domain.Entities;

namespace StockSense.Application.Services;

public class OrderSlipService : IOrderSlipService
{
    private readonly IOrderSlipRepository _repo;
    private readonly IDocumentService _docService; 
    private readonly IOrderEmailSender _orderEmailSender;

    public OrderSlipService(IOrderSlipRepository repo, IDocumentService docService, IOrderEmailSender orderEmailSender)
    {
        _repo = repo;
        _docService = docService;
        _orderEmailSender = orderEmailSender;
    }

    public async Task<List<OrderSlipDto>> GenerateSuggestedOrderSlipsAsync()
    {
        var lowStockProducts = await _repo.GetLowStockProductsAsync();
        
        var generatedSlips = new List<OrderSlip>();
        var groupedBySupplier = lowStockProducts.GroupBy(p => p.SupplierId);

        int slipCounter = 1;
        foreach (var group in groupedBySupplier)
        {
            var supplier = group.First().Supplier;
            var slip = new OrderSlip
            {
                SlipNumber = $"ORD-{DateTime.Now.Year}-{slipCounter:D3}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                SupplierId = supplier.Id,
                Supplier = supplier,
                DateGenerated = DateTime.Now,
                Items = group.Select(p => new OrderSlipItem
                {
                    ProductName = p.Name,
                    Brand = p.Brand,
                    Quantity = Math.Max(p.ReorderTarget - p.CurrentStock, 5)
                }).ToList()
            };
            generatedSlips.Add(slip);
            slipCounter++;
        }

        return generatedSlips.Select(s => s.ToDto()).ToList();
    }

    public async Task SaveOrderSlipToDbAsync(OrderSlipDto slipDto)
    {
        var newSlip = new OrderSlip
        {
            SlipNumber = slipDto.SlipNumber,
            DateGenerated = DateTime.Now,
            SupplierId = slipDto.SupplierId,
            Items = slipDto.Items.Select(i => new OrderSlipItem
            {
                ProductName = i.ProductName,
                Brand = i.Brand,
                Quantity = i.Quantity
            }).ToList()
        };

        await _repo.AddSlipAsync(newSlip);
        await _repo.SaveChangesAsync();
    }

    public async Task<List<OrderSlipDto>> GetSavedOrderSlipsAsync()
    {
        var slips = await _repo.GetSavedSlipsAsync();
        return slips.Select(s => s.ToDto()).ToList();
    }

    public async Task MarkAsReceivedAsync(OrderSlipDto slipDto)
    {
        var dbSlip = await _repo.GetSlipByIdAsync(slipDto.Id);
        if (dbSlip == null || dbSlip.IsReceived) return;

        foreach (var itemDto in slipDto.Items)
        {
            if (itemDto.ReceivedQuantity <= 0) continue;

            var product = await _repo.GetProductByNameAndBrandAsync(itemDto.ProductName, itemDto.Brand);
            if (product != null)
            {
                product.CurrentStock += itemDto.ReceivedQuantity;
            }
            
            var dbItem = dbSlip.Items.FirstOrDefault(i => i.Id == itemDto.Id);
            if (dbItem != null) dbItem.ReceivedQuantity = itemDto.ReceivedQuantity;
        }

        dbSlip.IsReceived = true;
        await _repo.UpdateSlipAsync(dbSlip);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteOrderSlipAsync(int id)
    {
        await _repo.DeleteSlipAsync(id);
        await _repo.SaveChangesAsync();
    }

    public async Task RemoveItemFromSlipAsync(int itemId)
    {
        await _repo.RemoveItemAsync(itemId);
        await _repo.SaveChangesAsync();
    }

    public async Task<List<OrderSlipDto>> GenerateSingleProductSlipAsync(int productId)
    {
        // Fetch the single product using the repo
        var p = await _repo.GetProductByIdAsync(productId);
        
        if (p == null) return new List<OrderSlipDto>();

        var slip = new OrderSlip
        {
            SlipNumber = $"ORD-SNGL-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            SupplierId = p.SupplierId,
            Supplier = p.Supplier,
            DateGenerated = DateTime.Now,
            Items = new List<OrderSlipItem> {
                new OrderSlipItem {
                    ProductName = p.Name,
                    Brand = p.Brand,
                    Quantity = Math.Max(p.ReorderTarget - p.CurrentStock, 10),
                    Reasoning = "On-demand single product reorder."
                }
            }
        };

        return new List<OrderSlipDto> { slip.ToDto() };
    }

    // --- DELEGATED INFRASTRUCTURE CALLS ---

    public async Task<byte[]> GeneratePdfBytesAsync(OrderSlipDto slipDto)
    {
        return await Task.FromResult(_docService.GenerateOrderSlipPdf(slipDto));
    }

    public async Task SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber)
    {
        string subject = $"Purchase Order - {slipNumber}";
        string body = $@"
            <h3>New Order Request</h3>
            <p>Please find the attached order slip <strong>{slipNumber}</strong> for motor parts.</p>
            <p>Kindly review the quantities and notify us once the items are ready for delivery.</p>
            <br/>
            <p>Regards,<br/>StockSense System</p>";
        string fileName = $"Order_{slipNumber}.pdf";

        // Fixed the method call to pass all required arguments
        await _orderEmailSender.SendEmailWithAttachmentAsync(recipientEmail, subject, body, pdfAttachment, fileName);
    }
}