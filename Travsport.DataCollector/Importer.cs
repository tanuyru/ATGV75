using ATG.ML.Models;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Travsport.DB;
using Travsport.DB.Entities;
using Travsport.WebParser.Json;

namespace Travsport.DataCollector
{
    public static class Importer
    {
        static Regex tempoRegex = new Regex(@"första 500 (.+?) \((.+?)\), 1000 (.+?) \((.+?)\) sista 500 (.*)");

        static Dictionary<string, int> monthNameToNumber = new Dictionary<string, int>();

        static Dictionary<string, int> atgArenaId = new Dictionary<string, int>();
        static Importer()
        {
            monthNameToNumber.Add("JANUARI", 1);
            monthNameToNumber.Add("FEBRUARI", 2);
            monthNameToNumber.Add("MARS", 3);
            monthNameToNumber.Add("APRIL", 4);
            monthNameToNumber.Add("MAJ", 5);
            monthNameToNumber.Add("JUNI", 6);
            monthNameToNumber.Add("JULI", 7);
            monthNameToNumber.Add("AUGUSTI", 8);
            monthNameToNumber.Add("SEPTEMBER", 9);
            monthNameToNumber.Add("OKTOBER", 10);
            monthNameToNumber.Add("NOVEMBER", 11);
            monthNameToNumber.Add("DECEMBER", 12);

            atgArenaId.Add("SOLVALLA", 5);
            atgArenaId.Add("ÅBY", 6);
            atgArenaId.Add("JÄGERSRO", 7);
            atgArenaId.Add("AXEVALLA", 8);
            atgArenaId.Add("BERGSÅKER", 9);
            atgArenaId.Add("BODEN", 11);
            atgArenaId.Add("BOLLNÄS", 12);
            atgArenaId.Add("DANNERO", 13);
            atgArenaId.Add("ESKILSTUNA", 14);
            atgArenaId.Add("FÄRJESTAD", 15);
            atgArenaId.Add("GÄVLE", 16);
            atgArenaId.Add("HAGMYREN", 17);
            atgArenaId.Add("HALMSTAD", 18);
            atgArenaId.Add("KALMAR", 19);
            atgArenaId.Add("LINDESBERG", 21);
            atgArenaId.Add("MANTORP", 22);
            atgArenaId.Add("ROMME", 23);
            atgArenaId.Add("RÄTTVIK", 24);
            atgArenaId.Add("SKELLEFTEÅ", 25);
            atgArenaId.Add("SOLÄNGET", 26);
            atgArenaId.Add("UMÅKER", 27);
            atgArenaId.Add("VISBY", 28);
            atgArenaId.Add("ÅMÅL", 29);
            atgArenaId.Add("ÅRJÄNG", 31);
            atgArenaId.Add("ÖREBRO", 32);
            atgArenaId.Add("ÖSTERSUND", 33);
            atgArenaId.Add("ARVIKA", 36);
            atgArenaId.Add("HOTING", 37);
            atgArenaId.Add("LYCKSELE", 38);
            atgArenaId.Add("OVIKEN", 39);
            atgArenaId.Add("STRÖMSHOLM", 41);
            atgArenaId.Add("VAGGERYD", 43);
            atgArenaId.Add("KARLSHAMN", 44);
            atgArenaId.Add("TINGSRYD", 46);
            atgArenaId.Add("GÖTEBORG", 49);
            atgArenaId.Add("TÄBY", 35);
        }


        public static void FixRaceIds()
        {
            using (var context = new TravsportContext())
            {
                var dic = context.Arenas.ToDictionary(a => a.Id);
                Console.WriteLine(DateTime.Now + ": Loading invalid races");
                var invalidRaces = context.Races.Where(r => r.RaceId == null).ToList();
                Console.WriteLine(DateTime.Now + ": Loaded " + invalidRaces.Count);
                int count = 0;
                var raceDayIds = invalidRaces.Select(r => r.RaceDayId).Distinct();
                var jsons = WebParser.WebParser.LoadResultsFromDiskReallyImported(raceDayIds);
                foreach (var race in invalidRaces)
                {
                    var arena = dic[race.ArenaId.Value];
                    var j = jsons.First(rd => rd.RaceDayId == race.RaceDayId);
                    var date = ParseDateFromHeading(j.Heading);

                    if (atgArenaId.TryGetValue(arena.Name, out var aid))
                    {
                        race.RaceId = date.Value.ToString("yyyy-MM-dd") + "_" + aid + "_" + race.RaceOrder;

                    }

                    race.StartTime = date.Value;
                    race.ScheduledStartTime = date.Value;

                }
                Console.WriteLine("Upating " + invalidRaces.Count + " races with count " + count);
                context.BulkUpdate(invalidRaces);
            }
        }
        public static void FixDates()
        {
            using (var context = new TravsportContext())
            {
                var maxDate = new DateTime(1990, 1, 1);
                var maxMonte = new DateTime(2004, 1, 1);
                Console.WriteLine(DateTime.Now + ": Loading invalid races");
                var invalidRaces = context.Races.Where(r => r.StartTime < maxDate).ToList();
                Console.WriteLine(DateTime.Now + ": Loaded " + invalidRaces.Count);
                int count = 0;
                foreach(var race in invalidRaces)
                {
                    if (race.RaceId != null)
                    {
                        var split = race.RaceId.Split('_');
                        var newDate = DateTime.Parse(split[0]);
                        race.StartTime = newDate;
                        race.ScheduledStartTime = newDate;
                        if (newDate < maxMonte && race.Sport == "unknown")
                        {
                            race.Sport = "trot";
                        }
                        count++;
                    }
                    
                }
                Console.WriteLine("Upating " + invalidRaces.Count + " races with count "+count);
                context.BulkUpdate(invalidRaces);
            }
        }
        public static List<RaceDayResultJson> ImportRaceDays(IEnumerable<RaceDayJson> raceDays)
        {
            var dic = raceDays.ToDictionary(rd => rd.RaceDayId, rd => DateTime.Parse(rd.RaceDayDate));

            var ids = raceDays.Select(rd => rd.RaceDayId).ToList();
            var results = WebParser.WebParser.LoadResultsFromDisk(ids);
            Console.WriteLine("Loaded " + results.Count + " results from " + raceDays.Count() + " jsons, importing meta-data");
            ImportMetaData(results);
            Console.WriteLine(DateTime.Now + ": Importing races..");
            var raceResults = ImportRacesOnly(results, dic);
            Console.WriteLine(DateTime.Now+": Imported races, found " + raceResults.Count + " results");
            using (var context = new TravsportContext())
            {
                context.BulkInsert(raceResults);
            }
            return results;
        }
        
        static RaceResult ParseScratched(WithdrawnHors row)
        {
            RaceResult rr = new RaceResult();
            rr.HorseId = row.Id;

            rr.Scratched = true;
            return rr;
        }
        static RaceResult ParseRaceResult(RaceResultRow row, int raceDist)
        {
            RaceResult rr = new RaceResult();
            if (row.ShoeInfo != null)
            {
                rr.BackShoes = row.ShoeInfo.Back;
                rr.FrontShoes = row.ShoeInfo.Front;
            }
            if (row.EquipmentSelection != null)
            {
                if (row.EquipmentSelection.SulkyOption != null)
                {
                    rr.Sulky = row.EquipmentSelection.SulkyOption.Code;
                }
            }
            rr.Distance = row.DistanceParsed();
            rr.DistanceHandicap = rr.Distance - raceDist;
            rr.DQ = row.PlacementDisplay == "d";
            if (row.Time != null)
                rr.Galopp = row.Time.Contains("g");
            rr.HorseId = row.Horse.Id;
            rr.KmTimeMilliSeconds = row.KmTimeMilliseconds;
            rr.FinishTimeMilliseconds = (long)row.FinishTime;
            rr.FinishPosition = row.PlacementNumber;
            rr.PositionForDistance = row.PositionParsed();
            rr.StartNumber = row.ParsedProgramNumber();
            if (row.Trainer != null)
                rr.TrainerId = row.Trainer.Id;

            rr.PlatsOdds = row.DoublePlatsOdds();

            if (row.Driver != null)
                rr.DriverId = row.Driver.Id;
            rr.WinOdds = row.DoubleOdds();
           
            rr.WonPrizeMoney = row.PrizeMoney;

            return rr;
        }

        public static List<RaceResult> ImportRacesOnly(IEnumerable<RaceDayResultJson> raceDays, Dictionary<long, DateTime> racedayDates)
        {
            List<RaceResult> rrs = new List<RaceResult>();
            // Assumes horses/etc exists
            using (var context = new TravsportContext())
            {
                Console.WriteLine("Importing " + raceDays.Count() + " racedays");
                Dictionary<string, Arena> existingArenas = context.Arenas.ToDictionary(a => a.Name);
                HashSet<long> existingRaces = new HashSet<long>(context.Races.Select(r => r.Id));
               
                List<Race> newRaces = new List<Race>();
                foreach (var rd in raceDays)
                {
                    var raceDayDate = racedayDates[rd.RaceDayId];
                    var headSplit = rd.Heading.Split(' ');
                    var trackName = headSplit[0];
                    string atgRaceId = null;
                    if (atgArenaId.TryGetValue(trackName, out var atgId))
                    {
                        atgRaceId = raceDayDate.ToString("yyyy-MM-dd") + "_" + atgId + "_";
                    }
                    long arenaId = existingArenas[trackName].Id;
                    var qualifyingId = rd.RacesWithBasicInfoAndResultStatus.Where(row => row.NumberDisplay == "k" || row.NumberDisplay == "p").Select(row => row.Id).ToList();
                    Dictionary<long, int> raceNumberDic = rd.RacesWithBasicInfoAndResultStatus.ToDictionary(row => row.Id, row => row.Number);
                    int raceCounter = 1;
                    foreach (var raceJson in rd.RacesWithReadyResult)
                    {
                        if (raceJson.RaceResultRows == null || qualifyingId.Contains(raceJson.RaceId) || existingRaces.Contains(raceJson.RaceId))
                            continue;
                        int raceNumber = raceCounter;

                        List<RaceResult> raceResults = new List<RaceResult>();
                        if (!raceNumberDic.ContainsKey(raceJson.RaceId))
                        {
                            Console.WriteLine("Cant find raceId " + raceJson.RaceId + " in basic-rows");

                        }
                        else
                        {
                            raceNumber = raceNumberDic[raceJson.RaceId];
                        }

                        var dbRace = ParseRace(raceJson, null, raceNumber, raceJson.GeneralInfo.TrackConditions);
                        
                        if (atgRaceId != null)
                        {
                            dbRace.RaceId = atgRaceId + raceNumber;
                        }
                        dbRace.RaceDayId = rd.RaceDayId;
                        dbRace.ArenaId = arenaId;
                        if (existingRaces.Contains(dbRace.Id))
                            continue;
                        newRaces.Add(dbRace);
                        foreach (var result in raceJson.RaceResultRows)
                        {
                            var rr = ParseRaceResult(result, dbRace.Distance);
                            rr.RaceFKId = dbRace.Id;
                            raceResults.Add(rr);
                            rrs.Add(rr);
                        }
                        if (raceJson.WithdrawnHorses != null)
                        {
                            foreach (var result in raceJson.WithdrawnHorses)
                            {
                                var rr = ParseScratched(result);
                                rr.RaceFKId = dbRace.Id;
                                raceResults.Add(rr);
                                rrs.Add(rr);
                            }
                        }
                        if (raceResults.Any(rr => rr.WinOdds != 0))
                        {
                            double overround = 0;
                            foreach (var raceRes in raceResults.Where(rr => rr.WinOdds != 0))
                            {
                                overround += (1.0 / raceRes.WinOdds);
                            }

                            foreach (var raceRes in raceResults.Where(rr => rr.WinOdds != 0))
                            {
                                raceRes.ImpliedWinProb = (1.0 / (raceRes.WinOdds * overround));
                            }

                        }

                        var validResults = raceResults.Where(rr => !rr.DQ && !rr.Scratched && rr.KmTimeMilliSeconds > 0);
                        if (validResults.Count() > 1)
                        {
                            var bestTime = validResults.Min(rr => rr.FinishTimeMilliseconds);
                            var worstPlaceTime = validResults.Where(rr => rr.FinishPosition > 0).Max(rr => rr.FinishTimeMilliseconds);
                            var worstTime = validResults.Max(rr => rr.FinishTimeMilliseconds);
                            dbRace.LastFinishTime = worstTime;
                            dbRace.LastPlaceFinishTime = worstPlaceTime;
                            dbRace.WinnerFinishTime = bestTime;
                            var winner = validResults.FirstOrDefault(rr => rr.FinishPosition == 1);

                            if (winner != null)
                            {
                                if (winner.KmTimeMilliSeconds == 0)
                                {
                                    Console.WriteLine("RaceId " + dbRace.Id + " has winner without kmtime");
                                }
                                else
                                {
                                    dbRace.WinnerKmTimeMilliseconds = winner.KmTimeMilliSeconds;
                                    dbRace.WinnerDriverId = winner.DriverId;
                                    dbRace.WinnerHorseId = winner.HorseId;
                                    dbRace.WinnerTrainerId = winner.TrainerId;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Race {dbRace.Id} has no winner amongst {validResults.Count()} valid results and {raceResults.Count} total results");
                            }
                            var totDiff = worstTime - bestTime;
                            var placeDiff = worstPlaceTime - bestTime;
                            if (totDiff == 0)
                            {
                                Console.WriteLine("RaceId " + dbRace.Id + " has no diff between " + worstTime + " and " + bestTime);
                            }
                            if (placeDiff == 0)
                            {
                                //Console.WriteLine("RaceId " + r.Id + " has no diff between " + worstPlaceTime + " and " + bestTime);
                            }
                            foreach (var rr in validResults)
                            {
                                rr.FinishTimeAfterWinner = Math.Round(rr.FinishTimeMilliseconds - bestTime);
                                if (totDiff > 0)
                                    rr.NormalizedFinishTime = rr.FinishTimeMilliseconds.Normalize(bestTime, totDiff);

                                if (placeDiff > 0)
                                    rr.NormalizedFinishTimesPlaced = rr.FinishTimeMilliseconds.Normalize(bestTime, placeDiff);

                            }
                            if (dbRace.WinnerKmTimeMilliseconds > 0)
                            {
                                if (dbRace.First1000KmTime.HasValue && dbRace.First1000KmTime.Value > 0)
                                {
                                    dbRace.First1000SpeedRatio = dbRace.WinnerKmTimeMilliseconds / dbRace.First1000KmTime.Value;
                                }
                                if (dbRace.First500KmTime.HasValue && dbRace.First500KmTime.Value > 0)
                                {
                                    dbRace.First500SpeedRatio = dbRace.WinnerKmTimeMilliseconds / dbRace.First500KmTime.Value;
                                }
                                if (dbRace.Last500KmTime.HasValue && dbRace.Last500KmTime.Value > 0)
                                {
                                    dbRace.Last500SpeedRatio = dbRace.WinnerKmTimeMilliseconds / dbRace.Last500KmTime.Value;
                                }

                                if (dbRace.First1000KmTime.HasValue && dbRace.Last500KmTime.HasValue && dbRace.First1000KmTime.Value > 0 && dbRace.Last500KmTime.Value > 0)
                                {
                                    dbRace.StartSpeedFigure = ((double)dbRace.Last500KmTime.Value) / ((double)dbRace.First1000KmTime.Value);
                                }
                                if (dbRace.Leader500HorseId.HasValue)
                                {
                                    var rrLeader = raceResults.SingleOrDefault(rrLeader => rrLeader.HorseId == dbRace.Leader500HorseId.Value);
                                    if (rrLeader == null)
                                    {
                                        Console.WriteLine($"RaceId {dbRace.Id} has leaderHorse {dbRace.Leader500HorseId.Value}, not a racer in race?");
                                    }
                                    else
                                    {
                                        dbRace.First500Position = rrLeader.PositionForDistance;
                                        dbRace.First500Handicap = rrLeader.DistanceHandicap;
                                    }
                                }


                                if (dbRace.Leader1000HorseId.HasValue)
                                {
                                    var rrLeader = raceResults.SingleOrDefault(rrLeader => rrLeader.HorseId == dbRace.Leader1000HorseId.Value);
                                    if (rrLeader == null)
                                    {
                                        Console.WriteLine($"RaceId {dbRace.Id} has leaderHorse {dbRace.Leader1000HorseId.Value}, not a racer in race?");
                                    }
                                    else
                                    {
                                        dbRace.First1000Position = rrLeader.PositionForDistance;
                                        dbRace.First1000Handicap = rrLeader.DistanceHandicap;
                                    }
                                }


                            }
                        }
                    }
                }
                BulkConfig cfg = new BulkConfig();
                cfg.SqlBulkCopyOptions = Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepNulls | Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepIdentity;
                cfg.TrackingEntities = false;
                Console.WriteLine(DateTime.Now + ": BulkInserting " + newRaces.Count + " new races");
                context.BulkInsert<Race>(newRaces, cfg);
                Console.WriteLine(DateTime.Now + ": Finished bulkinsert");
            }
            return rrs;
        }
        static TrainerDriver ParseDriver(DriverJson trainerJson)
        {
            TrainerDriver td = new TrainerDriver();
            td.ShortName = trainerJson.Name;
            td.Id = trainerJson.Id;
            td.Name = trainerJson.Name;
            td.Linkable = trainerJson.Linkable;
            return td;
        }
        static void ImportMetaData(IEnumerable<RaceDayResultJson> raceDays)
        {
            using (var context = new TravsportContext())
            {
                Dictionary<long, Horse> horses = context.Horses.ToDictionary(h => h.Id);
                Dictionary<long, TrainerDriver> drivers = context.TrainerDrivers.ToDictionary(d => d.Id);
                Dictionary<string, Arena> existingArenas = context.Arenas.ToDictionary(a => a.Name);

                List<Arena> newArenas = new List<Arena>();
                List<Horse> newHorses = new List<Horse>();
                List<TrainerDriver> newTrainers = new List<TrainerDriver>();
                foreach (var rd in raceDays)
                {
                    var headSplit = rd.Heading.Split(' ');
                    var trackName = headSplit[0];
                    if (!existingArenas.TryGetValue(trackName, out var arena))
                    {
                        arena = new Arena();
                        arena.Name = trackName;
                        newArenas.Add(arena);
                        existingArenas.Add(trackName, arena);
                    }

                    foreach(var raceJson in rd.RacesWithReadyResult)
                    {
                        if (raceJson.RaceResultRows == null)
                            continue;
                        foreach(var result in raceJson.RaceResultRows)
                        {
                            if (!horses.ContainsKey(result.Horse.Id))
                            {
                                var dbHorse = ParseHorse(result.Horse);
                                context.Horses.Add(dbHorse);
                                horses.Add(result.Horse.Id, dbHorse);
                                newHorses.Add(dbHorse);
                            }
                            if (!drivers.ContainsKey(result.Driver.Id))
                            {
                                var dbDriver = ParseDriver(result.Driver);
                                context.TrainerDrivers.Add(dbDriver);
                                drivers.Add(result.Driver.Id, dbDriver);
                                newTrainers.Add(dbDriver);
                            }
                            if (result.Trainer != null && !drivers.ContainsKey(result.Trainer.Id))
                            {
                                var dbDriver = ParseDriver(result.Trainer);
                                context.TrainerDrivers.Add(dbDriver);
                                drivers.Add(result.Trainer.Id, dbDriver);
                                newTrainers.Add(dbDriver);
                            }
                        }
                        if (raceJson.WithdrawnHorses != null)
                        {
                            foreach (var result in raceJson.WithdrawnHorses)
                            {
                                if (!horses.ContainsKey(result.Id))
                                {
                                    var dbHorse = new DB.Entities.Horse();
                                    dbHorse.Id = result.Id;
                                    dbHorse.Name = result.Name;
                                    context.Horses.Add(dbHorse);
                                    horses.Add(dbHorse.Id, dbHorse);
                                    newHorses.Add(dbHorse);
                                }
                            }
                        }
                    }
                }
                if (newHorses.Count > 0)
                {
                    Console.WriteLine("Inserting " + newHorses + " new horses");
                    context.BulkInsert(newHorses);
                }
                if (newArenas.Count > 0)
                {
                    Console.WriteLine(DateTime.Now + " Inserting " + newArenas.Count + " new arenas");
                    context.BulkInsert(newArenas);
                }
                if (newTrainers.Count > 0)
                {
                    Console.WriteLine(DateTime.Now + " inserting " + newTrainers.Count + " new trainers");
                    context.BulkInsert(newTrainers);
                }
            }
        }
        public static void BulkImport<T>(IEnumerable<T> races, SqlBulkCopyOptions options)
            where T : class 
        {
            var cfg = new BulkConfig();
            cfg.SqlBulkCopyOptions = options;
            using (var context = new TravsportContext())
            {
                context.BulkInsert<T>(races.ToList(), cfg);
            }
        }
        static TrainerDriver ParseDriver(TrainerJson trainerJson)
        {
            TrainerDriver td = new TrainerDriver();
            td.ShortName = trainerJson.Name;
            td.Id = trainerJson.Id;
            td.Name = trainerJson.Name;
            td.Linkable = trainerJson.Linkable;
            return td;
        }
       
        static DB.Entities.Horse ParseHorse(WebParser.Json.HorseJson json)
        {
            DB.Entities.Horse h = new DB.Entities.Horse();
            h.Id = json.Id;
            h.Name = json.Name;
            return h;
        }
   
        static long? GetHorseId(RacesWithReadyResult raceResult, string horseString)
        {
            var parts = horseString.Split(' ');
            if (int.TryParse(parts[0], out var startNumber))
            {
                var starter = raceResult.RaceResultRows.SingleOrDefault(rrr => rrr.ParsedProgramNumber() == startNumber);
                if (starter == null)
                {
                    var withdrawn = raceResult.WithdrawnHorses.SingleOrDefault(rrr => rrr.ParsedProgramNumber() == startNumber);
                    if (withdrawn == null)
                    {
                        Console.WriteLine("Couldnt parse horseString " + horseString + " into valid horse amongst starters");
                        return null;
                    }
                    return withdrawn.Id;
                }
                return starter.Horse.Id;
            }
            return null;
        }
        static long? GetKmTime(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return null;
            var parts = timeString.Split('.');
            if (parts.Length == 1)
                return null;
            if (parts.Length == 2)
            {

                int s = int.Parse(parts[0]);
                int t = int.Parse(parts[1]);
                int m = 1;
                var tot = m * 60 + s + t / 10.0;
                return (long)(tot * 1000);
            }
            int mins = int.Parse(parts[0]);
            int sec = int.Parse(parts[1]);
            int tenth = int.Parse(parts[2]);
            var totSeconds = mins * 60 + sec + tenth / 10.0;
            return (long)(totSeconds * 1000);
        }
        //BERGSÅKER TORSDAG 4 JANUARI 1996
        static DateTime? ParseDateFromHeading(string heading)
        {
            var split = heading.Split(' ');

            int yearIndex = split.Length - 1;
            int monthIndex = yearIndex - 1;
            int dayIndex = monthIndex - 1;
            if (!int.TryParse(split[yearIndex], out var year))
            {
                return null;
            }
            if (!int.TryParse(split[dayIndex], out var day))
            {
                return null;
            }
            if (!monthNameToNumber.TryGetValue(split[monthIndex], out var month))
            {
                return null;
            }
            return new DateTime(year, month, day);
        }
        static Race ParseRace(RacesWithReadyResult raceResult, string atgRaceId, int raceNumber, string trackCondition)
        {
            if (raceResult.RaceResultRows == null || raceResult.RaceResultRows.Count == 0)
                return null;
            var validDistances = raceResult.RaceResultRows.Select(rrr => rrr.DistanceParsed()).Where(d => d > 0);
            int dist = 0;
            if (validDistances.Any())
            {
                dist = validDistances.Min();
            }
            var tempoMatch = tempoRegex.Match(raceResult.GeneralInfo.TempoText);

            Race r = new Race();
            r.Id = raceResult.RaceId;
            r.Distance = dist;
            if (tempoMatch.Success)
            {
                var fiveTime = tempoMatch.Groups[1].Value;
                var fiveHorse = tempoMatch.Groups[2].Value;
                var tenTime = tempoMatch.Groups[3].Value;
                var tenHorse = tempoMatch.Groups[4].Value;
                var lastTime = tempoMatch.Groups[5].Value;

                r.First500KmTime = GetKmTime(fiveTime);
                r.First1000KmTime = GetKmTime(tenTime);
                r.Last500KmTime = GetKmTime(lastTime);
                r.Leader500HorseId = GetHorseId(raceResult, fiveHorse);
                r.Leader1000HorseId = GetHorseId(raceResult, tenHorse);
            }
            if (raceResult.PropositionDetailRows != null)
            {
                var allProps = string.Join(", ", raceResult.PropositionDetailRows.Select(r => r.Text));
                r.Name = string.Join(", ", raceResult.PropositionDetailRows.Where(r => r.Type == "U" || r.Type == "T").Select(r => r.Text));
                if (allProps.Contains("Monté") || allProps.Contains("monté"))
                    r.Sport = "monte";
                else
                    r.Sport = "trot";
            }
            else
            {
                r.Name = raceResult.GeneralInfo.Heading;
                r.Sport = "unknown";
            }
            r.RaceId = atgRaceId;
            r.RaceOrder = raceNumber;
            if (raceResult.GeneralInfo.StartTime != DateTime.MinValue)
            {
                r.StartTime = raceResult.GeneralInfo.StartTime;
                r.ScheduledStartTime = raceResult.GeneralInfo.StartTime;
            }
            else
            {
                var d = ParseDateFromHeading(raceResult.GeneralInfo.Heading);
                if (d.HasValue)
                {
                    r.StartTime = d.Value;
                }
            }
            r.StartType = raceResult.StartType();
            foreach (var row in raceResult.RaceResultRows)
            {
                if (row.PlacementDisplay.Contains("d"))
                    continue;
                var secTime = row.SecondsTime();
                if (secTime > 0)
                {
                    var kmTimeMilliseconds = (secTime + 60.0) * 1000.0;

                    row.FinishTime = Math.Round(kmTimeMilliseconds * (row.DistanceParsed() / 1000.0));
                    row.KmTimeMilliseconds = (long)kmTimeMilliseconds;
                }
                row.RaceDistance = r.Distance;
            }
            var validTimes = raceResult.RaceResultRows.Where(rr => rr.FinishTime > 0);
            if (validTimes.Any())
            {
                r.WinnerFinishTime = Math.Round(validTimes.Min(rr => rr.FinishTime));
            }
            r.TrackCondition = trackCondition;
            return r;
        }
    }
}
