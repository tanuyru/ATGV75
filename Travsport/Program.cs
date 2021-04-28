using ATG.ML;
using ATG.ML.MLModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Travsport.DataCollector;
using Travsport.DB;
using Travsport.DB.Entities;
using Travsport.WebParser.Json.StartListJson;

namespace Travsport
{
    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext();
            /*
            //var winDic = Travsport.ML.MLFacade.LoadStartWin("2014windic.json");
            Console.WriteLine(DateTime.Now + ": Loading and training");
            var trainedModel = ML.MLFacade.TrainModel(mlContext, 2010, 2013);
            Console.WriteLine("Loading validation-data");
            var validationData = ML.MLFacade.LoadDataView(mlContext, 2014, 2016);
            ML.MLFacade.Evaluate(mlContext, trainedModel, validationData, "TimeAfterWinner", "Score");
            Console.WriteLine("Finish");
            return;
            */
           PredictRaceday("Jägersro", 4, DateTime.Now, true);
            return;
            /*
            Console.WriteLine("Loading models!");
            List<string> ignored = new List<string>();
            ignored.Add("TimeAfterWinnerPlatsMax");
            LoadDBAndSave("traintotal.dat", "testtotal.dat");

            Console.ReadKey();*/
        }
        
        static void CreateDatasets()
        {
            var mlContext = new MLContext();
            for (int year = 2000; year < 2021; year++)
            {
                var prev = year - 1;
                Console.WriteLine(DateTime.Now + "Creating startwin for year " + year);
                var prevYearWin = ML.MLFacade.CreateStartWin(new DateTime(prev, 1, 1));
                Console.WriteLine(DateTime.Now + "Loading data for " + prev + "-" + year);
                var yearModels = ML.MLFacade.LoadModelsFromDatabas(new DateTime(prev, 1, 1), new DateTime(year, 1, 1), prevYearWin);
                Console.WriteLine(DateTime.Now + "Saving...");
                var yearView = mlContext.Data.LoadFromEnumerable(yearModels);
                ML.MLFacade.SaveModels(yearView, mlContext, prev + "-" + year + ".zip");
                Travsport.ML.MLFacade.SaveStartWin(prevYearWin, prev + "windic.json");
            }
            return;
        }
        static ITransformer LoadDbAndTrain(MLContext mlContext, string trainFile, string testFile, IEnumerable<string> ignored = null)
        {
            var testView = mlContext.Data.LoadFromBinary(testFile);
            var trainView = mlContext.Data.LoadFromBinary(trainFile);
            var model = MLFacade.TrainAndTest(mlContext, trainView, testView, "TimeAfterWinner", "Score");
            //MLFacade.PrintFeatureStats(mlContext, testView, 3, "TimeAfterWinner");
            return model;
        }
        static void LoadDBAndSave(string trainFile, string testFile)
        {
            var models = ModelLoader.LoadValidModelsTravspot().OrderByDescending(w => w.RaceDate);
            SplitEntriesOrdered(models, 0.25, out var test, out var train);

            var maxTrainDate = train.Max(w => w.RaceDate);
            var minTestDate = train.Min(w => w.RaceDate);
            Console.WriteLine("MaxTrain: " + maxTrainDate + " MinTest: " + minTestDate);
            var mlContext = new MLContext();
            var testView = mlContext.Data.LoadFromEnumerable(test.Select(w => w.Model));
            var trainView = mlContext.Data.LoadFromEnumerable(train.Select(w => w.Model));
            Console.WriteLine("Saving to disk " + trainFile + " " + testFile);
            using (var stream = File.OpenWrite(trainFile))
            {
                mlContext.Data.SaveAsBinary(trainView, stream);
            }
            using (var stream = File.OpenWrite(testFile))
            {
                mlContext.Data.SaveAsBinary(testView, stream);
            }
            var trainedModel = MLFacade.TrainAndTest(mlContext, trainView, testView, "TimeAfterWinner", "Score");
            MLFacade.PrintFeatureStats(mlContext, testView, 3, "TimeAfterWinner");
        }
        static void LoadDBAndSaveWinOdds(string trainFile, string testFile)
        {
            var models = ModelLoader.LoadValidWinOddsModels();
            SplitEntries(models, 0.25, out var test, out var train);


            var mlContext = new MLContext();
            var testView = mlContext.Data.LoadFromEnumerable(test);
            var trainView = mlContext.Data.LoadFromEnumerable(train);
            using (var stream = File.OpenWrite(trainFile))
            {
                mlContext.Data.SaveAsBinary(trainView, stream);
            }
            using (var stream = File.OpenWrite(testFile))
            {
                mlContext.Data.SaveAsBinary(testView, stream);
            }
            var trainedModel = MLFacade.TrainAndTest(mlContext, trainView, testView, "WinOddsProbability", "Score");
            MLFacade.PrintFeatureStats(mlContext, testView, 3, "WinOddsProbability");
        }
        static void SplitEntries<T>(IEnumerable<T> data, double firstPart, out List<T> first, out List<T> rest)
        {
            int tot = data.Count();
            int toTake = (int)(tot * firstPart);
            var list = new List<T>(data);
            int takeFirstEvery = tot / toTake;
            first = new List<T>();
            rest = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % takeFirstEvery == 0)
                {
                    first.Add(list[i]);
                }
                else
                {
                    rest.Add(list[i]);
                }
            }
        }
        static void SplitEntriesOrdered<T>(IEnumerable<T> data, double firstPart, out List<T> first, out List<T> rest)
        {
            int tot = data.Count();
            int toTake = (int)(tot * firstPart);
            first = data.Take(toTake).ToList();
            rest = data.Skip(toTake).ToList();
        }

        static void PredictRaceday(string arena, long arenaId, DateTime raceDate, bool forceAdd)
        {
            var raceDays = WebParser.WebParser.GetRaceDays(raceDate, raceDate);
            var raceDay = raceDays.SingleOrDefault(rd => rd.TrackName == arena);
            if (raceDay == null)
            {
                Console.WriteLine("Couldnt find " + arena + ", found " + string.Join(", ", raceDays.Select(rd => rd.TrackName)));
                return;
            }
            var startList = WebParser.WebParser.GetStartList(raceDay.RaceDayId);
            if (startList == null)
            {
                Console.WriteLine("Couldnt get startlist");
                return;
            }
            if (forceAdd)
            {
                Console.WriteLine("Importning races");
                ImportStartList(startList, arenaId);
            }
            var raceIds = startList.RaceList.Select(rl => (long)rl.RaceId).ToList();
            var horseModels = ModelLoader.LoadValidModelsTravspot(raceIds, true, null);
            MLContext context = new MLContext();
            var model = LoadDbAndTrain(context, "traintotal.dat", "testtotal.dat");
            var predictionEngine = context.Model.CreatePredictionEngine<RelativeModel, RelativeResult>(model);
            var perRace = horseModels.GroupBy(rw => rw.RaceId).OrderBy(g => g.First().RaceNumber);
            var totHorseDic = ModelLoader.GetTSHorseDic();
            List<Dictionary<string, float>> horsePredTimes = new List<Dictionary<string, float>>();
            int counter = 1;
            foreach(var group in perRace)
            {
                var horseDic = new Dictionary<string, float>();
                foreach(var wrapper in group)
                {
                    var horse = totHorseDic[wrapper.HorseId];
                    string name = horse.Name;
                    var pred = predictionEngine.Predict(wrapper.Model).TimeAfterWinner;
                    horseDic.Add(name, pred);
                }
                Console.WriteLine();
                Console.WriteLine("Race " + counter+"----------");
                foreach(var kvp in horseDic.OrderBy(k => k.Value))
                {
                    Console.WriteLine($"{kvp.Key}: {Math.Round(kvp.Value, 3)}");
                }
                counter++;
            }
        }
        static Race ParseStartList(RaceList race)
        {
            Race r = new Race();
            r.Id = race.RaceId;
            r.Distance = race.Distance;

            r.RaceOrder = race.RaceNumber;
            r.RaceResults = new List<RaceResult>();
            r.ScheduledStartTime = race.StartDateTime;
            r.Sport = "trot";
            r.StartTime = race.StartDateTime;
            if (race.RaceType.Code == "V")
                r.StartType = ATG.Shared.Enums.StartTypeEnum.Volt;
            else if (race.RaceType.Code == "A")
                r.StartType = ATG.Shared.Enums.StartTypeEnum.Auto;
            else
                r.StartType = ATG.Shared.Enums.StartTypeEnum.Unknown;
            r.TrackCondition = race.TrackConditions;

            return r;
        }
        static int ImportStartList(StartListRootJson json, long arenaId)
        {
            int count = 0;

            List<Race> newRaces = new List<Race>();
            List<RaceResult> results = new List<RaceResult>();
            using (var context = new TravsportContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                HashSet<long> addedDrivers = new HashSet<long>();
                HashSet<long> addedHorses = new HashSet<long>();
                foreach (var racedayResult in json.RaceList)
                {
                    var dbRace = ParseStartList(racedayResult);
                    if (context.Races.Any(race => race.Id == dbRace.Id))
                    {
                        Console.WriteLine("Race " + dbRace.Id + " #" + racedayResult.RaceNumber + " already exists");
                        continue;
                    }
                    int raceCounter = 1;

                    dbRace.ArenaId = arenaId;
                    newRaces.Add(dbRace);
                    count++;
                    foreach (var result in racedayResult.Horses)
                    {
                        var rr = ParseRaceResult(racedayResult, result);
                        rr.RaceFKId = dbRace.Id;
                        if (!context.Horses.Any(h => h.Id == result.Id) && addedHorses.Add(result.Id))
                        {

                            var dbHorse = ParseHorseFromStartList(result);

                            context.Horses.Add(dbHorse);
                        }
                  
                        if (rr.DriverId.HasValue && !context.TrainerDrivers.Any(h => h.Id == rr.DriverId.Value) && addedDrivers.Add(rr.DriverId.Value))
                        {
                            var dbDriver = ParseDriverFromStartList(result);
                            context.TrainerDrivers.Add(dbDriver);
                          
                        }
                        results.Add(rr);
                    }
                    raceCounter++;
                }

                Console.WriteLine(DateTime.Now + ": Saving changes count " + count);

                var updates = context.SaveChanges();

                Console.WriteLine(DateTime.Now + ": Finished, moving imported files to Imported, updated " + updates);
             
            }
            Console.WriteLine("Bulkimporting rest");
            Importer.BulkImport(newRaces, Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepNulls | Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepIdentity);
            Importer.BulkImport(results, Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepNulls);
            return count;
        }

        static Horse ParseHorseFromStartList(Hors hors)
        {
            Horse h = new Horse();
            h.Id = hors.Id;
            h.Name = hors.Name;
            h.Linkable = hors.Linkable;
            return h;
        }
        static TrainerDriver ParseDriverFromStartList(Hors hors)
        {
            TrainerDriver td = new TrainerDriver();
            td.Id = hors.Driver.LicenseId;
            td.Name = hors.Driver.Name;
            td.ShortName = hors.Driver.Name;
            return td;
        }
        static RaceResult ParseRaceResult(RaceList race, Hors horse)
        {
            RaceResult rr = new RaceResult();
            rr.HorseId = horse.Id;
            rr.DriverId = horse.Driver.LicenseId;
            rr.Distance = horse.ActualDistance;
            rr.DistanceHandicap = horse.ActualDistance - race.Distance;
            rr.StartNumber = horse.StartPosition;
            return rr;
        }
    }
}
