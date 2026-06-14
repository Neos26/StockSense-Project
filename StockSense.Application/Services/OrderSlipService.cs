using System.Text.Json;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Application.Mappings;
using StockSense.Domain.Entities;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class OrderSlipService : IOrderSlipService
{
    private readonly IOrderSlipRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IDocumentService _docService;
    private readonly IOrderEmailSender _orderEmailSender;
    private readonly IPinnedSlipRepository _pinnedRepo;

    public OrderSlipService(
        IOrderSlipRepository repo,
        IProductRepository productRepo,
        IDocumentService docService,
        IOrderEmailSender orderEmailSender,
        IPinnedSlipRepository pinnedRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _docService = docService;
        _orderEmailSender = orderEmailSender;
        _pinnedRepo = pinnedRepo;
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _repo.GetPendingCountAsync();
    }

    public async Task<List<OrderSlipDto>> GenerateSuggestedOrderSlipsAsync()
    {
        var products = await _productRepo.GetAllProductsAsync();
        var lowStockProducts = products.Where(p => p.CurrentStock < p.ReorderTarget).ToList();

        var generatedSlips = new List<OrderSlip>();
        var groupedBySupplier = lowStockProducts.GroupBy(p => p.SupplierId);

        int slipCounter = 1;
        foreach (var group in groupedBySupplier)
        {
            var supplier = group.First().Supplier;
            if (supplier == null) continue;
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
                    Category = p.Category,
                    CurrentStock = p.CurrentStock,
                    ReorderTarget = p.ReorderTarget,
                    Quantity = Math.Max(p.ReorderTarget - p.CurrentStock, 5)
                }).ToList()
            };
            generatedSlips.Add(slip);
            slipCounter++;
        }

        return generatedSlips.Select(s => s.ToDto()).ToList();
    }

    public async Task<bool> SaveOrderSlipToDbAsync(SaveOrderSlipCommand command)
    {
        var supplierId = command.SupplierId;
        if (supplierId <= 0)
        {
            var allProducts = await _productRepo.GetAllProductsAsync();
            var matched = allProducts.FirstOrDefault(p =>
                command.Items.Any(i => i.ProductName == p.Name && i.Brand == p.Brand));
            if (matched?.SupplierId > 0)
                supplierId = matched.SupplierId;
        }

        var newSlip = new OrderSlip
        {
            SlipNumber = command.SlipNumber,
            DateGenerated = DateTime.Now,
            SupplierId = supplierId,
            Items = command.Items.Select(i => new OrderSlipItem
            {
                ProductName = i.ProductName,
                Brand = i.Brand,
                Quantity = i.Quantity
            }).ToList()
        };

        await _repo.AddSlipAsync(newSlip);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<List<OrderSlipDto>> GetSavedOrderSlipsAsync()
    {
        var slips = await _repo.GetSavedSlipsAsync();
        return slips.Select(s => s.ToDto()).ToList();
    }

    public async Task<bool> MarkAsReceivedAsync(MarkAsReceivedCommand command)
    {
        var dbSlip = await _repo.GetSlipByIdAsync(command.SlipId);
        if (dbSlip == null || dbSlip.IsReceived) return false;

        foreach (var itemCmd in command.Items)
        {
            if (itemCmd.ReceivedQuantity <= 0) continue;

            var dtoItem = dbSlip.Items.FirstOrDefault(i => i.Id == itemCmd.ItemId);
            if (dtoItem == null) continue;

            var product = (await _productRepo.GetAllProductsAsync())
                .FirstOrDefault(p => p.Name == dtoItem.ProductName && p.Brand == dtoItem.Brand);
            if (product != null)
            {
                product.AddStock(itemCmd.ReceivedQuantity);
            }

            dbSlip.ReceiveItem(itemCmd.ItemId, itemCmd.ReceivedQuantity);
        }

        dbSlip.MarkAsReceived();
        await _repo.UpdateSlipAsync(dbSlip);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrderSlipAsync(int id)
    {
        var slip = await _repo.GetSlipByIdAsync(id);
        if (slip == null) return false;
        await _repo.DeleteSlipAsync(id);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemFromSlipAsync(int itemId)
    {
        await _repo.RemoveItemAsync(itemId);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<List<OrderSlipDto>> GenerateSingleProductSlipAsync(int productId)
    {
        var allProducts = await _productRepo.GetAllProductsAsync();
        var p = allProducts.FirstOrDefault(pr => pr.Id == productId);

        if (p == null) return new List<OrderSlipDto>();

        var slip = new OrderSlip
        {
            SlipNumber = $"ORD-SNGL-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            SupplierId = p.SupplierId,
            Supplier = p.Supplier!,
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

    public async Task<byte[]> GeneratePdfBytesAsync(OrderSlipDto slipDto)
    {
        return await Task.FromResult(_docService.GenerateOrderSlipPdf(slipDto));
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber)
    {
        string subject = $"Purchase Order - {slipNumber}";
        string body = $@"
            <h3>New Order Request</h3>
            <p>Please find the attached order slip <strong>{slipNumber}</strong> for motor parts.</p>
            <p>Kindly review the quantities and notify us once the items are ready for delivery.</p>
            <br/>
            <p>Regards,<br/>StockSense System</p>";
        string fileName = $"Order_{slipNumber}.pdf";

        await _orderEmailSender.SendEmailWithAttachmentAsync(recipientEmail, subject, body, pdfAttachment, fileName);
        return true;
    }

    public async Task ApplyProductToItemAsync(OrderSlipItemDto item, int productId)
    {
        var products = await _productRepo.GetAllProductsAsync();
        var product = products.FirstOrDefault(p => p.Id == productId);
        if (product == null) return;

        item.ProductName = product.Name;
        item.Brand = product.Brand;
        item.Category = product.Category;
        item.CurrentStock = product.CurrentStock;
        item.ReorderTarget = product.ReorderTarget;
        if (item.Quantity == 0) item.Quantity = product.ReorderTarget;
    }

    public async Task<List<OrderSlipDto>?> GetPinnedSlipsAsync(string userId)
    {
        var json = await _pinnedRepo.GetSlipDataAsync(userId);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<List<OrderSlipDto>>(json);
    }

    public async Task SavePinnedSlipsAsync(string userId, List<OrderSlipDto> slips)
    {
        var json = JsonSerializer.Serialize(slips);
        await _pinnedRepo.SetSlipDataAsync(userId, json);
    }
}
