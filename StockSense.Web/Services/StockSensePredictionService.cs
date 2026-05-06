using System;
using System.IO;
using Microsoft.ML;
using StockSense.Domain.Entities;
using StockSense.Web;
using Microsoft.Extensions.DependencyInjection;

namespace StockSense.Web.Services
{
    public enum OrderStrategy { Conservative, Normal, Aggressive }

    public class AIPredictionResult
    {
        public int FinalOrderQty { get; set; }
        public int PredictedDemand { get; set; }
        public double ConfidenceScore { get; set; }
        public string Reasoning { get; set; } = "";
    }

    public class StockSensePredictionService
    {
        private readonly MLContext _mlContext = new MLContext();
        private PredictionEngine<MLModel.ModelInput, MLModel.ModelOutput>? _predictionEngine;
        private readonly object _lock = new object();

        public StockSensePredictionService() { }

        private string GetModelPath()
        {
            if (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null)
            {
                var azurePath = @"D:\home\data\MLModel.mlnet";
                if (!Directory.Exists(@"D:\home\data")) Directory.CreateDirectory(@"D:\home\data");
                return azurePath;
            }
            return Path.Combine(AppContext.BaseDirectory, "MLModel.mlnet");
        }

        private void InitializeEngine()
        {
            lock (_lock)
            {
                if (_predictionEngine != null) return;

                string modelPath = GetModelPath();
                if (!File.Exists(modelPath))
                    modelPath = Path.Combine(AppContext.BaseDirectory, "MLModel.mlnet");

                if (!File.Exists(modelPath)) return;

                // Load the model once into memory
                ITransformer mlModel = _mlContext.Model.Load(modelPath, out var _);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<MLModel.ModelInput, MLModel.ModelOutput>(mlModel);
            }
        }

        public AIPredictionResult GetPredictiveOrderQty(Product product, int? overrideMonth = null, int? overrideYear = null, OrderStrategy strategy = OrderStrategy.Normal)
        {
            var resultObject = new AIPredictionResult();
            try
            {
                if (_predictionEngine == null) InitializeEngine();
                if (_predictionEngine == null) throw new Exception("Model not found.");

                // Timeframe Logic: Default to current date if not specified
                int targetMonth = overrideMonth ?? DateTime.Now.Month;
                int targetYear = overrideYear ?? DateTime.Now.Year;

                var input = new MLModel.ModelInput
                {
                    ProductID = product.Id.ToString(),
                    ProductName = product.Name,
                    Brand = product.Brand ?? "",
                    Category = product.Category ?? "",
                    MonthNum = (float)targetMonth,
                    Year = (float)targetYear,
                    QtySold = 0 // Required placeholder
                };

                MLModel.ModelOutput result;
                // Thread-safe prediction call
                lock (_lock) { result = _predictionEngine.Predict(input); }

                float basePrediction = result.Score;
                if (float.IsNaN(basePrediction) || basePrediction < 0) basePrediction = 0;

                // Confidence logic based on manual reorder target deviation
                double deviation = Math.Abs(basePrediction - product.ReorderTarget);
                double confidence = Math.Max(45.0, Math.Min(98.0, 100.0 - (deviation * 5.0)));
                resultObject.ConfidenceScore = Math.Round(confidence, 1);

                // Apply Strategy Multipliers
                if (strategy == OrderStrategy.Conservative) basePrediction *= 0.85f;
                else if (strategy == OrderStrategy.Aggressive) basePrediction *= 1.25f;

                int finalTarget = (int)Math.Ceiling(basePrediction);
                resultObject.PredictedDemand = finalTarget;
                resultObject.FinalOrderQty = Math.Max(0, finalTarget - product.CurrentStock);

                // Dynamic reasoning including the timeframe
                string monthName = new DateTime(2000, targetMonth, 1).ToString("MMMM");
                resultObject.Reasoning = resultObject.FinalOrderQty > 0
                    ? $"Aim for {finalTarget} units for {monthName} {targetYear}."
                    : $"Healthy for {monthName} {targetYear}.";

                return resultObject;
            }
            catch (Exception)
            {
                return new AIPredictionResult
                {
                    FinalOrderQty = Math.Max(0, (int)product.ReorderTarget - product.CurrentStock),
                    PredictedDemand = (int)product.ReorderTarget,
                    Reasoning = "Fallback: Using manual reorder targets."
                };
            }
        }
    }
}