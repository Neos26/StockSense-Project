using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StockSense.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty; // e.g., "Genuine Parts", "Racing Parts"
    public string Brand { get; set; } = string.Empty; // e.g., "Yamaha", "Honda"
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "https://placehold.co/300x200"; // Placeholder image
    // Add these to your REAL Product class if they are missing!
    public int CurrentStock { get; set; }
    public int ReorderTarget { get; set; }
    [JsonIgnore] public virtual ICollection<StoreService> StoreServices { get; set; } = new List<StoreService>();
    // This links the product to the Supplier class we just made
    public int SupplierId { get; set; }
    public virtual Supplier? Supplier { get; set; } = null!;
    [JsonIgnore] public List<PreBuildPackage> PreBuildPackages { get; set; } = new();
}