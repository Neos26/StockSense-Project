using System.ComponentModel.DataAnnotations;

namespace StockSense.shared
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class OrderItem
    {
        public int Id { get; set; } // Database Primary Key
        public int OrderSlipId { get; set; } // Foreign Key

        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderTarget { get; set; }
        public bool IsPredictedHighDemand { get; set; }

        // This is now a real column that stores the result of the math
        public int Quantity { get; set; }
    }

    public class OrderSlip
    {
        public int Id { get; set; } // Database Primary Key
        public string SlipNumber { get; set; } = string.Empty;
        public DateTime DateGenerated { get; set; } = DateTime.Now;

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        // This links to the line items table
        public List<OrderSlipItem> Items { get; set; } = new();

        public bool IsReceived { get; set; } = false;
    }


    public class OrderSlipItem
    {
        public int Id { get; set; }
        public int OrderSlipId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string? Category { get; set; }

        // Add these two lines if they are missing!
        public int CurrentStock { get; set; }
        public int ReorderTarget { get; set; }

        public int Quantity { get; set; }

        public int ReceivedQuantity { get; set; }
        public bool IsPredictedHighDemand { get; set; }
    }
}
