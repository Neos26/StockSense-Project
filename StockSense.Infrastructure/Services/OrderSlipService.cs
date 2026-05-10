using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;
using StockSense.Infrastructure.Data;
using MailKit.Net.Smtp;
using StockSense.Application.DTOs;
using StockSense.Application.Mappings;

namespace StockSense.Infrastructure.Services;

public class OrderSlipService : IOrderSlipService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public OrderSlipService(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<List<OrderSlipDto>> GenerateSuggestedOrderSlipsAsync()
    {
        var lowStockProducts = await _context.Products
            .Include(p => p.Supplier)
            .Where(p => p.CurrentStock < p.ReorderTarget)
            .ToListAsync();

        var generatedSlips = new List<OrderSlip>();
        var groupedBySupplier = lowStockProducts.GroupBy(p => p.SupplierId);

        int slipCounter = 1;
        foreach (var group in groupedBySupplier)
        {
            var supplier = group.First().Supplier;

            var slip = new OrderSlip
            {
                SlipNumber = $"ORD-{DateTime.Now.Year}-{slipCounter:D3}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
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
                    Quantity = Math.Max(p.ReorderTarget - p.CurrentStock, 5), 
                    IsPredictedHighDemand = false,
                    ConfidenceScore = 0,
                    Reasoning = "Manual reorder based on safety stock threshold."
                }).ToList()
            };

            generatedSlips.Add(slip);
            slipCounter++;
        }

        // Map the raw entities to DTOs before returning them to the UI
        return generatedSlips.Select(s => s.ToDto()).ToList();
    }

    public async Task<List<OrderSlipDto>> GenerateSingleProductSlipAsync(int productId)
    {
        var p = await _context.Products
            .Include(prod => prod.Supplier)
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (p == null) return new List<OrderSlipDto>();

        var slip = new OrderSlip
        {
            SlipNumber = $"ORD-SNGL-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
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

        // Map to DTO
        return new List<OrderSlipDto> { slip.ToDto() };
    }

    // --- DATABASE OPERATIONS ---
    public async Task SaveOrderSlipToDbAsync(OrderSlipDto slipDto)
    {
        if (slipDto.Id != 0) return;

        // Reverse map: We received a DTO from the UI, now we turn it back into an Entity to save it
        var newSlip = new OrderSlip
        {
            SlipNumber = slipDto.SlipNumber,
            DateGenerated = DateTime.Now,
            SupplierId = slipDto.SupplierId,
            IsReceived = false,
            Items = slipDto.Items.Select(i => new OrderSlipItem
            {
                ProductName = i.ProductName,
                Brand = i.Brand,
                Category = i.Category,
                CurrentStock = i.CurrentStock,
                ReorderTarget = i.ReorderTarget,
                Quantity = i.Quantity,
                ReceivedQuantity = i.ReceivedQuantity
            }).ToList()
        };

        _context.OrderSlips.Add(newSlip);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderSlipDto>> GetSavedOrderSlipsAsync()
    {
        var slips = await _context.OrderSlips
            .Include(s => s.Supplier)
            .Include(s => s.Items)
            .OrderByDescending(s => s.DateGenerated)
            .ToListAsync();

        // Convert the raw database entities to safe DTOs
        return slips.Select(s => s.ToDto()).ToList();
    }

    public async Task DeleteOrderSlipAsync(int id)
    {
        var slip = await _context.OrderSlips.FindAsync(id);
        if (slip != null)
        {
            _context.OrderSlips.Remove(slip);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveItemFromSlipAsync(int itemId)
    {
        var item = await _context.OrderSlipItems.FindAsync(itemId);
        if (item != null)
        {
            _context.OrderSlipItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    // --- INVENTORY MANAGEMENT: Stock Receipt ---
    public async Task MarkAsReceivedAsync(OrderSlipDto slipDto)
    {
        var dbSlip = await _context.OrderSlips
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == slipDto.Id);

        if (dbSlip == null || dbSlip.IsReceived) return;

        // Iterate over the DTO items sent from the UI
        foreach (var itemDto in slipDto.Items)
        {
            if (itemDto.ReceivedQuantity <= 0) continue;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Name == itemDto.ProductName && p.Brand == itemDto.Brand);

            if (product != null)
            {
                product.CurrentStock += itemDto.ReceivedQuantity;
            }
            else
            {
                var newProduct = new Product
                {
                    Name = itemDto.ProductName,
                    Brand = itemDto.Brand,
                    Category = !string.IsNullOrEmpty(itemDto.Category) ? itemDto.Category : "General",
                    CurrentStock = itemDto.ReceivedQuantity,
                    SupplierId = dbSlip.SupplierId,
                    Price = 0.00m,
                    ImageUrl = "https://placehold.co/300x200",
                    ReorderTarget = 10
                };
                _context.Products.Add(newProduct);
            }

            // Update the database item's received quantity to match the DTO
            var dbItem = dbSlip.Items.FirstOrDefault(i => i.Id == itemDto.Id);
            if (dbItem != null)
            {
                dbItem.ReceivedQuantity = itemDto.ReceivedQuantity;
            }
        }

        dbSlip.IsReceived = true;
        await _context.SaveChangesAsync();
    }

    // --- PDF GENERATION ---
    public async Task<byte[]> GeneratePdfBytesAsync(OrderSlipDto slipDto)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Inch);
                page.Header().Text($"Order Slip: {slipDto.SlipNumber}").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().Column(col =>
                {
                    // Notice how we use the flattened SupplierName from the DTO!
                    col.Item().Text($"Supplier: {slipDto.SupplierName}");
                    col.Item().Text($"Date: {slipDto.DateGenerated:MM/dd/yyyy}");
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns => {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });
                        table.Header(header => {
                            header.Cell().Text("Product");
                            header.Cell().Text("Quantity");
                        });
                        foreach (var item in slipDto.Items)
                        {
                            table.Cell().Text($"{item.ProductName} ({item.Brand})");
                            table.Cell().Text(item.Quantity.ToString());
                        }
                    });
                });
                page.Footer().AlignCenter().Text(x => {
                    x.Span("Generated by StockSense Inventory System - Page ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    // --- EMAIL FEATURE ---
    public async Task SendEmailAsync(string recipientEmail, byte[] pdfAttachment, string slipNumber)
    {
        // ... (This method doesn't use DTOs directly, so it stays exactly the same) ...
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("StockSense Admin", "admin@stocksense.com"));
        message.To.Add(new MailboxAddress("Supplier", recipientEmail));
        message.Subject = $"Purchase Order - {slipNumber}";

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
            <h3>New Order Request</h3>
            <p>Please find the attached order slip <strong>{slipNumber}</strong> for motor parts.</p>
            <p>Kindly review the quantities and notify us once the items are ready for delivery.</p>
            <br/>
            <p>Regards,<br/>StockSense System</p>"
        };

        builder.Attachments.Add($"Order_{slipNumber}.pdf", pdfAttachment);
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];

            int port = int.TryParse(portStr, out var p) ? p : 587;

            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);

            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SMTP Error: {ex.Message}");
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}