using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Travsport.DB;
using Travsport.DB.Entities;
using Travsport.DB.Entities.Util;
using Travsport.ML.Models;

namespace Travsport.ML
{
    public static class MLFacade
    {
        static string ModelDataPath = @"g:\travsport\models\v1\";
        private static string WinPerPosPath = @"g:\travsport\winperpos\";
        public static void SaveModels(IDataView view, MLContext context, string filename)
        {
            using (var stream = File.OpenWrite(ModelDataPath + filename))
            {
                context.Data.SaveAsBinary(view, stream);
            }
        }
        public static IDataView LoadDataView(MLContext mlContext, int minYear, int maxYear)
        {
            Console.WriteLine("Loading models from " + minYear + " to " + maxYear);
            List<string> fileNames = new List<string>();
            List<HorseTotalModel> models = new List<HorseTotalModel>();
            for(int i = minYear; i < maxYear; i++)
            {
                var f = ModelDataPath + i + "-" + (i + 1) + ".zip";
                fileNames.Add(f);
                Console.WriteLine("Loading file " + f);
                var view = mlContext.Data.LoadFromBinary(f);
                var enumerabl = mlContext.Data.CreateEnumerable<HorseTotalModel>(view, false);
                models.AddRange(enumerabl);
            }
            var dataView = mlContext.Data.LoadFromEnumerable(models);
            return dataView;
        }
        public static void SaveStartWin(WinPerStartPosition p, string fileName)
        {
            var json = JsonConvert.SerializeObject(p);
            File.WriteAllText(WinPerPosPath + fileName, json);
        }
        public static WinPerStartPosition LoadStartWin(string fileName)
        {
            var json = File.ReadAllText(WinPerPosPath + fileName);
            var win = JsonConvert.DeserializeObject<WinPerStartPosition>(json);
            return win;
        }
        public static WinPerStartPosition CreateStartWin(DateTime maxDate)
        {
            using (var context = new TravsportContext())
            {
                var races = context.Races.Where(r => r.StartTime < maxDate && r.DetailStatsVersion == 1 && r.Sport == "trot" && r.StartType != ATG.Shared.Enums.StartTypeEnum.Unknown)
                    .Include(r => r.RaceResults).ThenInclude(rr => rr.Race).ToList();

                var winPerPos = new WinPerStartPosition(races.SelectMany(r => r.RaceResults));
                winPerPos.MaxDate = maxDate;
                return winPerPos;
            }
        }
        public static List<HorseTotalModel> LoadModelsFromDatabas(DateTime from, DateTime to, WinPerStartPosition winPerPos)
        {
            List<HorseTotalModel> models = new List<HorseTotalModel>();

            int skipped = 0;
            using (var context = new TravsportContext())
            {
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                Console.WriteLine("Loading races from " + from + " to " + to);
                var races = context.Races.Where(r => r.StartTime >= from && r.StartTime < to && r.DetailStatsVersion == 1 && r.Sport == "trot")
                    .Include(r => r.RaceResults).ThenInclude(rr => rr.HorseStats).ThenInclude(hs => hs.KmTimeValidProfile)
                    .Include(r => r.RaceResults).ThenInclude(rr => rr.HorseStats).ThenInclude(hs => hs.TimeAfterWinnerLastCapProfile)
                    .Include(r => r.RaceResults).ThenInclude(rr => rr.HorseStats).ThenInclude(hs => hs.TimeAfterWinnerPlaceCapProfile)
                    .ToList();
                Console.WriteLine("Loaded " + races.Count + " races, creating models");
                foreach(var r in races)
                {
                    if (r.RaceResults.Count(rr => rr.HorseStatsId.HasValue) <= 2)
                    {
                        skipped++;
                        continue;
                    }
                    var raceStat = new RaceStats(r);
                    int db = HorseRaceResult.GetDistanceBucket(r.Distance);
                    
                    var trackCond = HorseTotalModel.ParseTrackCondition(r.TrackCondition);
                    var allStats = r.RaceResults.Where(res => !res.Scratched).Select(res => res.HorseStats).ToList();
                    if (allStats.Any(hs => hs.NumTotals == 0))
                    {
                        skipped++;
                        continue;
                    }
                    foreach (var rr in r.RaceResults.Where(res => !res.Scratched))
                    {
                        int hb = WinPerStartPosition.GetDistanceHandicapBucket(rr.DistanceHandicap);
                        var winOnPos = winPerPos.GetWinRatio(r.StartType, db, hb, rr.PositionForDistance);
                        var totMod = new HorseTotalModel(r, rr.HorseStats, allStats, HorseTotalModel.ProfileEnum.KmTime, winOnPos, trackCond);
                        models.Add(totMod);
                    }
                }
            }
            Console.WriteLine("Created " + models.Count + ", skipped " + skipped);
            return models;
        }

        public static ITransformer TrainModel(MLContext mlContext, int minYear, int maxYear, string winnerColumn = "TimeAfterWinner")
        {
            var trainView = LoadDataView(mlContext, minYear, maxYear);
            string[] feats = HorseTotalModel.RelativeColumns.ToArray();
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: winnerColumn)
                .Append(mlContext.Transforms.Concatenate("Features",feats))
                 .Append(mlContext.Regression.Trainers.Sdca());
            Console.WriteLine("Fitting " + feats.Length + " features on " + trainView.GetRowCount().Value + " models");
            var transformer = pipeline.Fit(trainView);
            return transformer;
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

        public static void Auto(MLContext mlContext, IDataView trainView, IDataView validationView, string labelCol = "TimeAfterWinner", string scoreCol = "Score")
        {
            string[] feats = HorseTotalModel.RelativeColumns.ToArray();
            IEstimator<ITransformer> preFeaturizer = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: labelCol)
                .Append(mlContext.Transforms.Concatenate("Features", feats));

            var experimentSettings = new RegressionExperimentSettings();
            experimentSettings.MaxExperimentTimeInSeconds = 600;
            experimentSettings.OptimizingMetric = RegressionMetric.RootMeanSquaredError;

            var experiment = mlContext.Auto().CreateRegressionExperiment(experimentSettings);

            ExperimentResult<RegressionMetrics> experimentResult = experiment.Execute(
                trainData: trainView,
                validationData: validationView, 
                columnInformation: null,
                preFeaturizer);
            IDataView predictions = experimentResult.BestRun.Model.Transform(validationView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
            Console.WriteLine("MEtrics: " + metrics.RootMeanSquaredError + " " + metrics.MeanSquaredError);
            Console.ReadKey();
        }

    }
}
