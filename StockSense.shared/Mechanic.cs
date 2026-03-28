namespace StockSense.shared
{
    public class Mechanic
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // This allows you to "hide" a mechanic if they quit or go on vacation 
        // without deleting their past appointment history!
        public bool IsActive { get; set; } = true;
    }
}
