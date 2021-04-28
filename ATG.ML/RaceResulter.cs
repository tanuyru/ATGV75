using ATG.ML.MLModels;
using ATG.ML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace ATG.ML
{
    public static class RaceResulter
    {
        public static void PrintStats(IEnumerable<RaceResultModel> models)
        {
            PrintPropertyStat(models, (m) => m.AvgKmTime, "AvgKm");
            PrintPropertyStat(models, (m) => m.RelAvgKmTime, "RelAvgKm");
            PrintPropertyStat(models, (m) => m.BestKmTime, "BestKm");
            PrintPropertyStat(models, (m) => m.RelBestKmTime, "RelBestKm");
            PrintPropertyStat(models, (m) => m.MedianKmTime, "MedianKmTime");
            PrintPropertyStat(models, (m) => m.RelMedianKmTime, "RelMedianKmTime");

            PrintPropertyStat(models, (m) => m.WinPercentFromTrack, "WinPercentFromTrack");
            PrintPropertyStat(models, (m) => m.RelTrainerWinPercent, "RelTrainerWinPercent");
            PrintPropertyStat(models, (m) => m.RelTrainerPlacePercentThisYear, "RelTrainerPlacePercentThisYear"); 
            PrintPropertyStat(models, (m) => m.RelDriverWinPercentThisYear, "RelDriverWinPercentThisYear");
            PrintPropertyStat(models, (m) => m.RelTrainerPlacePercentThisYear, "RelTrainerPlacePercentThisYear");
            PrintPropertyStat(models, (m) => m.RelHorseWinPercentThisYear, "RelHorseWinPercentThisYear"); 
            PrintPropertyStat(models, (m) => m.RelHorsePlacePercentThisYear, "RelHorsePlacePercentThisYear");
            PrintPropertyStat(models, (m) => m.RelHorsePlacePercentThisYear, "RelHorsePlacePercentThisYear");
            Console.WriteLine("Finished printing stats------");
        }
        public static void PrintPropertyStat(IEnumerable<RaceResultModel> models, Func<RaceResultModel, float> propSelector, string name)
        {
            var max = models.Max(rm => propSelector(rm));
            var min = models.Min(rm => propSelector(rm));
            var median = models.Select(rm => propSelector(rm)).GetMedian();
            Console.WriteLine($"{name}: Max {max}, Min {min}, {median}");
        }
        public static ITransformer TrainAndTest(MLContext mlContext, IEnumerable<RaceResultModel> train, IEnumerable<RaceResultModel> test)
        {
            Console.WriteLine("Training");
            var transformer = Train(mlContext, train, GetFeatures());
            Console.Write("Evaluating");
            Evaluate(mlContext, transformer, test);
    
            
            return transformer;
        }
        public static void PrintFeatureStats(MLContext context, IEnumerable<RaceResultModel> data, int permutations)
        {
            IDataView dataView2 = context.Data.LoadFromEnumerable<RaceResultModel>(data);

            string[] featureColumnNames = GetFeatures();
            
            var fastTree = context.Regression.Trainers.FastTree();
            // 5. Define Stochastic Dual Coordinate Ascent machine learning estimator
            var treeEstimator = TransformPipe(context, data, GetFeatures());
            IDataView dataView = treeEstimator.Transform(dataView2);

            var allFeats = dataView.Schema
                    .Select(column => column.Name)
                    .Where(columnName => columnName != "TimeAfterWinner").ToArray();
            // 6. Train machine learning model
            var sdcaModel = fastTree.Fit(dataView);

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
                string featName = "Unknown "+feature.index;
                if (feature.index < featureColumnNames.Length)
                    featName = featureColumnNames[feature.index];
                //Console.WriteLine($"{featName,-20}|\t{feature.RSquared.Mean:F6}");
                Console.WriteLine($"{allFeats[feature.index],-20}|\t{feature.RSquared.Mean:F6}");
            }
            return;

            // 5. Define Stochastic Dual Coordinate Ascent machine learning estimator
            var tweed = context.Regression.Trainers.FastTreeTweedie();

            // 6. Train machine learning model
            var tweedModel = tweed.Fit(dataView);

            permutationFeatureImportance =
                context
                    .Regression
                    .PermutationFeatureImportance(tweedModel, dataView, permutationCount: permutations);

            Console.WriteLine("Tweed------");
            foreach (var feature in featureImportanceMetrics)
            {
                Console.WriteLine($"{featureColumnNames[feature.index],-20}|\t{feature.RSquared.Mean:F6}");
            }

            // 5. Define Stochastic Dual Coordinate Ascent machine learning estimator
            var scada = context.Regression.Trainers.Sdca();

            // 6. Train machine learning model
            var lastModel = scada.Fit(dataView);

            permutationFeatureImportance =
                context
                    .Regression
                    .PermutationFeatureImportance(lastModel, dataView, permutationCount: permutations);

            Console.WriteLine("Scada------");
            foreach (var feature in featureImportanceMetrics)
            {
                Console.WriteLine($"{featureColumnNames[feature.index],-20}|\t{feature.RSquared.Mean:F6}");
            }
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
        public static ITransformer TransformPipe(MLContext mlContext, IEnumerable<RaceResultModel> train, string[] features)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<RaceResultModel>(train);
            List<string> ignored = new List<string>();
            ignored.Add("GallopedLast");
            ignored.Add("StartType");
            ignored.Add("GallopedLast");
            ignored.Add("DistanceHandicapBucket");
            ignored.Add("DistanceBucket");
            ignored.Add("TimeAfterWinner");
            ignored.Add("FinishPosition");
            ignored.Add("HorseId");
            ignored.Add("RaceId");
            ignored.Add("DriverId");
            ignored.Add("DQ");
            ignored.Add("Galopp");
            ignored.Add("FrontShoes");
            ignored.Add("BackShoes");
            ignored.Add("FrontChange");
            ignored.Add("BackChange");
            ignored.Add("Scratched");
            ignored.Add("TimeBehindWinner");

            ignored.Add("KmTimeMilliSeconds");
            ignored.Add("InvertedOdds");
            ignored.Add("WinOdds");
            ignored.Add("PrizeMoney");
            ignored.Add("Distribution");
            var allFeats = dataView.Schema
                   .Select(column => column.Name)
                   .Where(columnName => !ignored.Contains(columnName)).ToArray();
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "TimeAfterWinner")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "GallopedLastEncoded", inputColumnName: "GallopedLast"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "StartTypeCode", inputColumnName: "StartType"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HandicapBucket", inputColumnName: "DistanceHandicapBucket"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DistanceCode", inputColumnName: "DistanceBucket"))
                .Append(mlContext.Transforms.Concatenate("Features",
                    allFeats
                    ));

            var model = pipeline.Fit(dataView);
            return model;
        }
        public static ITransformer Train(MLContext mlContext, IEnumerable<RaceResultModel> train, string[] features)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<RaceResultModel>(train);
            List<string> ignored = new List<string>();
            ignored.Add("GallopedLast");
            ignored.Add("StartType");
            ignored.Add("GallopedLast");
            ignored.Add("DistanceHandicapBucket");
            ignored.Add("DistanceBucket");
            ignored.Add("TimeAfterWinner");
            ignored.Add("FinishPosition");
            ignored.Add("HorseId");
            ignored.Add("RaceId");
            ignored.Add("DriverId");
            ignored.Add("DQ");
            ignored.Add("Galopp");
            ignored.Add("FrontShoes");
            ignored.Add("BackShoes");
            ignored.Add("FrontChange");
            ignored.Add("BackChange");
            ignored.Add("Scratched");
            ignored.Add("TimeBehindWinner");
            ignored.Add("KmTimeMilliSeconds");
            ignored.Add("PrizeMoney");
            ignored.Add("InvertedOdds");
            ignored.Add("WinOdds");
            ignored.Add("Distribution");

            var allFeats = dataView.Schema
                   .Select(column => column.Name)
                   .Where(columnName => !ignored.Contains(columnName)).ToArray();
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "TimeAfterWinner")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "GallopedLastEncoded", inputColumnName: "GallopedLast"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "StartTypeCode", inputColumnName: "StartType"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HandicapBucket", inputColumnName: "DistanceHandicapBucket"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DistanceCode", inputColumnName: "DistanceBucket"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HorseIdCode", inputColumnName: "HorseId"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DriverIdCode", inputColumnName: "DriverId"))

                .Append(mlContext.Transforms.Concatenate("Features",
                    allFeats
                    ))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            return model;
        }
        public static float TestSinglePrediction(MLContext mlContext, ITransformer model, RaceResultModel entry, string debugText = "")
        {
            var predictionFunction = mlContext.Model.CreatePredictionEngine<RaceResultModel, RaceResultPrediction>(model);
            var prediction = predictionFunction.Predict(entry);
            //Console.WriteLine($"**********************************************************************");
           // Console.WriteLine($"Predicted time {debugText}: {prediction.TimeAfterWinner}, actual time: {entry.TimeBehindWinner}");
           // Console.WriteLine($"**********************************************************************");
            return prediction.TimeAfterWinner;

        }
        public static void Evaluate(MLContext mlContext, ITransformer model, IEnumerable<RaceResultModel> test)
        {
            IDataView dataView = mlContext.Data.LoadFromEnumerable<RaceResultModel>(test);
            var predictions = model.Transform(dataView);
            //var preview = predictions.Preview(10);
            //Console.WriteLine($"{string.Join(",", preview.ColumnView.SelectMany(ci => ci.Values.Select(o => o.ToString())))}");

            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

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
