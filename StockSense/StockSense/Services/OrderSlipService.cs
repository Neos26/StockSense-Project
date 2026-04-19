using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StockSense.Data;
using StockSense.shared; // Your cleaned-up models
using StockSense.Shared;

namespace StockSense.Services;

public class OrderSlipService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly StockSensePredictionService _predictionService;

    public OrderSlipService(ApplicationDbContext context, IConfiguration config, StockSensePredictionService predictionService)
    {
        _context = context;
        _config = config;
        _predictionService = predictionService;
    }

    // --- DASHBOARD FEATURE: Order All Low Stock Items ---
    public async Task<List<OrderSlip>> GenerateSuggestedOrderSlipsAsync()
    {
        // Get products where stock has fallen below the safety buffer (ReorderTarget)
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
                Items = group.Select(p =>
                {
                    // 1. Ask the AI for the raw demand forecast
                    var ai = _predictionService.GetPredictiveOrderQty(p);

                    // 2. THE CORRECT INVENTORY FORMULA:
                    // (How many people will buy + The minimum safety buffer we want) - What we already have on the shelf
                    int calculatedQty = (ai.PredictedDemand + p.ReorderTarget) - p.CurrentStock;

                    // 3. Ensure we never order a negative amount
                    int finalQty = Math.Max(calculatedQty, 0);

                    // 4. Fallback: If AI returns 0 or fails, use the standard fallback calculation
                    if (ai.PredictedDemand <= 0)
                    {
                        finalQty = Math.Max(p.ReorderTarget - p.CurrentStock, 0);
                        if (finalQty == 0) finalQty = 5; // Hard fallback if math zeros out but stock is low
                    }

                    return new OrderSlipItem
                    {
                        ProductName = p.Name,
                        Brand = p.Brand,
                        Category = p.Category,
                        CurrentStock = p.CurrentStock,
                        ReorderTarget = p.ReorderTarget,

                        // Apply the exact formula output
                        Quantity = finalQty,

                        // Save the AI tracking data so we can see it on the frontend
                        IsPredictedHighDemand = ai.ConfidenceScore > 75 && ai.PredictedDemand > p.ReorderTarget,
                        ConfidenceScore = ai.ConfidenceScore,
                        Reasoning = $"SS predicts {ai.PredictedDemand} units of demand. Formula applied: ({ai.PredictedDemand} Demand + {p.ReorderTarget} Safety) - {p.CurrentStock} Current."
                    };
                }).ToList()
            };

            generatedSlips.Add(slip);
            slipCounter++;
        }

        return generatedSlips;
    }

    // --- DASHBOARD FEATURE: Order a Specific Product from Alert ---
    public async Task<List<OrderSlip>> GenerateSingleProductSlipAsync(int productId)
    {
        var p = await _context.Products
            .Include(prod => prod.Supplier)
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (p == null) return new List<OrderSlip>();

        var ai = _predictionService.GetPredictiveOrderQty(p);

        // Apply the same correct formula here
        int calculatedQty = (ai.PredictedDemand + p.ReorderTarget) - p.CurrentStock;
        int finalQty = Math.Max(calculatedQty, 0);

        if (ai.PredictedDemand <= 0)
        {
            finalQty = Math.Max(p.ReorderTarget - p.CurrentStock, 0);
            if (finalQty == 0) finalQty = 10;
        }

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
                    Category = p.Category,
                    CurrentStock = p.CurrentStock,
                    ReorderTarget = p.ReorderTarget,
                    Quantity = finalQty,
                    IsPredictedHighDemand = ai.ConfidenceScore > 75 && ai.PredictedDemand > p.ReorderTarget,
                    ConfidenceScore = ai.ConfidenceScore,
                    Reasoning = $"SS predicts {ai.PredictedDemand} units of demand. Formula applied: ({ai.PredictedDemand} Demand + {p.ReorderTarget} Safety) - {p.CurrentStock} Current."
                }
            }
        };

        return new List<OrderSlip> { slip };
    }

    // --- DATABASE OPERATIONS ---
    public async Task SaveOrderSlipToDbAsync(OrderSlip slip)
    {
        if (slip.Id != 0) return;

        slip.DateGenerated = DateTime.Now;
        slip.IsReceived = false;

        var supplier = slip.Supplier;
        slip.Supplier = null!;

        _context.OrderSlips.Add(slip);
        await _context.SaveChangesAsync();

        slip.Supplier = supplier;
    }

    public async Task<List<OrderSlip>> GetSavedOrderSlipsAsync()
    {
        return await _context.OrderSlips
            .Include(s => s.Supplier)
            .Include(s => s.Items)
            .OrderByDescending(s => s.DateGenerated)
            .ToListAsync();
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
    public async Task MarkAsReceivedAsync(OrderSlip slip)
    {
        var dbSlip = await _context.OrderSlips
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == slip.Id);

        if (dbSlip == null || dbSlip.IsReceived) return;

        foreach (var item in dbSlip.Items)
        {
            if (item.ReceivedQuantity <= 0) continue;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Name == item.ProductName && p.Brand == item.Brand);

            if (product != null)
            {
                product.CurrentStock += item.ReceivedQuantity;
            }
            else
            {
                var newProduct = new Product
                {
                    Name = item.ProductName,
                    Brand = item.Brand,
                    Category = !string.IsNullOrEmpty(item.Category) ? item.Category : "General",
                    CurrentStock = item.ReceivedQuantity,
                    SupplierId = dbSlip.SupplierId,
                    Price = 0.00m,
                    ImageUrl = "https://placehold.co/300x200",
                    ReorderTarget = 10
                };
                _context.Products.Add(newProduct);
            }
        }

        dbSlip.IsReceived = true;
        await _context.SaveChangesAsync();
    }

    // --- PDF GENERATION ---
    public async Task<byte[]> GeneratePdfBytesAsync(OrderSlip slip)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Inch);
                page.Header().Text($"Order Slip: {slip.SlipNumber}").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Supplier: {slip.Supplier?.Name}");
                    col.Item().Text($"Date: {slip.DateGenerated:MM/dd/yyyy}");
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
                        foreach (var item in slip.Items)
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