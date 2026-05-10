namespace StockSense.Domain.Entities;

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