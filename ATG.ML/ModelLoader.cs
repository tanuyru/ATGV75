using ATG.DB;
using ATG.DB.Entities;
using ATG.ML.MLModels;
using ATG.ML.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Travsport.DB;
using TS = Travsport.DB.Entities;
namespace ATG.ML
{
    public static class ModelLoader
    {
        public static Dictionary<long, List<StartHistory>> LoadLookup(IEnumerable<RaceResult> raceresults, Func<RaceResult, long> keySelect)
        {
            Dictionary<long, List<StartHistory>> historyPerHorse = new Dictionary<long, List<StartHistory>>();
            var horseGroups = raceresults.GroupBy(rr => keySelect(rr));
            foreach (var g in horseGroups)
            {
                var horseList = g.Select(rr => new StartHistory(rr)).ToList();
                historyPerHorse.Add(g.Key, horseList);
            }
            return historyPerHorse;

        }
        public static Dictionary<long, List<StartHistory>> LoadLookup(IEnumerable<TS.RaceResult> raceresults, Func<TS.RaceResult, long> keySelect)
        {
            Dictionary<long, List<StartHistory>> historyPerHorse = new Dictionary<long, List<StartHistory>>();
            var horseGroups = raceresults.GroupBy(rr => keySelect(rr));
            foreach (var g in horseGroups)
            {
                var horseList = g.Select(rr => new StartHistory(rr)).ToList();
                historyPerHorse.Add(g.Key, horseList);
            }
            return historyPerHorse;

        }
        public static StartPositionWinStats LoadStartStatsForArena(long? arenaId, DateTime? maxHistoryDate = null)
        {
            using (var context = new AtgContext())
            {
                Console.WriteLine("Loading races");
                var races = context.Races.Include(r => r.RaceResults).Where(r => (!maxHistoryDate.HasValue || r.StartTime > maxHistoryDate.Value) && (!arenaId.HasValue || r.ArenaId == arenaId.Value) && r.WinnerFinishTime > 0 && r.Sport == "trot").ToList();
                return new StartPositionWinStats(races.SelectMany(r => r.RaceResults));
            }
         }
        public static void CompareStartStats(StartPositionWinStats s1, StartPositionWinStats s2)
        {
            s1.GetKeys(out var starts, out var db, out var hb, out var pos);
            s2.GetKeys(out var starts2, out var db2, out var hb2, out var pos2);

            foreach(var k1 in starts.Union(starts2).Distinct())
            {
                Console.WriteLine("StartType " + k1.ToString());
                foreach(var k2 in db.Union(db2).Distinct())
                {
                    Console.WriteLine("Distance-bucket " + k2);
                    foreach(var k3 in hb.Union(hb2).Distinct())
                    {
                        Console.WriteLine("Handicap-bucket " + k3);
                        foreach(var k4 in pos.Union(pos2))
                        {
                            float w1 = s1.GetWinRatio(k1, k2, k3, k4);
                            float w2 = s2.GetWinRatio(k1, k2, k3, k4);
                            if (w1 != 0 || w2 != 0)
                            Console.WriteLine("Pos " + k4 + ", diff: " + (w1 - w2)+" ratio: "+(w1/w2)+" w1: "+w1+" w2: "+w2);
                        }
                    }
                }
            }
        }
            public static List<RelativeModelWrapper> LoadValidModels(List<long> raceIds = null, bool includeUnfinished = false, DateTime? maxHistoryDate = null)
        {
            using (var context = new AtgContext())
            {
                Console.WriteLine("Loading races");
                var races = context.Races.Include(r => r.RaceResults).Where(r => (raceIds == null || raceIds.Contains(r.Id)) && (includeUnfinished || r.WinnerFinishTime > 0) && r.Sport == "trot").ToList();

                List<Race> historyRace = null;
                if (raceIds != null || includeUnfinished)
                {
                    historyRace = context.Races.Include(r => r.RaceResults).Where(r =>  (r.WinnerFinishTime > 0) && r.Sport == "trot").ToList();
                }
                else
                {
                    historyRace = races;
                }
                Console.WriteLine($"Found {races.Count} races with data");

                var allResults = historyRace.SelectMany(r => r.RaceResults);
                Console.WriteLine("Creating horses");
                Dictionary<long, List<StartHistory>> historyPerHorse = LoadLookup(allResults, (r) => r.HorseId);

                Console.WriteLine("Creating trainers?");
                Dictionary<long, List<StartHistory>> historyPerTrainer = LoadLookup(allResults.Where(rr => rr.TrainerId.HasValue), (r) => r.TrainerId.Value);

                Console.WriteLine("Creating drivers?");
                Dictionary<long, List<StartHistory>> historyPerDriver = LoadLookup(allResults, (r) => r.DriverId);

                List<RelativeModelWrapper> models = new List<RelativeModelWrapper>();

                Console.WriteLine("Building start map");
                StartPositionWinStats trackStats = new StartPositionWinStats(races.Where(r => !maxHistoryDate.HasValue || maxHistoryDate.Value > r.StartTime).SelectMany(r => r.RaceResults));
                Console.WriteLine("Starting process of " + races.Count + " races...");
                int counter = 0;
                int logEveryRace = 1000;
                int tot = races.Count;
                int skipped = 0;
                int skipedResults = 0;
                int invalidHistories = 0;
                List<StartHistory> trainerHistory = null;
                foreach(var race in races)
                {
                    counter++;
                    if (counter % logEveryRace == 0)
                    {
                        Console.WriteLine($"{DateTime.Now}: Finished {counter}/{tot}, skipped {skipped}, skippedResults {skipedResults}, invalid {invalidHistories}");
                    }
                    var distinctTracks = race.RaceResults.Select(rr => rr.Track).Distinct();
                    if (distinctTracks.Count() < race.RaceResults.Count || race.RaceResults.Any(rr => rr.DistanceHandicap < 0))
                    {
                        skipped++;
                        continue;
                    }

                    if (race.RaceResults.Count > 16)
                    {
                        skipped++;
                        continue;
                    }
                    List<RelativeModelWrapper> raceModels = new List<RelativeModelWrapper>();
                    bool validRaceHistory = true;
                    List<StarterProfile> profiles = new List<StarterProfile>();
                    foreach(var rr in race.RaceResults)
                    {
                        if (rr.Scratched)
                            continue;
                        if (!historyPerHorse.TryGetValue(rr.HorseId, out var list))
                        {
                            //Console.WriteLine($"ERROR: Cant find history for horse {rr.HorseId}");
                            validRaceHistory = false;
                            continue;
                        }
                        var before = list.Where(sh => sh.RaceDate.AddDays(1) < race.StartTime);
                        if (!before.Any())
                        {
                            //Console.WriteLine($"ERROR: No history before {race.StartTime} for horse {rr.HorseId}");
                            validRaceHistory = false;
                            continue;
                        }
                        if (!rr.TrainerId.HasValue || !historyPerTrainer.TryGetValue(rr.TrainerId.Value, out trainerHistory))
                        {
                            trainerHistory = new List<StartHistory>();
                        }
                        var beforeTrainer = trainerHistory.Where(sh => sh.RaceDate.AddDays(1) < race.StartTime);

                        if (!historyPerDriver.TryGetValue(rr.DriverId, out var driverHistory))
                        {
                            driverHistory = new List<StartHistory>();
                        }
                        var beforeDriver = driverHistory.Where(sh => sh.RaceDate.AddDays(1) < race.StartTime);
                        var profile = new StarterProfile(before, beforeTrainer, beforeDriver, race, rr);
                        profiles.Add(profile);
                    }
                    if (profiles.Count <= 1)
                    {
                        skipped++;
                        continue;
                    }
                    if (!validRaceHistory)
                    {
                        invalidHistories++;
                        //continue;
                    }
                    int db = StarterProfile.GetDistanceBucket(race.Distance);
                    foreach (var profile in profiles)
                    {
                        if (!includeUnfinished && !profile.IsValid)
                        {
                            skipedResults++;
                            continue;
                        }
                        int hb = StarterProfile.GetDistanceHandicapBucket(profile.Result.DistanceHandicap);

                        var others = profiles.ToList();
                        others.Remove(profile);
                        var relMod = new RelativeModel(others, profile, trackStats.GetWinRatio(race.StartType, db, hb, profile.Result.Position));
                        var wrap = new RelativeModelWrapper();
                        wrap.Model = relMod;
                        wrap.RaceDate = race.StartTime;
                        wrap.RaceId = race.Id;
                        wrap.HorseId = profile.HorseId;
                        wrap.WinOdds = profile.WinOdds;
                        wrap.FinishPosition = profile.FinishPosition;
                        raceModels.Add(wrap);
                    }
                    models.AddRange(raceModels);
                }
                Console.WriteLine($"Finished loading {models.Count} models skipped {skipped}, skippedRes {skipedResults}, invalid {invalidHistories}");
                return models;
            }
        }
        public static Dictionary<long, TS.Horse> GetTSHorseDic()
        {
            using (var context = new TravsportContext())
            {
                return context.Horses.ToDictionary(h => h.Id);
            }
        }
        public static List<RelativeModelWrapper> LoadValidModelsTravspotFKRaceIds(string arena, int startNum, int endNum, DateTime date)
        {
            using (var context = new TravsportContext())
            {
                var arenaDic = context.Arenas.ToDictionary(a => a.Name.ToLower());
                var lowerArena = arena.ToLower();
                var arenaId = arenaDic[lowerArena].Id;
                var dateOnly = date.Date;
                var rs = context.Races.Where(r => r.ArenaId.HasValue && r.ArenaId.Value == arenaId && r.StartTime.Date == dateOnly && r.RaceOrder >= startNum && r.RaceOrder <= endNum).ToList();
                Console.WriteLine("Found and loading " + rs.Count + " races");
                return LoadValidModelsTravspot(rs.Select(r => r.Id).ToList(), true);
            }
        }
        public static List<RelativeModelWrapper> LoadValidModelsTravspot(List<long> raceIds = null, bool includeUnfinished = false, DateTime? maxHistoryDate = null)
        {
            using (var context = new TravsportContext())
            {
                Console.WriteLine("Loading races");
                if (raceIds != null)
                {
                    Console.WriteLine("Using raceIds " + string.Join(", ", raceIds));
                }
                var races = context.Races.Include(r => r.RaceResults).Where(r => 
                (raceIds == null || raceIds.Contains(r.Id)) 
                && (includeUnfinished || (r.WinnerFinishTime > 0 && r.LastFinishTime > 0)) 
                && r.Sport == "trot"
                && r.InvalidReason == null
                ).ToList();

                List<TS.Race> historyRace = null;
                if (raceIds != null || includeUnfinished)
                {
                    historyRace = context.Races.Include(r => r.RaceResults).Where(r => (r.WinnerFinishTime > 0) && r.Sport == "trot").ToList();
                }
                else
                {
                    historyRace = races;
                }
                Console.WriteLine($"Found {races.Count} races with data");

                var allResults = historyRace.SelectMany(r => r.RaceResults);
                Console.WriteLine("Creating horses");
                Dictionary<long, List<StartHistory>> historyPerHorse = LoadLookup(allResults, (r) => r.HorseId);

                Console.WriteLine("Creating trainers?");
                Dictionary<long, List<StartHistory>> historyPerTrainer = LoadLookup(allResults.Where(rr => rr.TrainerId.HasValue), (r) => r.TrainerId.Value);

                Console.WriteLine("Creating drivers?");
                Dictionary<long, List<StartHistory>> historyPerDriver = LoadLookup(allResults.Where(rr => rr.DriverId.HasValue), (r) => r.DriverId.Value);

                List<RelativeModelWrapper> models = new List<RelativeModelWrapper>();

                Console.WriteLine("Building start map");
                StartPositionWinStats trackStats = new StartPositionWinStats(races.Where(r => !maxHistoryDate.HasValue || maxHistoryDate.Value > r.StartTime).SelectMany(r => r.RaceResults));
                Console.WriteLine("Starting process of " + races.Count + " races...");
                int counter = 0;
                int logEveryRace = 5000;
                int tot = races.Count;
                int skipped = 0;
                int skipedResults = 0;
                int invalidHistories = 0;
                List<StartHistory> trainerHistory = null;
                foreach (var race in races)
                {
                    counter++;
                    if (counter % logEveryRace == 0)
                    {
                        Console.WriteLine($"{DateTime.Now}: Finished {counter}/{tot}, skipped {skipped}, skippedResults {skipedResults}, invalid {invalidHistories}");
                    }
                    var distinctTracks = race.RaceResults.Select(rr => rr.PositionForDistance).Distinct();
                    if (race.RaceResults.Any(rr => rr.DistanceHandicap < 0))
                    {
                        skipped++;
                        continue;
                    }

                    if (race.RaceResults.Count > 16 || race.RaceResults.Count < 3)
                    {
                        skipped++;
                        continue;
                    }
                    List<RelativeModelWrapper> raceModels = new List<RelativeModelWrapper>();
                    bool validRaceHistory = true;
                    List<StarterProfile> profiles = new List<StarterProfile>();
                    int unscratched = race.RaceResults.Count(rr => !rr.Scratched);
                    foreach (var rr in race.RaceResults)
                    {
                        if (rr.Scratched)
                            continue;
                        if (!historyPerHorse.TryGetValue(rr.HorseId, out var list))
                        {
                            //Console.WriteLine($"ERROR: Cant find history for horse {rr.HorseId}");
                            validRaceHistory = false;
                            list = new List<StartHistory>();
                            if (!includeUnfinished)
                                continue;
                        }
                        var before = list.Where(sh => sh.RaceDate.AddDays(1) < race.StartTime);
                        if (!before.Any())
                        {
                            //Console.WriteLine($"ERROR: No history before {race.StartTime} for horse {rr.HorseId}");
                            validRaceHistory = false;
                            if (!includeUnfinished)
                                continue;
                        }
                        if (!rr.TrainerId.HasValue || !historyPerTrainer.TryGetValue(rr.TrainerId.Value, out trainerHistory))
                        {
                            trainerHistory = new List<StartHistory>();
                        }
                        var beforeTrainer = trainerHistory.Where(sh => sh.RaceDate.AddDays(1) < race.StartTime);

                        if (!rr.DriverId.HasValue || !historyPerDriver.TryGetValue(rr.DriverId.Value, out var driverHistory))
                        {
                            driverHistory = new List<StartHistory>();
                        }
                        var beforeDriver = driverHistory.Where(sh => sh.RaceDate.AddDays(1) < race.StartTime);
                        var profile = new StarterProfile(before, beforeTrainer, beforeDriver, race, rr);
                        profiles.Add(profile);
                    }
                    if (profiles.Count <= 1)
                    {
                        skipped++;
                        continue;
                    }
                    if (!validRaceHistory)
                    {
                        invalidHistories++;
                        if (!includeUnfinished)
                        {
                            continue;
                        }
                    }
                    int db = StarterProfile.GetDistanceBucket(race.Distance);
                    foreach (var profile in profiles)
                    {
                        if (!includeUnfinished && !profile.IsValid)
                        {
                            skipedResults++;
                            continue;
                        }
                        int hb = StarterProfile.GetDistanceHandicapBucket(profile.DistanceHandicapBucket);

                        var others = profiles.ToList();
                        others.Remove(profile);
                        var relMod = new RelativeModel(others, profile, trackStats.GetWinRatio(race.StartType, db, hb, profile.PositionInDistance));
                        relMod.NumHorses = unscratched;
                        var wrap = new RelativeModelWrapper();
                        wrap.Model = relMod;
                        wrap.RaceDate = race.StartTime;
                        wrap.RaceId = race.Id;
                        wrap.HorseId = profile.HorseId;
                        wrap.WinOdds = profile.WinOdds;
                        wrap.RaceNumber = profile.RaceNumber;
                        wrap.FinishPosition = profile.FinishPosition;
                        raceModels.Add(wrap);
                    }
                    models.AddRange(raceModels);
                }
                Console.WriteLine($"Finished loading {models.Count} models skipped {skipped}, skippedRes {skipedResults}, invalid {invalidHistories}");
                return models;
            }
        }

        public static List<WinOddsTimeModel> LoadValidWinOddsModels()
        {
            using (var context = new TravsportContext())
            {
                var races = context.Races.Include(r => r.RaceResults).Where(r =>
            
                (r.WinnerFinishTime > 0)
                && r.Sport == "trot"
                && r.LastFinishTime != 0 && r.InvalidReason == null
                ).ToList();
                List<WinOddsTimeModel> model = new List<WinOddsTimeModel>();
                var allResults = races.SelectMany(r => r.RaceResults.Where(rr => rr.WinOdds > 0)).GroupBy(rr => rr.RaceFKId);
                int skipped = 0;
                int count = 0;
                Console.WriteLine("Processing " + allResults.Count() + " races");
                foreach(var raceGroup in allResults)
                {
                    count++;
                    if (count % 500 == 0)
                    {
                        Console.WriteLine("Count " + count + " Skipped " + skipped+" loaded "+model.Count);
                    }
                    var overround = raceGroup.Sum(rr => 1.0 / rr.WinOdds);
                    if (overround < 1)

                    {
                        Console.WriteLine($"Overround {overround} found on race {raceGroup.Key}, skipping " + skipped);
                        skipped++;
                        continue;
                    }
                    foreach(var rr in raceGroup)
                    {
                        if (rr.FinishTimeMilliseconds == 0 || rr.DQ || rr.Scratched)
                                continue;
                        WinOddsTimeModel m = new WinOddsTimeModel();
                        m.WinOddsProbability = (float)(1.0 / (rr.WinOdds * overround));
                        m.TimeAfterWinner = (float)rr.FinishTimeAfterWinner;
                        model.Add(m);
                    }
                }
                Console.WriteLine("Loaded " + model.Count + " models skipped " + skipped);
                return model;
            }
        }
    }
}
