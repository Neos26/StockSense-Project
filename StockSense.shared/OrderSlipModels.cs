using System.ComponentModel.DataAnnotations;

namespace StockSense.shared
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class OrderSlip
    {
        public int Id { get; set; } // Database Primary Key
        public string SlipNumber { get; set; } = string.Empty;
        public DateTime DateGenerated { get; set; } = DateTime.Now;

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        // This links to the line items table below
        public List<OrderSlipItem> Items { get; set; } = new();

        public bool IsReceived { get; set; } = false;
    }

    public class OrderSlipItem
    {
        public int Id { get; set; }
        public int OrderSlipId { get; set; } // Foreign Key linking back to OrderSlip

        // --- Product Snapshot Data ---
        // (We store these as strings/ints so if the original product is deleted, the historical receipt isn't ruined)
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderTarget { get; set; }

        // --- Core Order Data ---
        public int Quantity { get; set; } // The amount we are asking the supplier for
        public int ReceivedQuantity { get; set; } // The amount that actually arrived at the store

        // --- AI Intelligence Data ---
        public bool IsPredictedHighDemand { get; set; }
        public double ConfidenceScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
}