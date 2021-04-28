using ATG.DB;
using ATG.DB.Entities;
using ATG.ML.MLModels;
using ATG.ML.Models;
using ATG.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ATG.ML
{
    public class RaceLoader
    {
        public RaceLoader()
        {
        }

        public List<HorseRaceEntry> GetHorseRaceEntries(GameTypeEnum gameType)
        {
            List<HorseRaceEntry> raceEntries = new List<HorseRaceEntry>();
            using (var context = new AtgContext())
            {

                var allResults = context.RaceResults.Where(rr => rr.Distributions.Any(gd => gameType == gd.Game.GameType))
                    .Include(rr => rr.Horse)
                    .Include(rr => rr.Race)
                    .Include(rr => rr.Distributions).ThenInclude(gd => gd.Game)
                    .ToList();
                var allHorses = allResults.Select(rr => rr.Horse.Id).Distinct();
                var historyDictionary = GetResultsForHorses(allHorses, context);
                Console.Write("Finished getting horse-data parsing stuffs");

                foreach(var raceGroup in allResults.GroupBy(rr => rr.Race.Id))
                {

                    var bestStarter = raceGroup.OrderByDescending(rr => rr.Distributions.Single(gd => gd.Game.GameType == gameType).Distribution).First();
                    var worstStarter = raceGroup.OrderBy(rr => rr.Distributions.Single(gd => gd.Game.GameType == gameType).Distribution).First();
                    int numHorses = raceGroup.Where(rr => !rr.Scratched).Count();

                    List<double> horseKmTimes = new List<double>();
                    List<double> horseAvgPos = new List<double>();

                    List<RaceResult> raceHistory = new List<RaceResult>();
                    foreach(var rr in raceGroup)
                    {
                        if (!historyDictionary.ContainsKey(rr.Horse.Id))
                            continue;
                        var history = historyDictionary[rr.Horse.Id];
                        if (history.Count == 0)
                            continue;
                        horseKmTimes.Add(history.Average(rr => rr.KmTimeMilliSeconds));
                        horseAvgPos.Add(history.Average(rr => rr.FinishPosition));
                        raceHistory.AddRange(history);
                    }
                    var raceAvgKmTime = (long)horseKmTimes.Average();
                    var raceAvgPos = horseAvgPos.Average();
                    var topRaceTime = raceHistory.Min(rr => rr.KmTimeMilliSeconds);
                    var bottoRaceTime = raceHistory.Max(rr => rr.KmTimeMilliSeconds);
                    var topAvgRaceTime = horseKmTimes.Min();
                    foreach (var rr in raceGroup)
                    {
                        var entry = ParseRaceEntry(rr.Race);
                        if (!historyDictionary.TryGetValue(rr.Horse.Id, out var horseResults))
                        {
                            horseResults = new List<RaceResult>();
                        }

                        if (horseResults.Count > 0)
                        {
                            entry.AvgKmTime = (float)horseResults.Average(rr => rr.KmTimeMilliSeconds);
                            entry.AvgFinishPosition = (float)horseResults.Average(rr => Math.Min(rr.FinishPosition, 15));
                            entry.LastFinishPosition = (float)horseResults.First().FinishPosition;
                            entry.HorseWinPercent = (float)horseResults.Count(rr => rr.FinishPosition == 1) / (float)horseResults.Count;
                            entry.HorsePlacePercent = (float)horseResults.Count(rr => rr.FinishPosition < 4) / (float)horseResults.Count;
                        }
                        if (rr.BackChange.HasValue)
                            entry.BackChanged = rr.BackChange.Value ? 1 : 0;
                        if (rr.BackShoes.HasValue)
                            entry.ShoesBack = rr.BackShoes.Value ? 1 : 0;
                        entry.BestHorseOnTrack = bestStarter.Track;

                        if (rr.Driver != null)
                            entry.DriverId = rr.Driver.Id;

                        if (rr.FrontChange.HasValue)
                            entry.FrontChanged = rr.FrontChange.Value ? 1 : 0;
                        if (rr.FrontShoes.HasValue)
                            entry.ShoesFront = rr.FrontShoes.Value ? 1 : 0;

                        entry.HorseAge = rr.Race.StartTime.Year - rr.Horse.BirthYear;
                        //entry.HorseGender = rr.Horse.Gender;
                        entry.HorseId = rr.Horse.Id;
                        entry.KmTime = rr.KmTimeMilliSeconds;
               
                        entry.RaceAvgKmTime = raceAvgKmTime;
                        entry.RaceAvgPosition = (float)raceAvgPos;
                        entry.RaceBestKmTime = topRaceTime;
                        entry.RaceTopAvgKmTime = (long)topAvgRaceTime;
                        entry.WinOdds = (float)rr.WinOdds;

                        raceEntries.Add(entry);
                    }
                }
            }
            return raceEntries;
        }
        private HorseRaceEntry ParseRaceEntry(Race r)
        {
            HorseRaceEntry entry = new HorseRaceEntry();
            if (r.Arena != null)
                entry.ArenaId = r.Arena.Id;
            entry.Distance = r.Distance;
            entry.IsVoltStart = r.StartType == StartTypeEnum.Volt ? 1 : 0;
            return entry;
        }
        public List<RaceEntryModel> Load(GameTypeEnum gt)
        {
            List<RaceEntryModel> results = new List<RaceEntryModel>();
            using (var context = new AtgContext())
            {
                //var horseDic = context.HorseStats.Where(hs => hs.NumMedium + hs.NumShort + hs.NumLong > 0).ToDictionary(hs => hs.HorseId);
                Dictionary<string, RaceModel> races = new Dictionary<string, RaceModel>();

                var allResults = context.RaceResults.Where(rr => rr.Distributions.Any(gd => gd.Game.GameType == gt))
                    .Include(rr => rr.Horse)
                    .Include(rr => rr.Race)
                    .Include(rr => rr.Distributions).ThenInclude(gd => gd.Game)
                    .ToList();
                var allHorses = allResults.Select(rr => rr.Horse.Id).Distinct();
                var historyDictionary = GetResultsForHorses(allHorses, context);
                Console.Write("Finished getting horse-data parsing stuffs");
                foreach (var raceGroup in context.RaceResults.Where(rr => rr.Distributions.Any(gd => gd.Game.GameType == gt))
                    .Include(rr => rr.Horse)
                    .Include(rr => rr.Race)
                    .Include(rr => rr.Distributions).ThenInclude(gd => gd.Game)
                    .ToList()
                    .GroupBy(rr => rr.Race.Id))
                {
                    
                    List<RaceEntryModel> raceModels = new List<RaceEntryModel>();
                    foreach (var rr in raceGroup)
                    {
                        RaceEntryModel rem = new RaceEntryModel();
                        if (!races.TryGetValue(rr.Race.RaceId, out var rm))
                        {
                            rm = GetRaceModel(rr);
                            races.Add(rr.Race.RaceId, rm);
                        }
                        /*
                        if (horseDic.TryGetValue(rr.Horse.Id, out var stats))
                        {
                            rem.AvgKmTime = stats.GetAvgTotal();
                            if (stats.LastMonth.HasValue)
                                rem.AvgRecentTime = stats.LastMonth.Value;
                            if (stats.ShortTime.HasValue)
                                rem.ShortAvgKmTime = stats.ShortTime.Value;

                            if (stats.MediumTime.HasValue)
                                rem.MediumAvgKmTime = stats.MediumTime.Value;

                            if (stats.LongTime.HasValue)
                                rem.LongAvgKmTime = stats.LongTime.Value;
                        }
                        else
                        {
                            continue;
                        }*/
                        if (!historyDictionary.TryGetValue(rr.Horse.Id, out var historyList))
                        {
                            Console.WriteLine("No history found for " + rr.Horse.Id);
                            continue;
                        }
                        var last3 = historyList.Where(raceResult => raceResult.Race.StartTime < rr.Race.StartTime && rr.FinishPosition < 16)
                            .OrderByDescending(raceResult => raceResult.Race.StartTime).Take(3).ToList();
                        if (last3.Any())
                        {
                            rem.LastFinishPosition = last3.First().FinishPosition;
                            rem.Last3AvgFinishPosition = last3.Average(rem => rem.FinishPosition);
                        }
                        rem.Distribution = rr.Distributions.First(gd => gd.Game.GameType == gt).Distribution;
                        rem.FinishPosition = rr.FinishPosition;
                        rem.RaceId = rr.Race.RaceId;
                        rem.StartGroup = 1; // FIX
                        rem.StartNumber = rr.Track;
                        rem.HorseId = rr.Horse.Id;
                        rem.HorseName = rr.Horse.Name;
                        rem.Race = rm;
                        raceModels.Add(rem);
                    }
                    foreach(var rm in raceModels)
                    {
                        rm.DistRank = GetDistrRank(raceModels, rm.HorseId);
                    }
                    results.AddRange(raceModels);
                }
            }
            return results;
        }
        private int GetDistrRank(IEnumerable<RaceEntryModel> rrs, long horseId)
        {
            var rr = rrs.Single(rr => rr.HorseId == horseId);
            int rank = 1;
            foreach(var other in rrs.OrderByDescending(raceResult => raceResult.Distribution))
            {
                if (other.HorseId == rr.HorseId)
                    continue;
                if (other.Distribution < rr.Distribution)
                    break;
                rank++;
            }
            return rank;
        }
        private RaceModel GetRaceModel(RaceResult rr)
        {
            var rm = new RaceModel();
            rm.Distance = rr.Race.Distance;
            rm.StartType = rr.Race.StartType;
            
            return rm;
        }

        private Dictionary<long, List<RaceResult>> GetResultsForHorses(IEnumerable<long> horseIds, AtgContext context)
        {
            var allRaceResults = context.RaceResults.Where(rr => rr.KmTimeMilliSeconds > 0 && rr.FinishPosition > 0 && horseIds.Contains(rr.Horse.Id)).Include(rr => rr.Race).Include(rr => rr.Horse).ToList().GroupBy(rr => rr.Horse.Id);

            Dictionary<long, List<RaceResult>> results = new Dictionary<long, List<RaceResult>>();
            foreach(var g in allRaceResults)
            {
                results.Add(g.Key, g.OrderByDescending(rr => rr.Race.StartTime).ToList());
            }
            return results;
        }

        public const int MinRecentWinRatio = 1;
        public List<TimeAfterWinnerModel> LoadTimeWinnerModels()
        {
            List<TimeAfterWinnerModel> list = new List<TimeAfterWinnerModel>();
            using (var context = new AtgContext())
            {
                /*
                var allResults = context.RaceResults.Include(rr => rr.Distributions).Include(rr => rr.RecentStarts).Include(rr => rr.Race)
                    .ThenInclude(r => r.Arena)
                    .Include(rr => rr.Horse)
                    .Where(rr => rr.RecentStarts.Count > 3).ToList();
                var arenas = context.Arenas.Include(arena => arena.Races).ThenInclude(race => race.RaceResults).ToList();
               // Console.WriteLine("Processing " + allResults.Count + " results");
                var trackInfoDictionary = arenas.ToDictionary(a => a.Id, a => new TrackWinInfo(a.Races.SelectMany(r => r.RaceResults.Where(rr => rr.Race.Arena.Id == a.Id))));

                int count = 0;
                var grouped = allResults.GroupBy(rr => rr.Race.Id);
                var groupedByHorse = allResults.GroupBy(rr => rr.Horse.Id).ToDictionary(g => g.Key,g => g.ToList());
                int totRaces = grouped.Count();
               foreach (var raceGroup in grouped)
                {
                    count++;
                    if (count % 25 == 0)
                    {
                        Console.WriteLine(DateTime.Now.ToLongTimeString()+": " + count + "/"+totRaces+" races");
                    }
                    List<double> raceMedianTime = new List<double>();
                    List<double> raceWinAverages = new List<double>();
                    List<TimeAfterWinnerModel> raceModels = LoadModelsForRace(raceGroup, groupedByHorse, trackInfoDictionary, out var tmp, false);
                    list.AddRange(raceModels);
                }
                */
            }

            return list;
        }

        public static List<TimeAfterWinnerModel> LoadModelsForRace(IGrouping<long, RaceResult> results, Dictionary<long, List<RaceResult>> groupedByHorse,
            Dictionary<long, TrackWinInfo> trackDic, out List<string> horseNames, bool future = false)
        {
            horseNames = new List<string>();
            List<double> raceMedianTime = new List<double>();
            List<double> raceWinAverages = new List<double>();
            List<TimeAfterWinnerModel> raceModels = new List<TimeAfterWinnerModel>();
            var validTimes = results.Where(rr => rr.KmTimeMilliSeconds > 0).Select(rr => rr.KmTimeMilliSeconds);
            long bestTime = 0;
            if (validTimes.Any())
            {
                bestTime = validTimes.Min();
            }
            foreach (var raceResult in results)
            {
                TimeAfterWinnerModel m = new TimeAfterWinnerModel();
                m.ArenaDistanceRel = 0.5f;
                //if (raceResult.RecentStarts.Count == 0 || (!future && raceResult.KmTimeMilliSeconds == 0) || raceResult.Galopp || raceResult.DQ)
                {
                    continue;
                }
               // m.DaysSinceLastRace = (int)(raceResult.Race.StartTime - raceResult.RecentStarts.First().Date).TotalDays;

                m.DriverWinPercentRel = 0.5f;
                if (raceResult.BackChange.HasValue && raceResult.FrontChange.HasValue)
                    m.EquipmentChange = (raceResult.BackChange.Value || raceResult.FrontChange.Value) ? 1 : 0;
                //m.GallopedLast = raceResult.RecentStarts.First().Galloped ? 1 : 0;
                if (!future)
                {
                    m.TimeAfterWinner = raceResult.KmTimeMilliSeconds - bestTime;
                }
                var horseResults = groupedByHorse[raceResult.Horse.Id];
                var untillNow = horseResults.Where(rr => rr.Race.StartTime < raceResult.Race.StartTime);
                if (untillNow.Count() > MinRecentWinRatio)
                {
                    var tot = untillNow.Count();
                    var wins = untillNow.Count(rr => rr.FinishPosition == 1);
                    double ratio = (double)(wins / (double)tot);
                    raceWinAverages.Add(ratio);
                    m.HorseWinPercentRel = (float)ratio;
                }
                else if (horseResults.Count(hr => hr.KmTimeMilliSeconds > 0) > MinRecentWinRatio)
                {
                    var tot = horseResults.Count();
                    var wins = horseResults.Count(rr => rr.FinishPosition == 1);
                    double ratio = (double)(wins / (double)tot);
                    raceWinAverages.Add(ratio);
                    m.HorseWinPercentRel = (float)ratio;
                }
                else if (!future)
                {
                    continue;
                }
                /*
                if (raceResult.RecentStarts.Any(rs => rs.KmTimeMilliseconds > 0))
                {
                    var median = raceResult.RecentStarts.Where(rs => rs.KmTimeMilliseconds > 0).Select(rs => rs.KmTimeMilliseconds).GetMedian();
                    raceMedianTime.Add(median);
                    m.MedianRelSpeed = (float)median;
                    m.RecentRelSpeed = (float)median;
                }*/
                if (!trackDic.TryGetValue(raceResult.Race.Arena.Id, out var trackInfo))
                {
                    trackInfo = trackDic.First().Value;
                }

                if (!trackInfo.WinPerTrackAndDistance.TryGetValue(raceResult.Track, out var dic))
                {
                    dic = trackInfo.WinPerTrackAndDistance.First().Value;
                }

                if (dic.TryGetValue(raceResult.Race.Distance, out var win))
                {
                    m.TrackWinAtDistance = (float)win;
                }

                if (raceResult.Distributions.Any())
                {
                    m.Distribution = (float)raceResult.Distributions.First().Distribution;
                }
                if (raceResult.FinishPosition > 0 && raceResult.FinishPosition < 16)
                {
                    m.FinishPosition = raceResult.FinishPosition;
                }
                else
                {
                    m.FinishPosition = 16;
                }
                m.RaceId = raceResult.Race.Id;
                //m.Debug = raceResult.Horse.Name;
                horseNames.Add(raceResult.Track+": "+raceResult.Horse.Name);
                raceModels.Add(m);
            }
            if (raceModels.Count == 0 || raceMedianTime.Count == 0 || raceWinAverages.Count == 0)
                return raceModels;
            double minWin = raceWinAverages.Min();
            double winDiff = raceWinAverages.Max() - raceWinAverages.Min();

            double minTime = raceMedianTime.Min();
            double timeDiff = raceMedianTime.Max() - minTime;
            foreach (var m in raceModels)
            {
                m.MedianRelSpeed = m.MedianRelSpeed.Normalize((float)minTime, (float)timeDiff);
                m.RecentRelSpeed = m.RecentRelSpeed.Normalize((float)minTime, (float)timeDiff);
                m.HorseWinPercentRel = m.HorseWinPercentRel.Normalize((float)minWin, (float)winDiff);
            }
            return raceModels;
        }
        public static List<RaceResultModel> LoadRaceResultModels(DateTime? maxDate = null)
        {
            int skipped = 0;
            int counter = 0;
            List<RaceResultModel> list = new List<RaceResultModel>();
            using (var context = new AtgContext())
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: Loading results");
                var races = context.Races.Where(r => r.Sport == "trot" && r.WinnerFinishTime > 0 && (!maxDate.HasValue || maxDate.Value > r.ScheduledStartTime)).Include(race => race.RaceResults).ToList();
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: Loading recents");
                var recentHorses = context.RecentHorseStarts.ToList().GroupBy(rs => rs.HorseId).ToDictionary(g => g.Key,g => g.ToList());

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
              
                int logAfter = 5000;
                foreach(var r in races)
                {
                    if (r.RaceResults.Any(rr => (!rr.DQ && rr.KmTimeMilliSeconds > 0) && (!recentHorses.TryGetValue(rr.HorseId, out var list) || list.Where(rs => rs.KmTimeMilliseconds > 0).Count() < 2)))
                    {
                        skipped++;
                        continue;
                    }
                    
                    TrackWinInfo twi = null;
                    if (r.StartType == StartTypeEnum.Volt)
                        twi = trackDicPerDistanceVolt[RaceResultModel.GetDistanceBucket(r.Distance)];
                    else if (r.StartType == StartTypeEnum.Auto)
                        twi = trackDicPerDistance[RaceResultModel.GetDistanceBucket(r.Distance)];
                    else
                    {
                        skipped++;
                        continue;
                    }
                    var models = LoadRaceModels(r, twi, recentHorses);
                    list.AddRange(models);
                    if (counter % logAfter == 0)
                    {
                        Console.WriteLine($"Loaded " + counter + " races and " + list.Count + " results skipped "+skipped);
                    }
                    counter++;
                }
            }
            Console.WriteLine($"Skipped " + skipped + " races and loaded " + counter);
            Console.WriteLine($"Found {RaceResultModel.NumFasterThanWinner} faster than winner");
            return list;
        }

        public static List<RaceResultModel> LoadRaceModels(Race race, TrackWinInfo winPerTrack, Dictionary<long, List<RecentHorseStart>> recents, int numRecentToUse = 4, bool future = false)
        {
            List<RecentHorseStart> allRecents = new List<RecentHorseStart>();
            foreach(var horseId in race.RaceResults.Select(rr => rr.HorseId))
            {
                if (recents.TryGetValue(horseId, out var list))
                {
                    allRecents.AddRange(list.Take(numRecentToUse));
                }
            }
            List<RaceResultModel> models = new List<RaceResultModel>();
            foreach(var rr in race.RaceResults)
            {
                if (rr.KmTimeMilliSeconds == 0 && !future)
                    continue;
                if (!recents.TryGetValue(rr.HorseId, out var list))
                {
                    list = new List<RecentHorseStart>();
                }
                list = list.Where(rs => rs.Date < race.ScheduledStartTime.Date).OrderByDescending(rs => rs.Date).ToList();
                var trackWinPercent = winPerTrack.WinPerTrack[rr.Track];
                var m = new RaceResultModel(race, rr, list, allRecents, rr.Distribution, list.Any() && list.First().Galloped, trackWinPercent);
                models.Add(m);
            }
            if (models.Count == 0)
            {
                return models;
            }
            RaceResultModel.NormalizeRelative(models);
            return models.Where(rm => !rm.DQ && (future || rm.KmTimeMilliSeconds > 0) && !rm.Scratched && (future || rm.WinOdds > 0)).ToList();
        }
    }
}
