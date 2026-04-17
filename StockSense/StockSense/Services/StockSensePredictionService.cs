using System;
using System.IO;
using Microsoft.ML;
using StockSense.Shared;
using StockSense;

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
        // PERFORMANCE FIX: Reuse the MLContext and PredictionEngine
        private readonly MLContext _mlContext = new MLContext();
        private PredictionEngine<MLModel.ModelInput, MLModel.ModelOutput> _predictionEngine;
        private DateTime _lastModelLoadTime;

        private string GetModelPath()
        {
            if (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null)
            {
                var azurePath = @"D:\home\data\MLModel.mlnet";
                // Only create the directory if it's missing (rare check)
                if (!Directory.Exists(@"D:\home\data")) Directory.CreateDirectory(@"D:\home\data");
                return azurePath;
            }
            return Path.Combine(AppContext.BaseDirectory, "MLModel.mlnet");
        }

        private void InitializeEngine()
        {
            string modelPath = GetModelPath();

            // Fallback to deployment folder if retrained model doesn't exist yet
            if (!File.Exists(modelPath))
                modelPath = Path.Combine(AppContext.BaseDirectory, "MLModel.mlnet");

            if (!File.Exists(modelPath)) return;

            // Load model and create engine (only if not already loaded)
            ITransformer mlModel = _mlContext.Model.Load(modelPath, out var _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<MLModel.ModelInput, MLModel.ModelOutput>(mlModel);
            _lastModelLoadTime = DateTime.Now;
        }

        public AIPredictionResult GetPredictiveOrderQty(Product product, int? overrideMonth = null, OrderStrategy strategy = OrderStrategy.Normal)
        {
            var resultObject = new AIPredictionResult();

            try
            {
                // PERFORMANCE FIX: Initialize engine only if null
                if (_predictionEngine == null) InitializeEngine();

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

                // Use the cached engine
                var result = _predictionEngine.Predict(input);
                float basePrediction = result.Score;

                if (float.IsNaN(basePrediction) || basePrediction < 0) basePrediction = 0;

                // --- Logic for Confidence & Strategy (Your original code is good here) ---
                double deviation = Math.Abs(basePrediction - product.ReorderTarget);
                double confidence = Math.Max(45.0, Math.Min(98.0, 100.0 - (deviation * 5.0)));
                resultObject.ConfidenceScore = Math.Round(confidence, 1);

                switch (strategy)
                {
                    case OrderStrategy.Conservative: basePrediction *= 0.85f; break;
                    case OrderStrategy.Aggressive: basePrediction *= 1.25f; break;
                }

                int finalTarget = (int)Math.Ceiling(basePrediction);
                int finalOrderQty = finalTarget - product.CurrentStock;

                resultObject.PredictedDemand = finalTarget;
                if (finalOrderQty <= 0)
                {
                    resultObject.FinalOrderQty = 0;
                    resultObject.Reasoning = $"Stock is sufficient. Predicted monthly demand is {finalTarget}.";
                }
                else
                {
                    resultObject.FinalOrderQty = finalOrderQty;
                    resultObject.Reasoning = $"AI predicts {finalTarget} units needed.";
                }

                return resultObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Error: {ex.Message}");
                return new AIPredictionResult
                {
                    FinalOrderQty = Math.Max(0, product.ReorderTarget - product.CurrentStock),
                    PredictedDemand = product.ReorderTarget,
                    Reasoning = "AI Offline. Using static database target."
                };
            }
        }
    }
}