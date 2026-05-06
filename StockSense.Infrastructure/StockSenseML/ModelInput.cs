using Microsoft.ML.Data;

namespace StockSenseML
{
    public class ModelInput
    {
        [ColumnName(@"ProductID"), LoadColumn(1)] // Changed from 0 to 1
        public string ProductID { get; set; } = string.Empty;

        [ColumnName(@"ProductName"), LoadColumn(2)] // Changed from 1 to 2
        public string ProductName { get; set; } = string.Empty;

        [ColumnName(@"Brand"), LoadColumn(3)] // Changed from 2 to 3
        public string Brand { get; set; } = string.Empty;

        [ColumnName(@"Category"), LoadColumn(4)] // Changed from 3 to 4
        public string Category { get; set; } = string.Empty;

        [ColumnName(@"QtySold"), LoadColumn(5)] // Changed from 6 to 5
        public float QtySold { get; set; }

        [ColumnName(@"MonthNum"), LoadColumn(6)] // Changed from 4 to 6
        public float MonthNum { get; set; }

        [ColumnName(@"Year"), LoadColumn(10)] // Changed from 5 to 10
        public float Year { get; set; }
    }
}