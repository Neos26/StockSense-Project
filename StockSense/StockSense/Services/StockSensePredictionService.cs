using System;
using StockSense.Shared;
using StockSense; // MUST BE HERE

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
        public AIPredictionResult GetPredictiveOrderQty(Product product, int? overrideMonth = null, OrderStrategy strategy = OrderStrategy.Normal)
        {
            var resultObject = new AIPredictionResult();

            try
            {
                int targetMonth = overrideMonth ?? DateTime.Now.Month;

                // This now perfectly matches your auto-generated code!
                var input = new MLModel.ModelInput
                {
                    MonthNum = (float)targetMonth,
                    ProductID = product.Id.ToString(),
                    ProductName = product.Name,
                    Brand = product.Brand ?? "",
                    Category = product.Category ?? "",
                    QtySold = 0
                };

                var result = MLModel.Predict(input);
                float basePrediction = result.Score;

                if (float.IsNaN(basePrediction) || basePrediction < 0) basePrediction = 0;

                double deviation = Math.Abs(basePrediction - product.ReorderTarget);
                double confidence = 100.0 - (deviation * 5.0);

                if (confidence > 98.0) confidence = 98.0;
                if (confidence < 45.0) confidence = 45.0;

                resultObject.ConfidenceScore = Math.Round(confidence, 1);

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