using System;
using System.IO;
using Microsoft.ML;
using StockSense.Shared;
using StockSense;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceScopeFactory

namespace StockSense.Services
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

        // FIX: Inject ScopeFactory instead of DbContext
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

                ITransformer mlModel = _mlContext.Model.Load(modelPath, out var _);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<MLModel.ModelInput, MLModel.ModelOutput>(mlModel);
            }
        }

        public AIPredictionResult GetPredictiveOrderQty(Product product, int? overrideMonth = null, OrderStrategy strategy = OrderStrategy.Normal)
        {
            var resultObject = new AIPredictionResult();
            try
            {
                if (_predictionEngine == null) InitializeEngine();
                if (_predictionEngine == null) throw new Exception("AI Model not found.");

                int targetMonth = overrideMonth ?? DateTime.Now.Month;
                var input = new MLModel.ModelInput
                {
                    MonthNum = (float)targetMonth,
                    ProductID = product.Id.ToString(),
                    ProductName = product.Name,
                    Brand = product.Brand ?? "",
                    Category = product.Category ?? "",
                    QtySold = 0
                };

                MLModel.ModelOutput result;
                lock (_lock) { result = _predictionEngine.Predict(input); }

                float basePrediction = result.Score;
                if (float.IsNaN(basePrediction) || basePrediction < 0) basePrediction = 0;

                double deviation = Math.Abs(basePrediction - product.ReorderTarget);
                double confidence = Math.Max(45.0, Math.Min(98.0, 100.0 - (deviation * 5.0)));
                resultObject.ConfidenceScore = Math.Round(confidence, 1);

                if (strategy == OrderStrategy.Conservative) basePrediction *= 0.85f;
                else if (strategy == OrderStrategy.Aggressive) basePrediction *= 1.25f;

                int finalTarget = (int)Math.Ceiling(basePrediction);
                resultObject.PredictedDemand = finalTarget;
                resultObject.FinalOrderQty = Math.Max(0, finalTarget - product.CurrentStock);
                resultObject.Reasoning = resultObject.FinalOrderQty > 0 ? $"AI predicts {finalTarget} units needed." : "Stock sufficient.";

                return resultObject;
            }
            catch (Exception ex)
            {
                return new AIPredictionResult
                {
                    FinalOrderQty = Math.Max(0, (int)product.ReorderTarget - product.CurrentStock),
                    PredictedDemand = (int)product.ReorderTarget,
                    Reasoning = "AI Fallback active."
                };
            }
        }
    }
}