using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Travsport.DB.Entities;
using Travsport.DB.Entities.Util;

namespace Travsport.DB
{
    public class DBFacade
    {
        public static void RelateResultsToStats()
        {
            int inserted = 1;
            int batchSize = 300000;
            while (inserted > 0)
            {
                using (var context = new TravsportContext())
                {
                    Console.WriteLine(DateTime.Now + ": Loading stats");
                    var stats = context.HorseStats.Include(hs => hs.RaceResult).Where(hs => !hs.Keyed.HasValue).Take(batchSize).ToList();
                    if (stats.Count == 0)
                        break;
                    Console.WriteLine("Loaded " + stats.Count + " stats");
                    foreach (var hs in stats)
                    {
                        hs.RaceResult.HorseStatsId = hs.Id;
                        hs.Keyed = true;
                    }
                    var toUpate = stats.Select(hs => hs.RaceResult).ToList();
                    Console.WriteLine(DateTime.Now + ": Updating " + toUpate.Count + " raceResults");
                    context.BulkUpdate(toUpate);
                    Console.WriteLine(DateTime.Now + ": Updating "+stats.Count+" stats");
                    context.BulkUpdate(stats);
                    Console.WriteLine(DateTime.Now + ": Finished");
                    inserted = stats.Count;
                }
            }
        }
        public static int CreateDetails(int numRaces)
        {
            int count = 0;

            using (var context = new TravsportContext())
            {
                Console.WriteLine(DateTime.Now + ": Loading " + numRaces);

                var races = context.Races.Where(r => r.WinnerFinishTime > 0 && r.StartType != ATG.Shared.Enums.StartTypeEnum.Unknown && r.DetailStatsVersion == 0 && r.LastFinishTime > 0 && r.Sport == "trot").Include(r => r.RaceResults).OrderByDescending(r => r.StartTime).Take(numRaces).ToList();
                var horseIds = races.SelectMany(r => r.RaceResults.Select(rr => rr.HorseId));
                var driverIds = races.SelectMany(r => r.RaceResults.Where(rr => rr.DriverId.HasValue).Select(rr => rr.DriverId.Value)).ToList();
                Console.WriteLine("Loaded " + races.Count + " races");
                Console.WriteLine(DateTime.Now + ": Loading history");

                var historyRaces = context.Races.Where(r => 
                    r.WinnerFinishTime > 0 &&
                    r.LastFinishTime > 0 &&
                    r.Sport == "trot" &&
                    r.StartType != ATG.Shared.Enums.StartTypeEnum.Unknown
                   // && (r.RaceResults.Any(rr => 
                     //   horseIds.Contains(rr.HorseId) 
                       // || (rr.DriverId.HasValue && driverIds.Contains(rr.DriverId.Value))
                    
                ).Include(r => r.RaceResults).ToList();
                Console.WriteLine(DateTime.Now + ": Loaded history races " + historyRaces.Count);
                Console.WriteLine(DateTime.Now + ": Loading horseHistory");
                var horseHistory = LoadLookup(historyRaces.SelectMany(r => r.RaceResults.Where(rr => !rr.Scratched)), (rr) => rr.HorseId);
                Console.WriteLine(DateTime.Now + ": Loading driverHistory");
                var driverHistory = LoadLookup(historyRaces.SelectMany(r => r.RaceResults.Where(rr => !rr.Scratched && rr.DriverId.HasValue)), (rr) => rr.DriverId.Value);
                List<HorseStatsProfile> newProfiles = new List<HorseStatsProfile>();
                List<HorseStats> newStats = new List<HorseStats>();
                List<Race> updatedRaces = new List<Race>();

                Dictionary<long, Dictionary<long, HorseStats>> horseStats = new Dictionary<long, Dictionary<long, HorseStats>>();
                Dictionary<long, Dictionary<long, Dictionary<string, HorseStatsProfile>>> profiles = new Dictionary<long, Dictionary<long, Dictionary<string, HorseStatsProfile>>>();
                Console.WriteLine(DateTime.Now + ": Processing " + races.Count + " races");
                foreach(var r in races)
                {
                    count++;
                    if (count % 1000 == 0)
                    {
                        Console.WriteLine(DateTime.Now+": RaceCounter is " + count);
                    }
                    Dictionary<long, HorseStats> raceHorseStats = new Dictionary<long, HorseStats>();
                    Dictionary<long, Dictionary<string, HorseStatsProfile>> raceProfiles = new Dictionary<long, Dictionary<string, HorseStatsProfile>>();
                    var rStats = new RaceStats(r);
                    foreach(var raceResult in r.RaceResults.Where(rr => !rr.Scratched))
                    {
                        if (!horseHistory.TryGetValue(raceResult.HorseId, out var horseList))
                        {
                            horseList = new List<HorseRaceResult>();
                        }
                        if (!driverHistory.TryGetValue(raceResult.DriverId.Value, out var driverList))
                        {
                            driverList = new List<HorseRaceResult>();
                        }
                        HorseStats stats = new HorseStats(raceResult.HorseId, raceResult.Id, rStats, horseList.Where(hr => hr.RaceTimestamp < rStats.RaceTimestamp).ToList(), driverList.Where(hr => hr.RaceTimestamp < rStats.RaceTimestamp).ToList());
                        newStats.Add(stats);
                        Dictionary<string, HorseStatsProfile> horseProfiles = new Dictionary<string, HorseStatsProfile>();
                        if (stats.KmTimeValidProfile != null)
                        {
                            newProfiles.Add(stats.KmTimeValidProfile);
                            horseProfiles.Add("KmTimeValidProfile", stats.KmTimeValidProfile);
                        }
                        if (stats.TimeAfterWinnerLastCapProfile != null)
                        {
                            newProfiles.Add(stats.TimeAfterWinnerLastCapProfile);
                            horseProfiles.Add("TimeAfterWinnerLastCapProfile", stats.KmTimeValidProfile);

                        }
                        if (stats.TimeAfterWinnerMinMaxProfile != null)
                        {
                            newProfiles.Add(stats.TimeAfterWinnerMinMaxProfile);
                            horseProfiles.Add("TimeAfterWinnerMinMaxProfile", stats.KmTimeValidProfile);

                        }
                        if (stats.TimeAfterWinnerNormMinPlacedCapProfile != null)
                        {
                            newProfiles.Add(stats.TimeAfterWinnerNormMinPlacedCapProfile);
                            horseProfiles.Add("TimeAfterWinnerNormMinPlacedCapProfile", stats.KmTimeValidProfile);

                        }
                        if (stats.TimeAfterWinnerPlaceCapProfile != null)
                        {
                            newProfiles.Add(stats.TimeAfterWinnerPlaceCapProfile);
                            horseProfiles.Add("TimeAfterWinnerPlaceCapProfile", stats.KmTimeValidProfile);
                        }
                        raceProfiles.Add(raceResult.HorseId, horseProfiles);
                        raceHorseStats.Add(raceResult.HorseId, stats);
                    }
                    horseStats.Add(r.Id, raceHorseStats);
                    profiles.Add(r.Id, raceProfiles);
                    r.DetailStatsVersion = 1;
                    updatedRaces.Add(r);
                }
                var outputIdCfg = new BulkConfig();
                outputIdCfg.SqlBulkCopyOptions = Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepNulls;
                outputIdCfg.SetOutputIdentity = true;
                Console.WriteLine(DateTime.Now+": Inserting " + newProfiles.Count + " new profiles");
                var profileList = newProfiles;// profiles.SelectMany(kvp1 => kvp1.Value.SelectMany(kvp2 => kvp2.Value.Select(kvp3 => kvp3.Value))).ToList();
                context.BulkInsert(profileList, outputIdCfg);
                List<HorseStats> addingStats = new List<HorseStats>();
                var profileDic = profileList.ToDictionary(pl => pl.TempId);
                foreach(var key in horseStats.Keys)
                {
                    var hsDic = horseStats[key];
                    var profDic = profiles[key];
                    foreach(var key2 in hsDic.Keys)
                    {
                        var statsForHorse = hsDic[key2];
                        var horseProfDic = profDic[key2];
                        if (statsForHorse.KmTimeValidProfile != null)
                        {
                            statsForHorse.KmTimeValidProfileId = profileDic[statsForHorse.KmTimeValidProfileIdTemp.Value].Id;
                        }


                        if (statsForHorse.TimeAfterWinnerLastCapProfile != null)
                        {
                            statsForHorse.TimeAfterWinnerLastCapProfileId = profileDic[statsForHorse.TimeAfterWinnerLastCapProfileIdTemp.Value].Id;
                        }
                        if (statsForHorse.TimeAfterWinnerMinMaxProfile != null)
                        {
                            statsForHorse.TimeAfterWinnerMinMaxProfileId = profileDic[statsForHorse.TimeAfterWinnerMinMaxProfileIdTemp.Value].Id;
                        }
                        if (statsForHorse.TimeAfterWinnerNormMinPlacedCapProfile != null)
                        {
                            statsForHorse.TimeAfterWinnerNormMinPlacedCapProfileId = profileDic[statsForHorse.TimeAfterWinnerNormMinPlacedCapProfileIdTemp.Value].Id;
                        }
                        if (statsForHorse.TimeAfterWinnerPlaceCapProfile != null)
                        {
                            statsForHorse.TimeAfterWinnerPlaceCapProfileId = profileDic[statsForHorse.TimeAfterWinnerPlaceCapProfileIdTemp.Value].Id;
                        }
                        addingStats.Add(statsForHorse);
                    }
                }
                Console.WriteLine(DateTime.Now + ": Inserting " + newStats.Count + " new stats");
                context.BulkInsert(addingStats, outputIdCfg);
                Console.WriteLine(DateTime.Now + ": Updating " + updatedRaces.Count + " races");
                context.BulkUpdate(updatedRaces);
                Console.WriteLine(DateTime.Now + ": Done");
            }
            return count;
        }
        public static Dictionary<long, List<HorseRaceResult>> LoadLookup(IEnumerable<RaceResult> raceresults, Func<RaceResult, long> keySelect)
        {
            Dictionary<long, List<HorseRaceResult>> historyPerHorse = new Dictionary<long, List<HorseRaceResult>>();
            var horseGroups = raceresults.GroupBy(rr => keySelect(rr));
            foreach (var g in horseGroups)
            {
                var horseList = g.Select(rr => new HorseRaceResult(rr.Race, rr)).ToList();
                historyPerHorse.Add(g.Key, horseList);
            }
            return historyPerHorse;

        }
    }
}
