using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StockSense.Domain.Entities
{
    public class PreBuildPackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // The specific motor specs this package is built for
        public string CompatibleBrand { get; set; } = string.Empty;
        public string CompatibleModel { get; set; } = string.Empty;
        public string TargetCC { get; set; } = string.Empty;

        public int EstimatedAddedCC { get; set; }
        public bool IsActive { get; set; } = true;

        // The list of actual products included in this build
        public List<Product> IncludedProducts { get; set; } = new();

        // We use [NotMapped] so Entity Framework doesn't try to create a column for this.
        // It will just calculate it on the fly based on the included products!
        [NotMapped]
        public decimal TotalPrice => IncludedProducts.Sum(p => p.Price);
    }
}
