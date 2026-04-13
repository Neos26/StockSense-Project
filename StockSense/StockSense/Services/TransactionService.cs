using Microsoft.EntityFrameworkCore;
using StockSense.Data; // Ensure this matches your namespace for ApplicationDbContext
using StockSense.shared; // Ensure this matches where your CartItem class is

namespace StockSense.Services
{
    public class TransactionService
    {
        private readonly ApplicationDbContext _context;

        // Constructor: This is how we get access to the database (_context)
        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ProcessSaleAsync(List<CartItem> items)
        {
            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product != null)
                {
                    if (product.CurrentStock < item.Quantity)
                        throw new Exception($"Insufficient stock for {product.Name}");

                    product.CurrentStock -= item.Quantity;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
