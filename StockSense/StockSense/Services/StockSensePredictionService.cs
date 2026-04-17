using System;
using System.IO;
using Microsoft.ML;
using StockSense.Shared;
using StockSense; // References your MLModel class

namespace StockSense.Services
{
    public enum OrderStrategy
    {
        Conservative,
        Normal,
        Aggressive
    }

    public class AIPredictionResult
    {
        public int FinalOrderQty { get; set; }
        public int PredictedDemand { get; set; }
        public double ConfidenceScore { get; set; }
        public string Reasoning { get; set; } = "";
    }

    public class StockSensePredictionService
    {
        /// <summary>
        /// Determines the correct path for the .mlnet file.
        /// Points to D:\home\data on Azure and local bin folder on your laptop.
        /// </summary>
        private string GetModelPath()
        {
            if (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null)
            {
                // Azure writable storage
                var azurePath = @"D:\home\data\MLModel.mlnet";

                // Ensure directory exists so it doesn't crash on first run
                if (!Directory.Exists(@"D:\home\data"))
                    Directory.CreateDirectory(@"D:\home\data");

                return azurePath;
            }

            // Local development path (C:\Users\...\bin\Debug\net8.0\MLModel.mlnet)
            return Path.Combine(AppContext.BaseDirectory, "MLModel.mlnet");
        }

        public AIPredictionResult GetPredictiveOrderQty(Product product, int? overrideMonth = null, OrderStrategy strategy = OrderStrategy.Normal)
        {
            var resultObject = new AIPredictionResult();

            try
            {
                int targetMonth = overrideMonth ?? DateTime.Now.Month;

                // 1. Prepare Input
                var input = new MLModel.ModelInput
                {
                    MonthNum = (float)targetMonth,
                    ProductID = product.Id.ToString(),
                    ProductName = product.Name,
                    Brand = product.Brand ?? "",
                    Category = product.Category ?? "",
                    QtySold = 0 // This is what we are predicting
                };

                // 2. Load Model Manually from Writable Path
                var mlContext = new MLContext();
                string modelPath = GetModelPath();

                // Fallback: If no retrained model exists in D:\home\data yet, 
                // try to load the original one shipped with the deployment.
                if (!File.Exists(modelPath))
                {
                    modelPath = Path.Combine(AppContext.BaseDirectory, "MLModel.mlnet");
                }

                if (!File.Exists(modelPath))
                {
                    throw new FileNotFoundException("MLModel.mlnet not found in any location.");
                }

                ITransformer mlModel = mlContext.Model.Load(modelPath, out var _);
                var predEngine = mlContext.Model.CreatePredictionEngine<MLModel.ModelInput, MLModel.ModelOutput>(mlModel);

                // 3. Perform Prediction
                var result = predEngine.Predict(input);
                float basePrediction = result.Score;

                // Handle bad data or negative predictions
                if (float.IsNaN(basePrediction) || basePrediction < 0) basePrediction = 0;

                // 4. Calculate Confidence Score (based on reorder target deviation)
                double deviation = Math.Abs(basePrediction - product.ReorderTarget);
                double confidence = 100.0 - (deviation * 5.0);

                if (confidence > 98.0) confidence = 98.0;
                if (confidence < 45.0) confidence = 45.0;

                resultObject.ConfidenceScore = Math.Round(confidence, 1);

                // 5. Apply Business Strategy (Conservative/Aggressive)
                string strategyText = "Baseline pattern.";
                switch (strategy)
                {
                    case OrderStrategy.Conservative:
                        basePrediction = basePrediction * 0.85f;
                        strategyText = "Reduced by 15% (Conservative).";
                        break;
                    case OrderStrategy.Aggressive:
                        basePrediction = basePrediction * 1.25f;
                        strategyText = "Increased by 25% (Aggressive buffer).";
                        break;
                }

                // 6. Final Calculation
                int finalTarget = (int)Math.Ceiling(basePrediction);
                int finalOrderQty = finalTarget - product.CurrentStock;

                if (finalOrderQty <= 0)
                {
                    resultObject.FinalOrderQty = 0;
                    resultObject.PredictedDemand = finalTarget;
                    resultObject.Reasoning = $"Stock is sufficient. Predicted monthly demand is {finalTarget}.";
                }
                else
                {
                    resultObject.FinalOrderQty = finalOrderQty;
                    resultObject.PredictedDemand = finalTarget;
                    resultObject.Reasoning = $"AI predicts {finalTarget} units needed. {strategyText}";
                }

                return resultObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Error: {ex.Message}");

                // Fallback to basic logic if AI crashes
                int fallbackQty = Math.Max(0, product.ReorderTarget - product.CurrentStock);
                return new AIPredictionResult
                {
                    FinalOrderQty = fallbackQty,
                    PredictedDemand = product.ReorderTarget,
                    ConfidenceScore = 0,
                    Reasoning = "AI Offline. Using static database target."
                };
            }
        }
    }
}