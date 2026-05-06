// This file is optimized for StockSense Year-Aware Forecasting.
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.LightGbm;

namespace StockSense.Web
{
    public partial class MLModel
    {
        public static void Train(string outputModelPath, IEnumerable<ModelInput> trainingData)
        {
            if (trainingData == null || !trainingData.Any())
            {
                throw new Exception("Training failed: No data provided to the AI.");
            }
            var mlContext = new MLContext();
            var data = mlContext.Data.LoadFromEnumerable<ModelInput>(trainingData);
            var model = RetrainModel(mlContext, data);
            SaveModel(mlContext, model, data, outputModelPath);
        }


        public static void SaveModel(MLContext mlContext, ITransformer model, IDataView data, string modelSavePath)
        {
            string directory = Path.GetDirectoryName(modelSavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            DataViewSchema dataViewSchema = data.Schema;
            using (var fs = File.Create(modelSavePath))
            {
                mlContext.Model.Save(model, dataViewSchema, fs);
            }
        }

        public static ITransformer RetrainModel(MLContext mlContext, IDataView trainData)
        {
            var pipeline = BuildPipeline(mlContext);
            var model = pipeline.Fit(trainData);
            return model;
        }

        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            var pipeline = mlContext.Transforms.ReplaceMissingValues(@"MonthNum", @"MonthNum")
                .Append(mlContext.Transforms.ReplaceMissingValues(@"Year", @"Year"))

                // 1. Text Featurization: Converts string identifiers into numerical vectors
                .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: @"ProductID", outputColumnName: @"ProductID"))
                .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: @"ProductName", outputColumnName: @"ProductName"))
                .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: @"Brand", outputColumnName: @"Brand"))
                .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: @"Category", outputColumnName: @"Category"))

                // 2. Concatenation: Merges all processed columns into a single 'Features' vector
                .Append(mlContext.Transforms.Concatenate(@"Features", new[] {
                    @"MonthNum",
                    @"Year",
                    @"ProductID",
                    @"ProductName",
                    @"Brand",
                    @"Category"
                }))

                // 3. Trainer Configuration: LightGBM tuned for business trend regression
                .Append(mlContext.Regression.Trainers.LightGbm(new LightGbmRegressionTrainer.Options()
                {
                    NumberOfLeaves = 885,
                    NumberOfIterations = 100,
                    MinimumExampleCountPerLeaf = 20,
                    LearningRate = 0.1,
                    LabelColumnName = @"QtySold",
                    FeatureColumnName = @"Features",
                    Booster = new GradientBooster.Options()
                    {
                        SubsampleFraction = 0.8,
                        FeatureFraction = 0.9,
                        L1Regularization = 2E-10,
                        L2Regularization = 1.0
                    },
                    MaximumBinCountPerFeature = 255
                }));

            return pipeline;
        }
    }
}