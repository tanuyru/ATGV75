using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Travsport.DB;
using Travsport.DB.Entities;
using Travsport.WebParser;
using Travsport.WebParser.Json;
using Travsport.WebParser.Json.StartListJson;

namespace Travsport.DataCollector
{
    class Program
    {
        static bool importUnfinished = true;
        static void Main(string[] args)
        {
            HashSet<long> invalidRaceDayids = new HashSet<long>();
            invalidRaceDayids.Add(500330);
            var raceDays = WebParser.WebParser.GetRaceDays(new DateTime(2020, 10, 1), new DateTime(2021, 4, 10));
            int mod = 500;

            Console.WriteLine(DateTime.Now + ": Found " + raceDays.Count + " racedays to download, logging each "+mod);
            int counter = 0;
           
            foreach(var rd in raceDays)
            {
                if (invalidRaceDayids.Contains(rd.RaceDayId))
                    continue;
                counter++;

                if (!rd.ResultsReady)
                {
                    if (importUnfinished)
                    {
                        var startList = WebParser.WebParser.GetStartList(rd.RaceDayId);
                        if (startList == null)
                        {
                            Console.WriteLine("Skipping raceday " + rd.RaceDayId);
                            continue;
                        }
                        ImportStartList(startList, 29);
                    }
                    continue;
                }

                var details = WebParser.WebParser.GetResults(rd.RaceDayId, rd.TrackName);
                if (details == null)
                {
                    continue;
                    Console.WriteLine(DateTime.Now+": Failed att downloading and parsing raceDayId " + rd.RaceDayId + " on date " + rd.RaceDayDate);
                }
                if (importUnfinished)
                {
                    if (details.RacesWithBasicInfoAndResultStatus.Count > details.RacesWithReadyResult.Count)
                    {
                        var startList = WebParser.WebParser.GetStartList(rd.RaceDayId);
                        if (startList == null)
                        {
                            Console.WriteLine("Skipping raceday " + rd.RaceDayId);
                            continue;
                        }
                        ImportStartList(startList, 29);
                    }
                }
                if (counter % mod == 0)
                {
                    Console.WriteLine(DateTime.Now + ": Downloaded " + counter + " racedays");
                }
            }
        }

        static void ImportFromDisk(DateTime from, DateTime to)
        {
            Console.WriteLine(DateTime.Now+": Getting raceDays from " + from + " to " + to);
            var raceDays = WebParser.WebParser.GetRaceDays(from, to);
            Console.WriteLine(DateTime.Now + ": Found " + raceDays.Count + " racedays, importing");
            var files = Importer.ImportRaceDays(raceDays);
            WebParser.WebParser.MoveFiles2(files.Select(f => f.FileName));
        }
        static int ImportFromDisk(int numToDisk)
        {
            var loaded = WebParser.WebParser.LoadFromDisk(numToDisk);
            if (loaded.Count == 0)
                return 0;
            return Import(loaded);
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
        static TrainerDriver ParseDriver(DriverJson trainerJson)
        {
            TrainerDriver td = new TrainerDriver();
            td.ShortName = trainerJson.Name;
            td.Id = trainerJson.Id;
            td.Name = trainerJson.Name;
            td.Linkable = trainerJson.Linkable;
            return td;
        }
        static HashSet<long> knownHorses;
        static HashSet<long> knownDrivers;
        static HashSet<long> knownBreeders;
        static HashSet<long> knownOwners;
        static Dictionary<string, Arena> knownArenas;
        static void LoadKnown()
        {
            using (var context = new TravsportContext())
            {
                knownHorses = new HashSet<long>(context.Horses.Select(h => h.Id));
                knownDrivers = new HashSet<long>(context.TrainerDrivers.Select(d => d.Id));
                knownBreeders = new HashSet<long>(context.Breeders.Select(b => b.Id));
                knownOwners = new HashSet<long>(context.Owners.Select(o => o.Id));
                knownArenas = context.Arenas.ToDictionary(a => a.Name);
            }
        }
        static void LoadHorseInfos(int num)
        {
            if (knownHorses == null)
                LoadKnown();
            using (var context = new TravsportContext())
            {
                var horses = context.Horses.Where(h => !h.HistoryTimestamp.HasValue).Take(num).ToList();
                foreach(var horse in horses)
                {
                    var horseJson = WebParser.WebParser.GetHorseInfo(horse.Id);
                    if (horseJson == null)
                        continue;
                    AssureHorse(horse, horseJson);
                    if (horse.OwnerId.HasValue && knownOwners.Add(horse.OwnerId.Value))
                    {
                        AssureOwner(horseJson.owner, context);
                    }
                    if (horse.TrainerId.HasValue && knownDrivers.Add(horse.TrainerId.Value))
                    {
                        AssureTrainer(horseJson.trainer, context);
                    }
                    if (horse.BreederId.HasValue && knownBreeders.Add(horse.BreederId.Value))
                    {
                        AssureBreeder(horseJson.breeder, context);
                    }
                }
                Console.WriteLine(DateTime.Now + ": Saving...");
                context.SaveChanges();
                Console.WriteLine(DateTime.Now + ": Done saving");
            }
        }
        static int Import(IEnumerable<RaceDayResultJson> jsons)
        {
            if (knownHorses == null)
            {
                LoadKnown();
            }
            int count = 0;
            using (var context = new TravsportContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                foreach(var racedayResult in jsons)
                {
                    string trackName = racedayResult.CreatedTrackName;
                    if (string.IsNullOrEmpty(trackName))
                    {
                        var headSplit = racedayResult.Heading.Split(' ');
                        trackName = headSplit[0];
                    }
                    if (!knownArenas.TryGetValue(trackName, out var arena))
                    {
                        arena = new Arena();
                        arena.Name = trackName;
                        arena.Country = "SE";
                        arena.Condition = "N/A";
                        context.Arenas.Add(arena);
                        knownArenas.Add(arena.Name, arena);
                    }
                    int raceCounter = 1;
                    foreach(var race in racedayResult.RacesWithReadyResult)
                    {
                        var dbRace = ParseRace(race, null, raceCounter, race.GeneralInfo.TrackConditions);
                        if (dbRace == null)
                        {
                            continue;
                        }
                        if (arena.Id == 0)
                        {
                            dbRace.Arena = arena;
                        }
                        else
                        {
                            dbRace.ArenaId = arena.Id;
                        }
                        context.Races.Add(dbRace);
                        count++;
                        foreach(var result in race.RaceResultRows)
                        {
                            var rr = ParseRaceResult(result, dbRace.Distance, dbRace.WinnerFinishTime);
                            rr.Race = dbRace;
                            if (!knownHorses.Contains(result.Horse.Id))
                            {
                                var dbHorse = ParseHorse(result.Horse);
                                context.Horses.Add(dbHorse);
                                knownHorses.Add(result.Horse.Id);
                            }
                            if (!knownDrivers.Contains(result.Driver.Id))
                            {
                                var dbDriver = ParseDriver(result.Driver);
                                context.TrainerDrivers.Add(dbDriver);
                                knownDrivers.Add(result.Driver.Id);
                            }
                            if (result.Trainer != null && !knownDrivers.Contains(result.Trainer.Id))
                            {
                                var dbDriver = ParseDriver(result.Trainer);
                                context.TrainerDrivers.Add(dbDriver);
                                knownDrivers.Add(result.Trainer.Id);
                            }
                            context.RaceResults.Add(rr);
                        }
                        if (race.WithdrawnHorses != null)
                        {
                            foreach (var result in race.WithdrawnHorses)
                            {
                                var rr = ParseScratched(result);
                                rr.Race = dbRace;
                                if (!knownHorses.Contains(rr.HorseId))
                                {
                                    var dbHorse = new DB.Entities.Horse();
                                    dbHorse.Id = rr.HorseId;
                                    dbHorse.Name = result.Name;
                                    context.Horses.Add(dbHorse);
                                    knownHorses.Add(dbHorse.Id);
                                }
                                context.RaceResults.Add(rr);
                            }
                        }
                        raceCounter++;
                    }
                }
                Console.WriteLine(DateTime.Now + ": Saving changes");
                context.SaveChanges();
                Console.WriteLine(DateTime.Now + ": Finished, moving imported files to Imported");
                var moved = WebParser.WebParser.MoveFiles(jsons.Select(j => j.FileName));
                Console.WriteLine(DateTime.Now + ": Moved " + moved + " to imported");
            }
            return count;
        }
        static int ImportStartList(StartListRootJson json, long arenaId)
        {
            if (knownHorses == null)
            {
                LoadKnown();
            }
            int count = 0;
            using (var context = new TravsportContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                foreach (var racedayResult in json.RaceList)
                {
                    var dbRace = ParseStartList(racedayResult);
                    if (context.Races.Any(race => race.Id == dbRace.Id))
                    {
                        Console.WriteLine("Race " + dbRace.Id + " #" + racedayResult.RaceNumber+" already exists");
                        continue;
                    }
                    int raceCounter = 1;

                    dbRace.ArenaId = arenaId;
                        context.Races.Add(dbRace);
                    count++;
                        foreach (var result in racedayResult.Horses)
                        {
                        var rr = ParseRaceResult(racedayResult, result);
                            rr.Race = dbRace;
                            if (!knownHorses.Contains(rr.HorseId))
                            {
                                var dbHorse = ParseHorseFromStartList(result);
                                context.Horses.Add(dbHorse);
                                knownHorses.Add(rr.HorseId);
                            }
                            if (rr.DriverId.HasValue && !knownDrivers.Contains(rr.DriverId.Value))
                            {
                                var dbDriver = ParseDriverFromStartList(result);
                                context.TrainerDrivers.Add(dbDriver);
                                knownDrivers.Add(rr.DriverId.Value);
                            }
                            context.RaceResults.Add(rr);
                        }
                        raceCounter++;
                    }
                
                Console.WriteLine(DateTime.Now + ": Saving changes count "+count);
                var updates = context.SaveChanges();
                Console.WriteLine(DateTime.Now + ": Finished, moving imported files to Imported, updated "+updates);
          
            }
            return count;
        }
        static Regex tempoRegex = new Regex(@"första 500 (.+?) \((.+?)\), 1000 (.+?) \((.+?)\) sista 500 (.*)");

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
                r.Leader500HorseId = GetHorseId(raceResult,fiveHorse);
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
            r.RaceResults = new List<RaceResult>();
            r.StartTime = raceResult.GeneralInfo.StartTime;
            r.ScheduledStartTime = raceResult.GeneralInfo.StartTime;
           
            r.StartType = raceResult.StartType();
            foreach(var row in raceResult.RaceResultRows)
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
        static DB.Entities.Horse ParseHorse(WebParser.Json.HorseJson json)
        {
            DB.Entities.Horse h = new DB.Entities.Horse();
            h.Id = json.Id;
            h.Name = json.Name;
            return h;
        }
        static RaceResult ParseScratched(WithdrawnHors row)
        {
            RaceResult rr = new RaceResult();
            rr.HorseId = row.Id;
            
            rr.Scratched = true;
            return rr;
        }
        static RaceResult ParseRaceResult(RaceResultRow row, int raceDist, double winnerTime)
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
            if (rr.FinishTimeMilliseconds > 0)
            {
                rr.FinishTimeAfterWinner = rr.FinishTimeMilliseconds - winnerTime;
                if (rr.FinishTimeAfterWinner < 0 && rr.FinishTimeAfterWinner > -1)
                    rr.FinishTimeAfterWinner = 0;
            }
            rr.WonPrizeMoney = row.PrizeMoney;
           
            return rr;
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

        static void AssureHorse(Horse horse, HorseInfoJson info)
        {
            if (info.dateOfBirth.HasValue)
            {
                horse.BirthYear = info.dateOfBirth.Value;
            }
            horse.Gender = info.horseGender.text;
            horse.Country = info.birthCountryCode;
            horse.Color = info.color;
            if (info.breeder != null)
                horse.BreederId = info.breeder.Id;
            horse.Race = info.horseBreed.text;
            if (info.trainer != null)
                horse.TrainerId = info.trainer.Id;
            if (info.owner != null)
                horse.OwnerId = info.owner.Id;

        }
        static void AssureOwner(WebParser.Json.OwnerJson json, TravsportContext contex)
        {
            Owner o = new Owner();
            o.Name = json.Name;
            o.Id = json.Id;
            contex.Owners.Add(o);
        }
        static void AssureBreeder(WebParser.Json.BreederJson json, TravsportContext contex)
        {
            Breeder o = new Breeder();
            o.Name = json.Name;
            o.Id = json.Id;
            contex.Breeders.Add(o);
        }
        static void AssureTrainer(WebParser.Json.TrainerJson json, TravsportContext contex)
        {
            TrainerDriver o = ParseDriver(json);
          
            contex.TrainerDrivers.Add(o);
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
        static Race ParseStartList(RaceList race)
        {
            Race r = new Race();
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
    }
}
