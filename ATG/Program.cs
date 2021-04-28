using ATG.DB;
using ATG.ML;
using ATG.ML.MLModels;
using ATG.ML.Models;
using ATG.ML.Sorters;
using ATG.Shared;
using ATG.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.ML;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace ATG
{
    class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext();

            var winModel = LoadDbAndTrain(mlContext, "trainwinodds.dat", "testwinodds.dat", "WinOddsProbability", "Score", false);
            var timeModel = LoadDbAndTrain(mlContext, "traindatatimeshape.dat", "testdatatimeshape.dat", "TimeAfterWinner", "Score");

            var predEngine = mlContext.Model.CreatePredictionEngine<WinOddsTimeModel, WinOddsTimeResult>(winModel);
            PredictGame("V64_2020-10-01_12_4", mlContext, timeModel, predEngine);
            Console.ReadKey();
            return;

           
            return;

        }
        static ITransformer LoadDbAndTrain(MLContext mlContext, string trainFile, string testFile, string label, string score, bool normalize = true, IEnumerable<string> ignored = null)
        {
            var testView = mlContext.Data.LoadFromBinary(testFile);
            var trainView = mlContext.Data.LoadFromBinary(trainFile);
            var model = MLFacade.TrainAndTest(mlContext, trainView, testView, label, score, ignored, normalize);
           // MLFacade.PrintFeatureStats(mlContext, testView, 3, label);
            return model;
        }
        static int GetOddsBucket(double winOdds)
        {
            if (winOdds <= 2)
                return 0;
            if (winOdds <= 5)
                return 1;
            if (winOdds <= 10)
                return 2;
            if (winOdds <= 25)
                return 3;
            return 4;
        }
        static void RunSortTest5(IEnumerable<RaceResultModel> entries)
        {

            Dictionary<int, float> profitLossPerBucket = new Dictionary<int, float>();
            Dictionary<int, float> sumOddsPerBucket = new Dictionary<int, float>();
            Dictionary<int, int> betsPerBucket = new Dictionary<int, int>();
            Dictionary<int, int> wonBetsPerBucket = new Dictionary<int, int>();
            for(int i = 0; i < 5; i++)
            {
                profitLossPerBucket.Add(i, 0);
                betsPerBucket.Add(i, 0);
                wonBetsPerBucket.Add(i, 0);
                sumOddsPerBucket.Add(i, 0);
            }
 
            int numHorsesMissing = 0;
            var group = entries.GroupBy(rem => rem.RaceId);
            float betSize = 1;
            foreach (var g in group)
            {
                if (g.Count() < 3)
                {
                    numHorsesMissing++;
                    continue;
                }
                foreach(var horse in g)
                {
                    if (horse.WinOdds == 0 || horse.Scratched)
                        continue;
                    int bucket = GetOddsBucket(horse.WinOdds);
                    bool won = horse.FinishPosition == 1;
                    betsPerBucket[bucket]++;
                    if (won)
                    {
                        wonBetsPerBucket[bucket]++;
                        profitLossPerBucket[bucket] += horse.WinOdds;
                    }
                    sumOddsPerBucket[bucket] += horse.WinOdds;
                    profitLossPerBucket[bucket] -= betSize;
                }
            }
            
            for(int i = 0; i < 5; i++)
            {
                var avgImpliedProb = 1.0f / (sumOddsPerBucket[i] / betsPerBucket[i]);
                var winRatio = wonBetsPerBucket[i] / (float)betsPerBucket[i];
                Console.WriteLine($"Bucket {i}: PL: {profitLossPerBucket[i]}, NumBets: {betsPerBucket[i]}, WonBets: {wonBetsPerBucket[i]}, AvgOdds: {avgImpliedProb}, WinRatio: {winRatio}");
            }
        }
        static void RunSortTest4(int numToTake, ITransformer model, IEnumerable<RaceResultModel> entries, MLContext mlContext)
        {

            Dictionary<string, int> numPicked = new Dictionary<string, int>();
            Dictionary<string, int> numBetsPlaced = new Dictionary<string, int>();
            Dictionary<string, float> sumProbPicked = new Dictionary<string, float>();
            int numRaces = 0;
            Dictionary<string, ISorter<RaceResultModel>> sorters = GetSorters3(mlContext, model);
            foreach (var key in sorters.Keys)
            {
                numPicked.Add(key, 0);
                sumProbPicked.Add(key, 0);
                numBetsPlaced.Add(key, 0);
            }

            int skipped = 0;
            int numHorses = 0;
            int numHorsesMissing = 0;
            var group = entries.GroupBy(rem => rem.RaceId);
            
            foreach (var g in group)
            {
                if (g.Count() < 3)
                {
                    numHorsesMissing++;
                    continue;
                }
                foreach (var kvp in sorters)
                {
                    foreach (var horse in kvp.Value.Sort(g, null).Take(numToTake))
                    {
                        numBetsPlaced[kvp.Key]++;
                        if (horse.FinishPosition == 1)
                        {
                            numPicked[kvp.Key]++;
                            sumProbPicked[kvp.Key] += horse.WinOdds - 1.0f;
                        }
                    }
                }
                numHorses += g.Count();
                numRaces++;
            }
            numHorses = group.Count() - numHorsesMissing;
            Console.WriteLine("Finished " + numRaces + " skipped " + skipped + " missingHorses " + numHorsesMissing);
            foreach (var key in sorters.Keys)
            {
                float win = sumProbPicked[key] - numBetsPlaced[key];
                float probRatio = sumProbPicked[key] / numPicked[key];
                float ratio = ((float)numPicked[key] / numHorses);
                Console.WriteLine($"{key}: {numPicked[key]}/{numHorses}, Win: {win} on {numBetsPlaced[key]} placed bets, SumProb: {Math.Round(sumProbPicked[key], 5)}, probRatio: {Math.Round(probRatio, 5)} ({Math.Round(ratio, 5)})");
            }
        }
        static void RunSortTest3()
        {
            var loader = new RaceLoader();
            var entries = RaceLoader.LoadRaceResultModels();

            Dictionary<string, int> numPicked = new Dictionary<string, int>();
            Dictionary<string, float> sumProbPicked = new Dictionary<string, float>();
            int numRaces = 0;
            Dictionary<string, ISorter<RaceResultModel>> sorters = new Dictionary<string, ISorter<RaceResultModel>>();
            foreach (var key in sorters.Keys)
            {
                numPicked.Add(key, 0);
                sumProbPicked.Add(key, 0);
            }

            int skipped = 0;
            int numHorses = 0;
            int numHorsesMissing = 0;

            foreach (var g in entries.GroupBy(rem => rem.RaceId))
            {
                if (g.Count() < 3)
                {
                    numHorsesMissing++;
                    continue;
                }
                foreach (var kvp in sorters)
                {
                    foreach (var horse in kvp.Value.Sort(g, null))
                    {
                        numPicked[kvp.Key]++;
                        sumProbPicked[kvp.Key] += horse.InvertedWinOdds;
                        if (horse.FinishPosition == 1)
                        {
                            break;
                        }
                    }
                }
                numHorses += g.Count();
                numRaces++;
            }
            Console.WriteLine("Finished " + numRaces + " skipped " + skipped + " missingHorses " + numHorsesMissing);
            foreach (var key in sorters.Keys)
            {
                float probRatio = sumProbPicked[key] / numPicked[key];
                float ratio = ((float)numPicked[key] / numHorses);
                Console.WriteLine($"{key}: {numPicked[key]}/{numHorses}, SumProb: {Math.Round(sumProbPicked[key], 3)}, probRatio: {Math.Round(probRatio, 4)} ({Math.Round(ratio, 3)})");
            }
            Console.ReadKey();
        }
        static void RunSortTest2()
        {
            var loader = new RaceLoader();
            var entries = loader.LoadTimeWinnerModels();

            Dictionary<string, int> numPicked = new Dictionary<string, int>();
            int numRaces = 0;
            Dictionary<string, ISorter<TimeAfterWinnerModel>> sorters = GetSorters2();
            foreach (var key in sorters.Keys)
            {
                numPicked.Add(key, 0);
            }

            int skipped = 0;
            int numHorses = 0;
            int numHorsesMissing = 0;

            foreach (var g in entries.GroupBy(rem => rem.RaceId))
            {
                if (g.Count() < 5)
                {
                    numHorsesMissing++;
                    continue;
                }
                foreach (var kvp in sorters)
                {
                    foreach (var horse in kvp.Value.Sort(g, null))
                    {
                        numPicked[kvp.Key]++;
                        if (horse.FinishPosition == 1)
                        {
                            break;
                        }
                    }
                }
                numHorses += g.Count();
                numRaces++;
            }
            Console.WriteLine("Finished " + numRaces + " skipped " + skipped + " missingHorses " + numHorsesMissing);
            foreach (var key in sorters.Keys)
            {
                float ratio = ((float)numPicked[key] / numHorses);
                Console.WriteLine($"{key}: {numPicked[key]}/{numHorses} ({Math.Round(ratio, 5)})");
            }
        }
        static void RunSortTest1()
        {
            var loader = new RaceLoader();
            var entries = loader.Load(Shared.Enums.GameTypeEnum.V75);

            Dictionary<string, int> numPicked = new Dictionary<string, int>();
            int numRaces = 0;
            Dictionary<string, ISorter<RaceEntryModel>> sorters = new Dictionary<string, ISorter<RaceEntryModel>>();
            sorters.Add("Dist", new DistributionSorter());
            sorters.Add("Avg", new AvgTimeTotalSorter());
            sorters.Add("Recent", new RecentFormSorter());
            sorters.Add("FormDist", new FormDistributionSorter());
            sorters.Add("DistAndForm", new DistAndFormSorter());
            sorters.Add("Rand", new RandomSorter());
            sorters.Add("Track", new TrackSorter());
            sorters.Add("Top1", new TopPicker(1));
            sorters.Add("Top3", new TopPicker(3));
            sorters.Add("Top5", new TopPicker(5));

            sorters.Add("Min60", new MinDistSorter(0.6f));
            sorters.Add("Min50", new MinDistSorter(0.5f));
            sorters.Add("Min40", new MinDistSorter(0.4f));
            sorters.Add("Min30", new MinDistSorter(0.3f));
            sorters.Add("Min20", new MinDistSorter(0.2f));

            sorters.Add("LastPos", new LastPosSorter());
            sorters.Add("LastAvgPos", new Last3AvgPos());
            foreach (var key in sorters.Keys)
            {
                numPicked.Add(key, 0);
            }
            var grouped = entries.GroupBy(rem => rem.RaceId);
            var horses = entries.Select(rem => rem.HorseId).Distinct();

            int skipped = 0;
            int numHorses = 0;
            int numHorsesMissing = 0;
            Console.WriteLine("Running test for " + entries.Count + " entries running in " + grouped.Count() + " races");
            foreach (var g in entries.GroupBy(rem => rem.RaceId))
            {
                if (g.Any(rem => rem.AvgKmTime == 0))
                {
                    skipped++;
                    continue;
                }
                if (g.Count() < 5)
                {
                    numHorsesMissing++;
                    continue;
                }
                foreach (var kvp in sorters)
                {
                    foreach (var horse in kvp.Value.Sort(g, null))
                    {
                        numPicked[kvp.Key]++;
                        if (horse.FinishPosition == 1)
                        {
                            break;
                        }
                    }
                }
                numHorses += g.Count(rem => rem.AvgKmTime > 0);
                numRaces++;
            }
            Console.WriteLine("Finished " + numRaces + " skipped " + skipped + " missingHorses " + numHorsesMissing);
            foreach (var key in sorters.Keys)
            {
                float ratio = ((float)numPicked[key] / numHorses);
                Console.WriteLine($"{key}: {numPicked[key]}/{numHorses} ({Math.Round(ratio, 5)})");
            }
        }
        static Dictionary<string, ISorter<RaceResultModel>> GetSorters3(MLContext mlContext, ITransformer model)
        {
            Dictionary<string, ISorter<RaceResultModel>> sorters = new Dictionary<string, ISorter<RaceResultModel>>();
            sorters.Add("Rand", new RRRandSorter());
            sorters.Add("Dist", new RROddsSorter());
            sorters.Add("Predict", new RRPredictSorter(mlContext, model));
            return sorters;
        }
        static Dictionary<string, ISorter<TimeAfterWinnerModel>> GetSorters2()
        {
            var mlContext = new MLContext();
            var model = TimeToWinner.TrainAndTest(mlContext, "ttwtrain.txt", "ttwtest.txt");

            Dictionary<string, ISorter<TimeAfterWinnerModel>> sorters = new Dictionary<string, ISorter<TimeAfterWinnerModel>>();
            sorters.Add("Rand", new TTWRandSort());
            sorters.Add("Dist", new TTWDistSorter());
            sorters.Add("Predict", new PredictSorter(mlContext, model));
            return sorters;
        }
        static Dictionary<string, ISorter<RaceEntryModel>> GetSorters()
        {
            Dictionary<string, ISorter<RaceEntryModel>> sorters = new Dictionary<string, ISorter<RaceEntryModel>>();
            sorters.Add("Dist", new DistributionSorter());
            sorters.Add("Avg", new AvgTimeTotalSorter());
            sorters.Add("Recent", new RecentFormSorter());
            sorters.Add("FormDist", new FormDistributionSorter());
            sorters.Add("DistAndForm", new DistAndFormSorter());
            sorters.Add("Rand", new RandomSorter());
            sorters.Add("Track", new TrackSorter());
            sorters.Add("Top1", new TopPicker(1));
            sorters.Add("Top3", new TopPicker(3));
            sorters.Add("Top5", new TopPicker(5));

            sorters.Add("Min60", new MinDistSorter(0.6f));
            sorters.Add("Min50", new MinDistSorter(0.5f));
            sorters.Add("Min40", new MinDistSorter(0.4f));
            sorters.Add("Min30", new MinDistSorter(0.3f));
            sorters.Add("Min20", new MinDistSorter(0.2f));

            sorters.Add("LastPos", new LastPosSorter());
            sorters.Add("LastAvgPos", new Last3AvgPos());
            return sorters;
        }


        static void PlayTotosGeneric(int numPicksPerRace, DateTime start, ITransformer model, MLContext mlContext)
        {
            List<GameTypeEnum> gameTypes = new List<GameTypeEnum>();
            //gameTypes.Add(GameTypeEnum.GS75);
            gameTypes.Add(GameTypeEnum.V75);
            gameTypes.Add(GameTypeEnum.V86);
            //gameTypes.Add(GameTypeEnum.V64);
            //gameTypes.Add(GameTypeEnum.V65);
            Dictionary<GameTypeEnum, double> netPerGT = new Dictionary<GameTypeEnum, double>();
            Dictionary<GameTypeEnum, double> netPerGTOdds = new Dictionary<GameTypeEnum, double>();

            foreach (var gt in gameTypes)
            {
                netPerGT.Add(gt, 0);
                netPerGTOdds.Add(gt, 0);
            }
            using (var context = new AtgContext())
            {
                var games = context.ComboGames
                    .Include(c => c.Payouts)
                    .Include(c => c.Races)
                    .Where(cg => gameTypes.Contains(cg.GameType) && cg.Races.Any(gr => gr.Race.ScheduledStartTime > start)).ToList();
                var allRaceIds = games.SelectMany(cg => cg.Races).Select(gr => gr.RaceId).ToList();


                var wrappers = ModelLoader.LoadValidModels(allRaceIds);
                    
                double totalCost = 0;
                double totalWin = 0;


                double totalCostOdds = 0;
                double totalWinOdds = 0;
                int numWins = 0;
                int numWinsOdds = 0;
                foreach (var game in games)
                {
                    List<List<HorseWinPrediction>> predOutcomes = new List<List<HorseWinPrediction>>();
                    List<Dictionary<int, float>> outputs = new List<Dictionary<int, float>>();

                    var raceIds = game.Races.Select(r => r.RaceId).ToList();
                    foreach (var raceId in raceIds)
                    {
                        var raceWrappers = wrappers.Where(w => w.RaceId == raceId);
                        
                        Dictionary<string, float> horseTimes = new Dictionary<string, float>();
                        Dictionary<int, float> finisherTimes = new Dictionary<int, float>();
                        Dictionary<string, float> actualTimes = new Dictionary<string, float>();

                        List<HorseWinPrediction> preds = new List<HorseWinPrediction>();
                        //Console.WriteLine("Creating models for race " + r.Id + " from " + predModels.Count + " from " + r.RaceResults.Count + " raceresults");
                        foreach (var m in raceWrappers)
                        {
                            var p = MLFacade.TestSinglePrediction<RelativeModel, RelativeResult>(mlContext, model, m.Model, "");

                            var hwp = new HorseWinPrediction();
                            hwp.Distribution = 0;
                            hwp.FinishPosition = m.FinishPosition;
                            hwp.HorseId = m.HorseId;
                            hwp.Time = p.TimeAfterWinner;
                            hwp.WinOdds = m.WinOdds;
                            preds.Add(hwp);
                            if (!finisherTimes.ContainsKey(m.FinishPosition))
                            {
                                finisherTimes.Add(m.FinishPosition, p.TimeAfterWinner);
                            }

                        }
                        predOutcomes.Add(preds);
                        outputs.Add(finisherTimes);

                    }

                    int counter = 1;

                    int corrects = 0;
                    int oddsCorrect = 0;
                    foreach (var list in predOutcomes)
                    {
                        //Console.WriteLine("Race " + counter + " rankings");
                        foreach (var hwp in list.Where(p => p.WinOdds > 0).OrderBy(p => p.Time).Take(numPicksPerRace))
                        {
                            if (hwp.FinishPosition == 1)
                            {
                                corrects++;
                                break;
                            }
                        }
                        foreach (var hwp in list.Where(p => p.WinOdds > 0).OrderBy(p => p.WinOdds).Take(numPicksPerRace))
                        {
                            if (hwp.FinishPosition == 1)
                            {
                                oddsCorrect++;
                                break;
                            }
                        }
                        counter++;
                    }
                    var rowCost = (Math.Pow(numPicksPerRace, counter) * GetCostPerRow(game.GameType));
                    netPerGT[game.GameType] -= rowCost;
                    totalCost += rowCost;
                    var payout = game.Payouts.FirstOrDefault(gp => gp.NumWins == corrects);
                    if (payout != null)
                    {
                        var sekPayout = payout.Payout / 100.0;

                        totalWin += (sekPayout);
                        var net = totalWin - totalCost;
                        Console.WriteLine($"---{sekPayout} on game {game.GameId} and {payout.NumWins} wins ({net})");

                        netPerGT[game.GameType] += sekPayout;
                        numWins++;
                    }
                    netPerGTOdds[game.GameType] -= rowCost;
                    totalCostOdds += rowCost;
                    var payoutOdds = game.Payouts.FirstOrDefault(gp => gp.NumWins == oddsCorrect);
                    if (payoutOdds != null)
                    {
                        var sekPayout = payoutOdds.Payout / 100.0;
                        var net = totalWinOdds - totalCostOdds;
                        Console.WriteLine($"+++ODDS {sekPayout} on game {game.GameId} and {payoutOdds.NumWins} wins ({net})");
                        totalWinOdds += (sekPayout);
                        netPerGTOdds[game.GameType] += sekPayout;
                        numWinsOdds++;
                    }
                }

                var netRes = totalWin - totalCost;
                var netResPerGame = netRes / games.Count;
                Console.WriteLine($"TotalCost {totalCost}, TotWin {totalWin}, Net {netRes}, PerGame {netResPerGame}, {numWins} wins");


                var netResOdds = totalWinOdds - totalCostOdds;
                var netResPerGameOdds = netResOdds / games.Count;
                Console.WriteLine($"ODDS TotalCost {totalCostOdds}, TotWin {totalWinOdds}, Net {netResOdds}, PerGame {netResPerGameOdds}, {numWinsOdds} wins");
                foreach (var gt in gameTypes)
                {
                    Console.WriteLine($"Gametype " + gt + " stats:");
                    Console.WriteLine($"Pred {netPerGT[gt]}, Odds {netPerGTOdds[gt]}");
                }
            }
        }
        static void PlayTotos(int numPicksPerRace, DateTime start)
        {
            var models = RaceLoader.LoadRaceResultModels();
            var mlContext = new Microsoft.ML.MLContext();
            
            List<GameTypeEnum> gameTypes = new List<GameTypeEnum>();
            //gameTypes.Add(GameTypeEnum.GS75);
            gameTypes.Add(GameTypeEnum.V75);
            gameTypes.Add(GameTypeEnum.V86);
            gameTypes.Add(GameTypeEnum.V64);
            gameTypes.Add(GameTypeEnum.V65);
            Dictionary<GameTypeEnum, double> netPerGT = new Dictionary<GameTypeEnum, double>();
            Dictionary<GameTypeEnum, double> netPerGTOdds = new Dictionary<GameTypeEnum, double>();

            foreach(var gt in gameTypes)
            {
                netPerGT.Add(gt, 0);
                netPerGTOdds.Add(gt, 0);
            }
            using (var context = new AtgContext())
            {
                var games = context.ComboGames
                    .Include(c => c.Payouts)
                    .Include(c => c.Races).ThenInclude(gr => gr.Race).Where(cg => cg.Races.Count > 0 && cg.Races.All(gr => gr.Race.Sport == "trot" && gr.Race.ScheduledStartTime >= start) && gameTypes.Contains(cg.GameType)).ToList();
                var gameRaceIds = games.SelectMany(gr => gr.Races.Select(r => r.RaceId)).ToList();
                var raceDictionary = context.Races.ToDictionary(r => r.Id, r => r.ScheduledStartTime);
                var trainSet = models.Where(mod => raceDictionary[mod.RaceId] < start).ToList();
                var testSet = models.Where(mod => raceDictionary[mod.RaceId] >= start).ToList();
                Console.WriteLine("Training with " + trainSet.Count + " evaluating on " + testSet.Count + " entries");
                var model = RaceResulter.TrainAndTest(mlContext, trainSet, testSet);
                Console.WriteLine($"Found {games.Count} comboGames with only trot");
               
                var races = context.Races.Include(race => race.RaceResults).ToList();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: Loading recents");
                var horseDic = context.Horses.ToDictionary(h => h.Id, h => h);
                var distinctDistances = races.Select(r => RaceResultModel.GetDistanceBucket(r.Distance)).Distinct();
                Dictionary<int, TrackWinInfo> trackDicPerDistance = new Dictionary<int, TrackWinInfo>();
                Dictionary<int, TrackWinInfo> trackDicPerDistanceVolt = new Dictionary<int, TrackWinInfo>();
                foreach (var dd in distinctDistances)
                {
                    TrackWinInfo twi = new TrackWinInfo(races.Where(r => r.StartType == StartTypeEnum.Volt && RaceResultModel.GetDistanceBucket(r.Distance) == dd).SelectMany(r => r.RaceResults).Where(rr => rr.FinishTimeMilliseconds > 0));
                    trackDicPerDistanceVolt.Add(dd, twi);

                    TrackWinInfo twi2 = new TrackWinInfo(races.Where(r => r.StartType == StartTypeEnum.Auto && RaceResultModel.GetDistanceBucket(r.Distance) == dd).SelectMany(r => r.RaceResults).Where(rr => rr.FinishTimeMilliseconds > 0));
                    trackDicPerDistance.Add(dd, twi2);
                }
                Console.WriteLine($"Creating models");

                var recentHorses = context.RecentHorseStarts.ToList().GroupBy(rs => rs.HorseId).ToDictionary(g => g.Key, g => g.ToList());
                double totalCost = 0;
                double totalWin = 0;


                double totalCostOdds = 0;
                double totalWinOdds = 0;
                int numWins = 0;
                int numWinsOdds = 0;
                foreach (var game in games)
                {
                    var raceIds = game.Races.Select(r => r.RaceId).ToList();
                   // Console.WriteLine("Found " + string.Join(",", raceIds) + " races for game " + game.GameId);
                    var allResults = races.Where(rr => raceIds.Contains(rr.Id)).SelectMany(r => r.RaceResults).ToList();

                    var groupedByHorse = allResults.GroupBy(rr => rr.Horse.Id).ToDictionary(g => g.Key, g => g.ToList());
                    var g = allResults.GroupBy(rr => rr.Race.Id);
                    StringBuilder sb = new StringBuilder();
                    List<Dictionary<int, float>> outputs = new List<Dictionary<int, float>>();
                    var racesForGame = races.Where(r => raceIds.Contains(r.Id)).ToList();
                    List<List<HorseWinPrediction>> predOutcomes = new List<List<HorseWinPrediction>>();
                    if (racesForGame.Any(r => r.RaceResults.All(rr => rr.KmTimeMilliSeconds == 0)))
                    {
                        Console.WriteLine($"Skipping game {game.GameId} because no results");
                        continue;
                    }
                    foreach (var r in racesForGame)
                    {
                        TrackWinInfo twi = null;
                        if (r.StartType == StartTypeEnum.Auto)
                            twi = trackDicPerDistance[RaceResultModel.GetDistanceBucket(r.Distance)];
                        else if (r.StartType == StartTypeEnum.Volt)
                            twi = trackDicPerDistanceVolt[RaceResultModel.GetDistanceBucket(r.Distance)];
                        else
                            continue;
                        var predModels = RaceLoader.LoadRaceModels(r, twi, recentHorses);
                        if (predModels.Count == 0)
                        {
                            continue;
                        }
                        Dictionary<string, float> horseTimes = new Dictionary<string, float>();
                        Dictionary<int, float> finisherTimes = new Dictionary<int, float>();
                        Dictionary<string, float> actualTimes = new Dictionary<string, float>();
                        
                        List<HorseWinPrediction> preds = new List<HorseWinPrediction>();
                        //Console.WriteLine("Creating models for race " + r.Id + " from " + predModels.Count + " from " + r.RaceResults.Count + " raceresults");
                        foreach (var m in predModels)
                        {
                            var p = RaceResulter.TestSinglePrediction(mlContext, model, m, "");

                            var hwp = new HorseWinPrediction();
                            hwp.Distribution = m.Distribution;
                            hwp.FinishPosition = m.FinishPosition;
                            hwp.HorseId = m.HorseId;
                            hwp.Time = p;
                            hwp.WinOdds = m.WinOdds;
                            preds.Add(hwp);
                            if (!finisherTimes.ContainsKey(m.FinishPosition))
                            {
                                finisherTimes.Add(m.FinishPosition, p);
                            }

                        }
                        predOutcomes.Add(preds);
                        outputs.Add(finisherTimes);

                    }

                    int counter = 1;

                    int corrects = 0;
                    int oddsCorrect = 0;
                    foreach (var list in predOutcomes)
                    {
                        //Console.WriteLine("Race " + counter + " rankings");
                        foreach (var hwp in list.Where(p => p.WinOdds > 0).OrderBy(p => p.Time).Take(numPicksPerRace))
                        {
                            if (hwp.FinishPosition == 1)
                            {
                                corrects++;
                                break;
                            }
                        }
                        foreach (var hwp in list.Where(p => p.WinOdds > 0).OrderBy(p => p.WinOdds).Take(numPicksPerRace))
                        {
                            if (hwp.FinishPosition == 1)
                            {
                                oddsCorrect++;
                                break;
                            }
                        }
                        counter++;
                    }
                    var rowCost = (Math.Pow(numPicksPerRace, counter) * GetCostPerRow(game.GameType));
                    netPerGT[game.GameType] -= rowCost;
                    totalCost += rowCost;
                    var payout = game.Payouts.FirstOrDefault(gp => gp.NumWins == corrects);
                    if (payout != null)
                    {
                        var sekPayout = payout.Payout / 100.0;

                        totalWin += (sekPayout);
                        var net = totalWin - totalCost;
                        Console.WriteLine($"Won {sekPayout} on game {game.GameId} and {payout.NumWins} wins ({net})");

                        netPerGT[game.GameType] += sekPayout;
                        numWins++;
                    }
                    netPerGTOdds[game.GameType] -= rowCost;
                    totalCostOdds += rowCost;
                    var payoutOdds = game.Payouts.FirstOrDefault(gp => gp.NumWins == oddsCorrect);
                    if (payoutOdds != null)
                    {
                        var sekPayout = payoutOdds.Payout / 100.0;
                        var net = totalWinOdds - totalCostOdds;
                        Console.WriteLine($"ODDS Won {sekPayout} on game {game.GameId} and {payoutOdds.NumWins} wins ({net})");
                        totalWinOdds += (sekPayout);
                        netPerGTOdds[game.GameType] += sekPayout;
                        numWinsOdds++;
                    }
                }

                var netRes = totalWin - totalCost;
                var netResPerGame = netRes / games.Count;
                Console.WriteLine($"TotalCost {totalCost}, TotWin {totalWin}, Net {netRes}, PerGame {netResPerGame}, {numWins} wins");


                var netResOdds = totalWinOdds - totalCostOdds;
                var netResPerGameOdds = netResOdds / games.Count;
                Console.WriteLine($"ODDS TotalCost {totalCostOdds}, TotWin {totalWinOdds}, Net {netResOdds}, PerGame {netResPerGameOdds}, {numWinsOdds} wins");
                foreach(var gt in gameTypes)
                {
                    Console.WriteLine($"Gametype " + gt + " stats:");
                    Console.WriteLine($"Pred {netPerGT[gt]}, Odds {netPerGTOdds[gt]}");
                }
            }
        }
        static double GetCostPerRow(GameTypeEnum gt)
        {
            if (gt == GameTypeEnum.V75)
            {
                return 0.5;
            }
            else if (gt == GameTypeEnum.V86)
            {
                return 0.25;
            }
            else
            {
                return 1;
            }
        }
        static void PredictGame(string gameId, MLContext mlContext, ITransformer model, PredictionEngine<WinOddsTimeModel, WinOddsTimeResult> oddsTranslator)
        {
            using (var context = new AtgContext())
            {
                var game = context.ComboGames
                        .Include(c => c.Races).ThenInclude(gr => gr.Race).ThenInclude(r => r.Arena).SingleOrDefault(cg => cg.GameId == gameId);

                var arenas = game.Races.Select(gr => gr.Race.Arena.Name).Distinct().ToList();
                var date = game.Races.First().Race.StartTime;
                List<RelativeModelWrapper> models = new List<RelativeModelWrapper>();
                
                foreach (var arena in arenas)
                {
                    var mods = ModelLoader.LoadValidModelsTravspotFKRaceIds(arena, 1, 16, date);
                    models.AddRange(mods);
                }
                var perRace = models.GroupBy(w => w.RaceId);

                List<Dictionary<string, float>> outputs = new List<Dictionary<string, float>>();
                List<Dictionary<string, float>> horsePredictedOdds = new List<Dictionary<string, float>>();
                List<Dictionary<string, float>> horseActualOdds = new List<Dictionary<string, float>>();
                var horseDic = ModelLoader.GetTSHorseDic();
                foreach (var g in perRace)
                {
                    Dictionary<string, float> horseTimes = new Dictionary<string, float>();
                    Dictionary<string, float> actualTimes = new Dictionary<string, float>();
                    Dictionary<string, float> predictedOdds = new Dictionary<string, float>();
                    Dictionary<string, float> actualOdds = new Dictionary<string, float>();
                    float overround = 0;
                    float predOverround = 0;
                    foreach (var m in g)
                    {
                        var p = MLFacade.TestSinglePrediction<RelativeModel, RelativeResult>(mlContext, model, m.Model, "");
                        var horseName = horseDic[m.HorseId].Name;
                        if (!horseTimes.ContainsKey(horseName))
                        {
                            horseTimes.Add(horseName, p.TimeAfterWinner);
                            WinOddsTimeModel oddsModel = new WinOddsTimeModel();
                            oddsModel.TimeAfterWinner = p.TimeAfterWinner;
                            var predOdds = oddsTranslator.Predict(oddsModel).WinOddsProbability;
                            Console.WriteLine("PredTime " + p.TimeAfterWinner + " translated to winProb " + predOdds);
                          
                            predOverround += predOdds;
                            predictedOdds.Add(horseName, predOdds);
                            actualOdds.Add(horseName, m.WinOdds);
                            if (m.WinOdds > 0)
                                overround += 1.0f / m.WinOdds;
                        }
                    }
                    Console.WriteLine("Overround predicted " + predOverround + " overroundActual " + overround);
                    foreach(var key in actualOdds.Keys.ToList())
                    {
                        if (actualOdds[key] > 0 && overround > 1)
                        {
                            actualOdds[key] = 1.0f / (actualOdds[key] * overround);
                        }
                        if (predOverround > 0)
                            predictedOdds[key] = predictedOdds[key] / predOverround;
                    }
                    horseActualOdds.Add(actualOdds);
                    horsePredictedOdds.Add(predictedOdds);
                    outputs.Add(horseTimes);

                }

                int counter = 1;
                int index = 0;
                for (int i = 0; i < outputs.Count; i++)
                {
                    var dictionary = outputs[i];

                    Console.WriteLine("Race " + counter + " rankings");
                    var actualDic = horseActualOdds[i];
                    var predDic = horsePredictedOdds[i];
                    foreach (var kvp in dictionary.OrderBy(kvp => kvp.Value))
                    {
                        var winRatio = 0f;
                        if (predDic[kvp.Key] > 0 && actualDic[kvp.Key] > 0)
                        {
                            winRatio = (float)Math.Round(predDic[kvp.Key] / actualDic[kvp.Key], 4);
                        }
                        if (winRatio > 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else if (winRatio == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        Console.WriteLine($"{kvp.Key}: {kvp.Value}, PredictedOdds: {Math.Round(predDic[kvp.Key], 3)}, Actual: {Math.Round(actualDic[kvp.Key], 3)}, Ratio: {Math.Round(winRatio, 3)}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("--------------------------------");
                    counter++;
                }
            }
        }
        static void PredictGame(string gameId, ITransformer model)
        {
            var mlContext = new Microsoft.ML.MLContext();
            using (var context = new AtgContext())
            {
                var game = context.ComboGames
                    .Include(c => c.Races).ThenInclude(gr => gr.Race).SingleOrDefault(cg => cg.GameId == gameId);

                if (game == null)
                {
                    Console.WriteLine("Unknown gameId " + gameId);
                    return;
                }
                var races = context.Races.Include(race => race.RaceResults).ToList();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: Loading recents");
                var horseDic = context.Horses.ToDictionary(h => h.Id, h => h);
                var distinctDistances = races.Select(r => RaceResultModel.GetDistanceBucket(r.Distance)).Distinct();
                Dictionary<int, TrackWinInfo> trackDicPerDistance = new Dictionary<int, TrackWinInfo>();
                Dictionary<int, TrackWinInfo> trackDicPerDistanceVolt = new Dictionary<int, TrackWinInfo>();
                foreach (var dd in distinctDistances)
                {
                    TrackWinInfo twi = new TrackWinInfo(races.Where(r => r.StartType == StartTypeEnum.Volt && RaceResultModel.GetDistanceBucket(r.Distance) == dd).SelectMany(r => r.RaceResults).Where(rr => rr.FinishTimeMilliseconds > 0));
                    trackDicPerDistanceVolt.Add(dd, twi);

                    TrackWinInfo twi2 = new TrackWinInfo(races.Where(r => r.StartType == StartTypeEnum.Auto && RaceResultModel.GetDistanceBucket(r.Distance) == dd).SelectMany(r => r.RaceResults).Where(rr => rr.FinishTimeMilliseconds > 0));
                    trackDicPerDistance.Add(dd, twi2);
                }
                Console.WriteLine($"Creating models");

                var recentHorses = context.RecentHorseStarts.ToList().GroupBy(rs => rs.HorseId).ToDictionary(g => g.Key, g => g.ToList());

                var raceIds = game.Races.Select(gr => gr.Race.Id);
                Console.WriteLine("Found " + string.Join(",", raceIds) + " races for game " + gameId);
                var allResults = context.RaceResults.Include(rr => rr.Race).Include(rr => rr.Horse)
                    .Include(rr => rr.Distributions).Where(rr => raceIds.Contains(rr.Race.Id)).ToList();

                var groupedByHorse = allResults.GroupBy(rr => rr.Horse.Id).ToDictionary(g => g.Key, g => g.ToList());
                var g = allResults.GroupBy(rr => rr.Race.Id);
                StringBuilder sb = new StringBuilder();
                List<Dictionary<string, float>> outputs = new List<Dictionary<string, float>>();
                foreach(var r in races.Where(r => raceIds.Contains(r.Id)))
                {
                    TrackWinInfo twi = null;
                    if (r.StartType == StartTypeEnum.Auto)
                        twi = trackDicPerDistance[RaceResultModel.GetDistanceBucket(r.Distance)];
                    else if (r.StartType == StartTypeEnum.Volt)
                        twi = trackDicPerDistanceVolt[RaceResultModel.GetDistanceBucket(r.Distance)];
                    else
                        continue;
                    var predModels = RaceLoader.LoadRaceModels(r, twi, recentHorses,future: true);

                    Dictionary<string, float> horseTimes = new Dictionary<string, float>();
                    Dictionary<string, float> actualTimes = new Dictionary<string, float>();
                    Console.WriteLine("Creating models for race " + r.Id+" from "+predModels.Count+" from "+r.RaceResults.Count+" raceresults");
                    foreach (var m in predModels)
                    {
                        var p = RaceResulter.TestSinglePrediction(mlContext, model, m, "");
                        if (!horseTimes.ContainsKey(horseDic[m.HorseId].Name + " (" + m.Track + ")"))
                            horseTimes.Add(horseDic[m.HorseId].Name+" ("+m.Track+")", p);
                        else
                            Console.WriteLine($"Found horse {horseDic[m.HorseId].Name} in multiple models?");

                    }
                    outputs.Add(horseTimes);
             
                }

                int counter = 1;
                int index = 0;
                foreach(var dictionary in outputs)
                {
                    Console.WriteLine("Race " + counter + " rankings");
                    foreach(var kvp in dictionary.OrderBy(kvp => kvp.Value))
                    {
                        Console.WriteLine($"{kvp.Key}: Predicted {kvp.Value}");
                    }
                    Console.WriteLine("--------------------------------");
                    counter++;
                }
            }
            
        }
        static void LoadTimeToWinnerModels()
        {
            var loader = new RaceLoader();
            var entries = loader.LoadTimeWinnerModels();
            SplitEntries<TimeAfterWinnerModel>(entries, 0.7, out var trainData, out var testData);
            Console.WriteLine("Loaded " + entries.Count + " entries");
            TimeToWinner.WriteCsv(trainData, "ttwtrain.txt");
            TimeToWinner.WriteCsv(testData, "ttwtest.txt");
            Console.WriteLine("Finished !");
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
        static void LoadMLModels()
        {
            var loader = new RaceLoader();
            var entries = loader.GetHorseRaceEntries( GameTypeEnum.V75 );
            SplitEntries<HorseRaceEntry>(entries, 0.7, out var trainData, out var testData);
            MLModelParser.WriteCsv(trainData, "train.txt");
            MLModelParser.WriteCsv(testData, "test.txt");
            Console.WriteLine("Loaded " + entries.Count + " entries");
        }
        static void EvaluteRankingOrder()
        {
            var loader = new RaceLoader();
            var entries = loader.Load(Shared.Enums.GameTypeEnum.V75);

            Dictionary<string, int> totDiff = new Dictionary<string, int>();
            int numRaces = 0;
            Dictionary<string, ISorter<RaceEntryModel>> sorters = GetSorters();
            foreach (var key in sorters.Keys)
            {
                totDiff.Add(key, 0);
            }
            var grouped = entries.GroupBy(rem => rem.RaceId);
            var horses = entries.Select(rem => rem.HorseId).Distinct();

            int skipped = 0;
            int numHorses = 0;
            int numHorsesMissing = 0;
            Console.WriteLine("Running test for " + entries.Count + " entries running in " + grouped.Count() + " races");
            foreach (var g in entries.GroupBy(rem => rem.RaceId))
            {
                if (g.Any(rem => rem.AvgKmTime == 0))
                {
                    skipped++;
                    continue;
                }
                if (g.Count() < 4)
                {
                    numHorsesMissing++;
                    continue;
                }
                foreach (var kvp in sorters)
                {
                    var finishOrder = g.OrderBy(rem => rem.FinishPosition).ToList();
                    var rankOrder = kvp.Value.Sort(g, null).ToList();
                    int index = 1;
                    for(int i = 3; i < 6 && i < rankOrder.Count; i++)
                    {
                        var finisher = finishOrder[i];
                        var pos = i + 1;
                        var rankPos = rankOrder.IndexOf(finisher);

                        totDiff[kvp.Key] += Math.Abs(i - rankPos);
                        index++;
                    }
                }
                numHorses += g.Count(rem => rem.AvgKmTime > 0);
                numRaces++;
            }
            Console.WriteLine("Finished " + numRaces + " skipped " + skipped + " missingHorses " + numHorsesMissing);
            foreach (var key in sorters.Keys)
            {
                float ratio = ((float)totDiff[key] / numHorses);
                Console.WriteLine($"{key}: {totDiff[key]}/{numHorses} ({Math.Round(ratio, 5)})");
            }
        }
    }
}
