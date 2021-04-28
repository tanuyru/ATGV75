using ATG.ML.MLModels;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ATG.ML
{
    public static class TimeToWinner
    {
        public static ITransformer TrainAndTest(MLContext mlContext, string trainData, string testData)
        {
            Console.WriteLine("Training");
            var transformer = Train(mlContext, trainData);
            Console.Write("Evaluating");
            Evaluate(mlContext, transformer, testData);
            Console.ReadKey();
            return transformer;
        }
        public static ITransformer Train(MLContext mlContext, string trainDataPath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile< TimeAfterWinnerModel>(trainDataPath, hasHeader: false, separatorChar: ';');
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "TimeAfterWinner")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "GallopedLastEncoded", inputColumnName: "GallopedLast"))

                .Append(mlContext.Transforms.Concatenate("Features",
                    // "HorseIdEncoded",
                    //"DriverIdEncoded",
                    "Distribution",
                    "DaysSinceLastRace",
                    "EquipmentChange",
                    "GallopedLastEncoded",
                     "HorseWinPercentRel",
                     "MedianRelSpeed",
                    "RecentRelSpeed",
                    "TrackWinAtDistance"
                    //"FrontChanged",
                    //"BackChanged",
                    //"Track",
                    // "IsVoltStart",
                    // "DistanceEncode",
                    //"ArenaIdEncoded",
                    //"DaysSinceLastRace",
                    //"RaceAvgPosition",
                    //"RaceAvgKmTime",
                    //"RaceTopAvgKmTime",
                    //"RaceBestKmTime",
                    //"BestHorseOnTrack",
                    //"NumHorsesInRace",
                    //"WinOdds"
                    ))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            return model;
        }
        public static float TestSinglePrediction(MLContext mlContext, ITransformer model, TimeAfterWinnerModel entry, string debugText = "")
        {
            var predictionFunction = mlContext.Model.CreatePredictionEngine<TimeAfterWinnerModel, TimeResult>(model);
            var prediction = predictionFunction.Predict(entry);
            //Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted time {debugText}: {prediction.TimeAfterWinner}, actual time: {entry.TimeAfterWinner}");
            Console.WriteLine($"**********************************************************************");
            return prediction.TimeAfterWinner;

        }
        public static void Evaluate(MLContext mlContext, ITransformer model, string testDataPath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<TimeAfterWinnerModel>(testDataPath, hasHeader: false, separatorChar: ';');
            var predictions = model.Transform(dataView);
            var preview = predictions.Preview(10);
            Console.WriteLine($"{string.Join(",", preview.ColumnView.SelectMany(ci => ci.Values.Select(o => o.ToString())))}");

            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");

            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
        }
        public static void WriteCsv(IEnumerable<TimeAfterWinnerModel> entries, string filename)
        {
            using (var streamWriter = new StreamWriter(File.Open(filename, FileMode.Create)))
            {
                foreach (var hre in entries)
                {
                    WriteRow(streamWriter, hre, ";");
                }
            }
        }
        private static void WriteRow(StreamWriter sw, TimeAfterWinnerModel model, string del)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(model.ArenaDistanceRel).Append(del);
            sb.Append(model.DaysSinceLastRace).Append(del);
            sb.Append(model.DriverWinPercentRel).Append(del);
            sb.Append(model.EquipmentChange).Append(del);
            sb.Append(model.GallopedLast).Append(del);
            sb.Append(model.HorseWinPercentRel).Append(del);
            sb.Append(model.MedianRelSpeed).Append(del);
            sb.Append(model.RecentRelSpeed).Append(del);
            sb.Append(model.TrackWinAtDistance).Append(del);
            sb.Append(model.Distribution).Append(del);
            sb.Append(model.FinishPosition).Append(del);
            sb.Append(model.RaceId).Append(del);
            sb.Append(model.TimeAfterWinner);
            sw.WriteLine(sb.ToString());
        }
    }
}
