using ATG.ML.MLModels;
using ATG.ML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace ATG.ML
{
    public static class MLFacade
    {
        public static ITransformer TrainAndTest<T>(MLContext mlContext, IEnumerable<T> train, IEnumerable<T> test, string labelColumn, string scoreColumn, IEnumerable<string> ignoredColumns = null, bool normalize = true)
            where T : class
        {
            IDataView trainDataView = mlContext.Data.LoadFromEnumerable<T>(train);
            IDataView testDataView = mlContext.Data.LoadFromEnumerable<T>(test);

            return TrainAndTest(mlContext, trainDataView, testDataView, labelColumn, scoreColumn, ignoredColumns, normalize);
    
            
        }
        public static ITransformer TrainAndTest(MLContext mlContext, IDataView train, IDataView test, string labelColumn, string scoreColumn, IEnumerable<string> ignoredColumns = null, bool normalize = true)

        {
            Console.WriteLine("Training");
            if (normalize)
            {
                var transformer = Train(mlContext, train, ignoredColumns, labelColumn);
                Console.Write("Evaluating");
                Evaluate(mlContext, transformer, test, labelColumn, scoreColumn);
                return transformer;
            }
            else
            {
                var transformer = TrainNoNormalize(mlContext, train, ignoredColumns, labelColumn);
                Console.Write("Evaluating");
                Evaluate(mlContext, transformer, test, labelColumn, scoreColumn);
                return transformer;
            }

        }
        public static void PrintFeatureStats(MLContext context, IDataView dataView2, int permutations, string label, IEnumerable<string> ignoredFeatures = null)
        
        {
         
            
            // 5. Define Stochastic Dual Coordinate Ascent machine learning estimator
            var treeEstimator = TransformPipe(context, dataView2, null, label);
            IDataView dataView = treeEstimator.Transform(dataView2);

            var allFeats = dataView.Schema
                    .Select(column => column.Name)
                    .Where(columnName => columnName != label && (ignoredFeatures == null || !ignoredFeatures.Contains(columnName))).ToArray();
            // 6. Train machine learning model
            var estimator = context.Regression.Trainers.FastTree();
            var sdcaModel = estimator.Fit(dataView);

            ImmutableArray<RegressionMetricsStatistics> permutationFeatureImportance =
                context
                    .Regression
                    .PermutationFeatureImportance(sdcaModel, dataView, permutationCount: permutations);

            // Order features by importance
            var featureImportanceMetrics =
                permutationFeatureImportance
                    .Select((metric, index) => new { index, metric.RSquared })
                    .OrderByDescending(myFeatures => Math.Abs(myFeatures.RSquared.Mean));

            Console.WriteLine("Feature\tPFI");
            Console.WriteLine("FastTree------");
            foreach (var feature in featureImportanceMetrics)
            {
                //Console.WriteLine($"{featName,-20}|\t{feature.RSquared.Mean:F6}");
                Console.WriteLine($"{allFeats[feature.index],-20}|\t{feature.RSquared.Mean:F6}");
            }
            return;

        }
        public static string[] GetFeatures()
        {
            return new string[]
            {
                "RelLastYearTrainerWinPercent",
                "DistanceCode",
                    "GallopedLastEncoded",
                    "WinPercentFromTrack",
                //    "RelTrainerWinPercent",
                  //  "RelTrainerPlacePercentThisYear",
                    "RelDriverWinPercentThisYear",
                  //  "RelTrainerPlacePercentThisYear",
                   //  "RelHorseWinPercentThisYear",
                   //  "RelHorsePlacePercentThisYear",
                    "RelMedianKmTime",
                     "MedianKmTime",
                    "BestKmTime",
                    "AvgKmTime",
                     "RelBestKmTime",
                    "RelAvgKmTime",
                    "StartTypeCode",
                    "HandicapBucket",
                    "BestSpeedAfterFastest",
                    "AvgSpeedAfterFastest",
                    "MedianSpeedAfterFastest",
                   // "InvertedWinOdds",

            };
        }
        public static ITransformer TransformPipe(MLContext mlContext, IDataView dataView, IEnumerable<string> ignoredFeatures, string winnerColumn)
     
        {

            List<string> ignored = null;
            if (ignoredFeatures != null)
                ignored = new List<string>(ignoredFeatures);
            else
                ignored = new List<string>();
            var allFeats = dataView.Schema
                   .Select(column => column.Name)
                   .Where(columnName => !ignored.Contains(columnName) && columnName != winnerColumn).ToArray();

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: winnerColumn)
                /*
                        .Append(mlContext.Transforms.NormalizeMinMax("WinRate"))
                .Append(mlContext.Transforms.NormalizeMinMax("PlaceRate"))
                .Append(mlContext.Transforms.NormalizeMinMax("MoneyPerRace"))
                .Append(mlContext.Transforms.NormalizeMinMax("WinShape"))
                .Append(mlContext.Transforms.NormalizeMinMax("PlaceShape"))
                .Append(mlContext.Transforms.NormalizeMinMax("MoneyShape"))
                .Append(mlContext.Transforms.NormalizeMinMax("BestTimeOnDistance"))
                .Append(mlContext.Transforms.NormalizeMinMax("MedianTimeOnDistance"))
                .Append(mlContext.Transforms.NormalizeMinMax("DistanceWinRate"))
                .Append(mlContext.Transforms.NormalizeMinMax("DistancePlaceRate"))
                .Append(mlContext.Transforms.NormalizeMinMax("StartTypeWinRate"))
                .Append(mlContext.Transforms.NormalizeMinMax("StartTypePlaceRate"))
                */
                .Append(mlContext.Transforms.Concatenate("Features",
                    allFeats
                    ))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"));



            var model = pipeline.Fit(dataView);
            return model;
        }
        public static ITransformer Train(MLContext mlContext, IDataView dataView, IEnumerable<string> ignoredFeatures, string winnerColumn)

        {

            List<string> ignored = null;
            if (ignoredFeatures != null)
                ignored = new List<string>(ignoredFeatures);
            else
                ignored = new List<string>();
            var allFeats = dataView.Schema
                   .Select(column => column.Name)
                   .Where(columnName => !ignored.Contains(columnName) && columnName != winnerColumn).ToArray();

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: winnerColumn)

                .Append(mlContext.Transforms.Concatenate("Features",
                    allFeats
                    ))
            .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.Regression.Trainers.FastTree(
                    numberOfLeaves: 30,
                    numberOfTrees: 150));

            var model = pipeline.Fit(dataView);
            return model;
        }
        public static ITransformer TrainNoNormalize(MLContext mlContext, IDataView dataView, IEnumerable<string> ignoredFeatures, string winnerColumn)

        {

            List<string> ignored = null;
            if (ignoredFeatures != null)
                ignored = new List<string>(ignoredFeatures);
            else
                ignored = new List<string>();
            var allFeats = dataView.Schema
                   .Select(column => column.Name)
                   .Where(columnName => !ignored.Contains(columnName) && columnName != winnerColumn).ToArray();

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: winnerColumn)

                .Append(mlContext.Transforms.Concatenate("Features",
                    allFeats
                    ))
                .Append(mlContext.Regression.Trainers.FastTree(
                    numberOfLeaves: 30,
                    numberOfTrees: 150));

            var model = pipeline.Fit(dataView);
            return model;
        }
        public static R TestSinglePrediction<T,R>(MLContext mlContext, ITransformer model, T entry, string debugText = "")
            where T : class
            where R : class, new()
        {
            var predictionFunction = mlContext.Model.CreatePredictionEngine<T, R>(model);
            var prediction = predictionFunction.Predict(entry);
            //Console.WriteLine($"**********************************************************************");
            // Console.WriteLine($"Predicted time {debugText}: {prediction.TimeAfterWinner}, actual time: {entry.TimeBehindWinner}");
            // Console.WriteLine($"**********************************************************************");
            return prediction;

        }
        public static void Evaluate(MLContext mlContext, ITransformer model, IDataView dataView, string labelCol, string scoreCol)
     
        {
            var predictions = model.Transform(dataView);
            //var preview = predictions.Preview(10);
            //Console.WriteLine($"{string.Join(",", preview.ColumnView.SelectMany(ci => ci.Values.Select(o => o.ToString())))}");

            var metrics = mlContext.Regression.Evaluate(predictions, labelCol, scoreCol);

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");

            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
        }
        public static void WriteCsv(IEnumerable<RaceResultModel> entries, string filename)
        {
            using (var streamWriter = new StreamWriter(File.Open(filename, FileMode.Create)))
            {
                foreach (var hre in entries)
                {
                    WriteRow(streamWriter, hre, ";");
                }
            }
        }
        private static void WriteRow(StreamWriter sw, RaceResultModel model, string del)
        {
            StringBuilder sb = new StringBuilder();

            sw.WriteLine(sb.ToString());
        }
    }
}
