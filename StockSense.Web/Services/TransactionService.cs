using Microsoft.EntityFrameworkCore;
using StockSense.Domain.Entities;
using StockSense.Web.Data;

// Add this if your Transaction, TransactionItem, and SalesHistory models are in a different namespace
// using StockSense.Models; 

namespace StockSense.Web.Services
{
    public class TransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Changed from Task to Task<Transaction> so the POS UI can get the receipt data back
        public async Task<Transaction> ProcessSaleAsync(List<CartItem> items)
        {
            // 1. Generate the Receipt Header
            var receipt = new Transaction
            {
                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                TransactionDate = DateTime.Now,
                TotalAmount = items.Sum(i => i.Price * i.Quantity),
                Items = new List<TransactionItem>()
            };

            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // Deduct Stock
                    product.CurrentStock -= item.Quantity;

                    // 2. Add to your existing SalesHistory (For AI)
                    _context.SalesHistory.Add(new SalesHistory
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd"),
                        ProductID = product.Id.ToString(),
                        ProductName = product.Name,
                        Brand = product.Brand,
                        Category = product.Category,
                        QtySold = (float)item.Quantity,
                        UnitPrice = (float)product.Price,
                        TotalSales = (float)(item.Quantity * product.Price),
                        MonthNum = (float)DateTime.Now.Month
                        // ... add your other SalesHistory fields here
                    });

                    // 3. Add to the Receipt Details (For the PDF/Print)
                    receipt.Items.Add(new TransactionItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = item.Quantity
                    });
                }
            }

            // Save the Receipt
            _context.Transactions.Add(receipt);

            // Save everything (Stock updates, SalesHistory, and Receipt) at once
            await _context.SaveChangesAsync();

            // Return the receipt so the POS screen can print it
            return receipt;
        }
    }
}