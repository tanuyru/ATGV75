using ATG.ML.MLModels;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ATG.ML
{
    public static class MLModelParser
    {
        public static void TrainAndTest(MLContext mlContext, string trainData, string testData)
        {
            Console.WriteLine("Training");
            var transformer = Train(mlContext, trainData);
            Console.Write("Evaluating");
            Evaluate(mlContext, transformer, testData);
            Console.ReadKey();
        }
        public static ITransformer Train(MLContext mlContext, string trainDataPath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<HorseRaceEntry>(trainDataPath, hasHeader: false, separatorChar: ';');
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "KmTime")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "HorseIdEncoded", inputColumnName: "HorseId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DriverIdEncoded", inputColumnName: "DriverId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "ArenaIdEncoded", inputColumnName: "ArenaId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DistanceEncode", inputColumnName: "Distance"))
                //.Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "TrackIdEncoded", inputColumnName: "Track"))

                .Append(mlContext.Transforms.Concatenate("Features",
                   // "HorseIdEncoded",
                    //"DriverIdEncoded",
                    //"HorseAge",
                    "AvgKmTime",
                    "LastFinishPosition",
                    //"AvgFinishPosition",
                   // "HorseWinPercent",
                   // "HorsePlacePercent",
                    //"ShoesFront",
                    //"ShoesBack",
                    //"FrontChanged",
                    //"BackChanged",
                    "Track",
                    // "IsVoltStart",
                    "DistanceEncode",
                    //"ArenaIdEncoded",
                    //"DaysSinceLastRace",
                    //"RaceAvgPosition",
                    //"RaceAvgKmTime",
                    //"RaceTopAvgKmTime",
                    //"RaceBestKmTime",
                    //"BestHorseOnTrack",
                    //"NumHorsesInRace",
                    "WinOdds"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            return model;
        }
        public static void TestSinglePrediction(MLContext mlContext, ITransformer model, HorseRaceEntry entry)
        {
            var predictionFunction = mlContext.Model.CreatePredictionEngine<HorseRaceEntry, HorsRaceTime>(model);
            var prediction = predictionFunction.Predict(entry);

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted time: {prediction.KmTime}, actual fare: {entry.KmTime}");
            Console.WriteLine($"**********************************************************************");
        }
        public static void Evaluate(MLContext mlContext, ITransformer model, string testDataPath)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<HorseRaceEntry>(testDataPath, hasHeader: false, separatorChar: ';');
            var predictions = model.Transform(dataView);

            var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");

            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
        }
        public static void WriteCsv(IEnumerable<HorseRaceEntry> entries, string filename)
        {
            using (var streamWriter = new StreamWriter(File.Open(filename, FileMode.Create)))
            {
                foreach(var hre in entries)
                {
                    WriteRow(streamWriter, hre, ";");
                }
            }
        }
        private static void WriteRow(StreamWriter sw, HorseRaceEntry horse, string del)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(horse.HorseId).Append(del);
            sb.Append(horse.DriverId).Append(del);
            sb.Append(horse.HorseAge).Append(del);
            sb.Append(horse.AvgKmTime).Append(del);
            sb.Append(horse.LastFinishPosition).Append(del);
            sb.Append(horse.AvgFinishPosition).Append(del);
            sb.Append(horse.HorseWinPercent).Append(del);
            sb.Append(horse.HorsePlacePercent).Append(del);
            sb.Append(horse.ShoesFront).Append(del);
            sb.Append(horse.ShoesBack).Append(del);
            sb.Append(horse.FrontChanged).Append(del);
            sb.Append(horse.BackChanged).Append(del);
            sb.Append(horse.Track).Append(del);
            sb.Append(horse.IsVoltStart).Append(del);
            sb.Append(horse.Distance).Append(del);
            sb.Append(horse.ArenaId).Append(del);
            sb.Append(horse.DaysSinceLastRace).Append(del);
            sb.Append(horse.RaceAvgKmTime).Append(del);
            sb.Append(horse.RaceTopAvgKmTime).Append(del);
            sb.Append(horse.RaceBestKmTime).Append(del);
            sb.Append(horse.BestHorseOnTrack).Append(del);
            sb.Append(horse.NumHorsesInRace).Append(del);
            sb.Append(horse.WinOdds).Append(del);
            sb.Append(horse.KmTime).Append(del);
            sw.WriteLine(sb.ToString());
        }
    }
}
