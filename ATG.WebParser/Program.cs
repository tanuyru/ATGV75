using ATG.DB;
using ATG.DB.Entities;
using ATG.Shared.Enums;
using ATG.WebParser.Json;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace ATG.WebParser
{
    class Program
    {
        public static int NumHistoryGames = 0;
        public static bool ForceReloadHistory = false;
        static void Main(string[] args)
        {
            LoadGame(GameTypeEnum.V75, DateTime.Now.AddDays(0));
            return;
            LoadByGameDays();
            Console.Write("FINISHE");
            return;
        }
        static int raceCounter = 0;

        static void LoadKnown()
        {
            using (var context = new AtgContext())
            {
                var horses = context.Horses.Select(h => h.Id);
                var trainers = context.Trainers.Select(t => t.Id);
                var arenas = context.Arenas.Select(a => a.Id);
                var drivers = context.Drivers.Select(d => d.Id);
                var breeders = context.Breeders.Select(b => b.Id);
                var owners = context.Owners.Select(o => o.Id);
                var starts = context.RecentHorseStarts.Select(rs => rs.RaceId + "_" + rs.HorseId);
                var games = context.AvailableGames.ToList();

                knownAvailableGames = new HashSet<string>(games.Select(g => g.GameId));
                knownArenas = new HashSet<long>(arenas);
                knownDrivers = new HashSet<long>(drivers);
                knownTrainers = new HashSet<long>(trainers);
                knownHorses = new HashSet<long>(horses);
                knownOwners = new HashSet<long>(owners);
                knownBreeders = new HashSet<long>(breeders);
                knownHorseStarts = new HashSet<string>(starts);
            }
        }
        static HashSet<long> knownHorses = new HashSet<long>();
        static HashSet<long> knownTrainers = new HashSet<long>();
        static HashSet<long> knownDrivers = new HashSet<long>();
        static HashSet<long> knownArenas = new HashSet<long>();

        static HashSet<long> knownBreeders = new HashSet<long>();
        static HashSet<long> knownOwners = new HashSet<long>();
        static HashSet<string> knownHorseStarts = new HashSet<string>();
        static HashSet<string> knownAvailableGames = new HashSet<string>();
        static Horse AssureHorse(Horse horse, AtgContext dbContext)
        {
            if (horse.Id == 0 || !knownHorses.Contains(horse.Id))
            {
                horse = dbContext.AssureHorse(horse);
                knownHorses.Add(horse.Id);

                if (horse.Mother != null)
                {
                    horse.Mother = AssureHorse(horse.Mother, dbContext);
                }
                if (horse.Father != null)
                {
                    horse.Father = AssureHorse(horse.Father, dbContext);
                }
                if (horse.GrandFather != null)
                {
                    horse.GrandFather = AssureHorse(horse.GrandFather, dbContext);
                 
                }
                if (horse.GrandMother != null)
                {
                    horse.GrandMother = AssureHorse(horse.GrandMother, dbContext);
                }
                
                if (horse.Trainer != null)
                {
                    if (knownTrainers.Add(horse.Trainer.Id) || horse.Trainer.Id == 0)
                    {
                        horse.Trainer = dbContext.AssureTrainer(horse.Trainer);
                    }
                }
                if (horse.Owner != null)
                {
                    if (knownOwners.Add(horse.Owner.Id) || horse.Owner.Id == 0)
                    {
                        horse.Owner = dbContext.AssureOwner(horse.Owner);
                    }
                }
                if (horse.Breeder != null)
                {
                    if (knownBreeders.Add(horse.Breeder.Id) || horse.Breeder.Id == 0)
                    {
                        horse.Breeder = dbContext.AssureBreeder(horse.Breeder);
                    }
                }
                
            }
            return horse;
        }

       
        static Race LoadRace(string raceId, AtgContext dbContext, bool assureResults = false, PoolDistJson vinnarJson = null)
        {
            raceCounter++;
            if (raceCounter % 100 == 0)
            {
                Console.WriteLine("Finished " + raceCounter + " races--------------");
            }
            //if (dbContext.Races.Any(dbRace => dbRace.RaceId  == raceId) || dbContext.Races.Local.Any(db => db.RaceId == raceId))
            {
                //raceCounter++;
                //Console.WriteLine("Skipping raceId " + race.id + " because already exists");
                //continue;
            }
            var extended = Parser.GetRace(raceId);
            
            if (extended == null)
            {
                Thread.Sleep(5000);
                extended = Parser.GetRace(raceId);
                if (extended == null || extended.starts == null)
                {
                    Console.WriteLine("Skipping raceId " + raceId);
                    return null;
                }
            }
            var raceEntity = Converter.ParseRace(extended);
            var trackEntity = Converter.ParseArena(extended.track);
            if (!knownArenas.Contains(trackEntity.Id))
            {
                Console.WriteLine("Adding new arena " + trackEntity.Name);

                trackEntity = dbContext.AssureArena(trackEntity);
                knownArenas.Add(trackEntity.Id);
            }
            raceEntity.ArenaId = trackEntity.Id;

            
            raceEntity = dbContext.AssureRace(raceEntity);

            dbContext.SaveChanges();

          
            if (extended.starts == null)
            {
                return raceEntity;
            }
            double winTime = 0;
            var startsWithTime = extended.starts.Where(rs => rs.result != null && rs.result.kmTime != null && rs.result.kmTime.ParsedTimeSpan() > TimeSpan.Zero);
            if (!startsWithTime.Any())
            {
                //Console.WriteLine("No starters finished? " + raceId);
                //continue;
            }
            else
            {
                winTime = startsWithTime
                    .Min(rs => rs.result.kmTime.ParsedTimeSpan().TotalMilliseconds);
            }
            Dictionary<int, NoHorseResultJson> finishTimes = null;
            NoRaceJsonResult noResult = null;
            HashSet<int> dqStarters = null;
            HashSet<int> gallopedStarters = null;
            DateTime lastNoDetailDate = new DateTime(2015, 1, 1);
            if (raceEntity.ScheduledStartTime > lastNoDetailDate && trackEntity.Country == "NO")
            {
                var noRes = Parser.GetNoRaceJson(trackEntity.Name, raceEntity.ScheduledStartTime, raceEntity.RaceOrder);
                if (noRes == null)
                {
                    Console.WriteLine("Failed at getting NO-times, sleeping and trying again...");
                    Thread.Sleep(5000);
                    noRes = Parser.GetNoRaceJson(trackEntity.Name, raceEntity.ScheduledStartTime, raceEntity.RaceOrder);
                    if (noRes == null)
                    {
                        Console.WriteLine("Failed second time, returning as is");
                        return raceEntity;
                    }
                    Console.WriteLine("Ugly-fix worked, not ddosed banned yet");
                }
                if (noRes.success && noRes.result != null)
                {
                    noResult = noRes.result;
                    finishTimes = new Dictionary<int, NoHorseResultJson>();
                    dqStarters = new HashSet<int>();
                    gallopedStarters = new HashSet<int>();
                    foreach(var r in noRes.result.results)
                    {
                        // Apparently sometimes results are duplicated?
                        if (finishTimes.ContainsKey(r.startNumber))
                        {
                            Console.WriteLine("Found 2 finishing times for startNumber " + r.startNumber + " in race " + raceEntity.RaceId);
                            continue;
                        }
                        finishTimes.Add(r.startNumber, r);
                        var time = r.ParseKmTime(out var g, out var dq);
                        if (dq)
                        {
                            dqStarters.Add(r.startNumber);
                        }
                        if (g)
                        {
                            gallopedStarters.Add(r.startNumber);
                        }
                    }
                    var times = finishTimes.Select(no => no.Value.ParseKmTime(out var tmp, out var tmp2)).Where(t => t > 0).ToList();
                    if (winTime == 0 && times.Any())
                    {
                        winTime = times.Min();
                    }
                }
            }
            if (noResult != null)
            {
                raceEntity.First500KmTime = noResult.GetFirst500();
                raceEntity.First1000KmTime = noResult.GetFirst1900();
                raceEntity.Last500KmTime = noResult.Last500();
                if (noResult.first500MetersHorseName != null)
                    raceEntity.Leader500StartNumber = noResult.GetFirst500Number();
                if (noResult.first1000MetersHorseName != null)
                    raceEntity.Leader1000StartNumber = noResult.GetFirst1000Number();
            }

            foreach (var starter in extended.starts)
            {
                if (starter.horse == null)
                    continue;
                TimeSpan? noKmTime = null;
                bool noGalloped = false;
                bool noDq = false;
                if (noResult != null && noResult.results != null)
                {
                    var noStart = noResult.results.FirstOrDefault(r => r.startNumber == starter.number);
                    if (noStart != null)
                    {
                        starter.result.finishOrder = noStart.order;
                        if (noStart.kmTime != null)
                        {
                            var time = noStart.ParseKmTime(out noGalloped, out noDq);
                            noKmTime = TimeSpan.FromMilliseconds(time);
                            starter.disqualified = noDq;
                            starter.galloped = noGalloped;
                        }
                    }
                }
                var startEntity = Converter.ParseRaceResult(starter, raceEntity.Distance, vinnarJson);
                if (!startEntity.Scratched && starter?.result?.kmTime == null)
                {
                    // Might be missing these values in some races.
                    if (noResult == null || noResult.results == null || noResult.results.Count == 0 || finishTimes.Count(ft => ft.Value.kmTime != null) == 0)
                    {
                        //Console.WriteLine("NO NORWAY FINISH TIMES AND NO TIME FROM ATG SKIPPING STARTER " + starter.horse.name);
                        //continue;
                    }
                    else

                    {
                        var raceInfo = finishTimes.Values.SingleOrDefault(info => info.startNumber == starter.number);
                        if (noKmTime.HasValue)
                        {
                            startEntity.KmTime = noKmTime.Value;
                            startEntity.DQ = noDq;
                            startEntity.Galopp = noGalloped;
                            startEntity.KmTimeMilliSeconds = (long)startEntity.KmTime.TotalMilliseconds;
                        }
                        else
                        {
                            startEntity.KmTimeMilliSeconds = raceInfo.ParseKmTime(out var g, out var dq);
                            startEntity.DQ = dq;
                            startEntity.Galopp = g;
                            startEntity.KmTime = TimeSpan.FromMilliseconds(startEntity.KmTimeMilliSeconds);
                        }

                    }
                }
                if (startEntity.KmTimeMilliSeconds > 0)
                {
                    startEntity.TimeBehindWinner = (long)(startEntity.KmTimeMilliSeconds - winTime);
                    startEntity.FinishTimeMilliseconds = startEntity.KmTimeMilliSeconds * (startEntity.Distance / 1000.0);
                }
                var horse = Converter.ParseHorse(starter.horse);

                horse = AssureHorse(horse, dbContext);
                var driver = Converter.ParseDriver(starter.driver);
                /*
                if (starter.driver.homeTrack != null)
                {
                    var parsedArena = Converter.ParseArena(starter.driver.homeTrack);
                    if (!string.IsNullOrEmpty(parsedArena.Country))
                    {
                        if (!knownArenas.Contains(parsedArena.Id))
                        {
                            parsedArena = dbContext.AssureArena(parsedArena);
                            knownArenas.Add(parsedArena.Id);
                        }
                        driver.HomeArenaId = parsedArena.Id;
                    }
                }
                */
                if (!knownDrivers.Contains(driver.Id))
                {
                    driver = dbContext.AssureDriver(driver);
                    knownDrivers.Add(driver.Id);
                    //Console.WriteLine("Adding driver " + driver.ShortName + " with id " + driver.Id);
                }
                
                startEntity.DriverId = driver.Id;
                startEntity.HorseId = horse.Id;
                startEntity.RaceFKId = raceEntity.Id;

                int currYear = raceEntity.ScheduledStartTime.Year;
                int pastYear = currYear - 1;
                if (starter.horse.statistics != null)
                {
                    if (starter.horse.statistics.years != null)
                    {
                        if (starter.horse.statistics.years.TryGetValue(currYear, out var ysCurr))
                        {
                            startEntity.HorseMoneyThisYear = ysCurr.earnings;
                            if (ysCurr.placement.Count > 0)
                            {
                                startEntity.Wins = ysCurr.placement[1];
                                startEntity.Seconds = ysCurr.placement[2];
                                startEntity.Thirds = ysCurr.placement[3];
                            }
                            startEntity.Starts = ysCurr.starts;
                            startEntity.HorseWinPercent = ysCurr.winPercentage;
                            if (ysCurr.earningsPerStart.HasValue)
                            {
                                startEntity.MoneyPerStart = ysCurr.earningsPerStart.Value;
                            }
                        }

                        if (starter.horse.statistics.years.TryGetValue(pastYear, out var ysPast))
                        {
                            startEntity.HorseMoneyLastYear = ysPast.earnings;
                            if (ysPast.placement.Count > 0)
                            {
                                startEntity.LastYearWins = ysPast.placement[1];
                                startEntity.LastYearSeconds = ysPast.placement[2];
                                startEntity.LastYearThirds = ysPast.placement[3];
                            }
                            startEntity.LastYearStarts = ysPast.starts;
                            startEntity.LastYearHorseWinPercent = ysPast.winPercentage;
                            if (ysPast.earningsPerStart.HasValue)
                            {
                                startEntity.LastYearMoneyPerStart = ysCurr.earningsPerStart.Value;
                            }
                        }

                        if (starter.horse.statistics.life != null)
                        {
                            startEntity.HorseTotalMoney = starter.horse.statistics.life.earnings;
                            if (starter.horse.statistics.life.earningsPerStart.HasValue)
                            {
                                startEntity.MoneyPerStartTotal = starter.horse.statistics.life.earningsPerStart.Value;
                            }
                            if (starter.horse.statistics.life.placePercentage.HasValue)
                            {
                                startEntity.HorseTotalPlacepercent = starter.horse.statistics.life.placePercentage.Value;
                            }
                            startEntity.HorseTotalWinPercent = starter.horse.statistics.life.winPercentage;
                        }
                    }
                }
                if (starter.horse.trainer != null && starter.horse.trainer.statistics != null)
                {
                    if (starter.horse.trainer.statistics.years.TryGetValue(currYear, out var ysCurr))
                    {
                        startEntity.TrainerMoneyThisYear = ysCurr.earnings;
                        if (ysCurr.placement.Count > 0)
                        {
                            startEntity.TrainerWins = ysCurr.placement[1];
                            startEntity.TrainerSeconds = ysCurr.placement[2];
                            startEntity.TrainerThirds = ysCurr.placement[3];
                        }
                        startEntity.TrainerStarts = ysCurr.starts;
                        startEntity.TrainerWinPercent = ysCurr.winPercentage;
                    }

                    if (starter.horse.trainer.statistics.years.TryGetValue(pastYear, out var ysPast))
                    {
                        startEntity.TrainerMoneyLastYear = ysPast.earnings;
                        if (ysPast.placement.Count > 0)
                        {
                            startEntity.LastYearTrainerWins = ysPast.placement[1];
                            startEntity.LastYearTrainerSeconds = ysPast.placement[2];
                            startEntity.LastYearTrainerThirds = ysPast.placement[3];
                        }
                        startEntity.LastYearTrainerStarts = ysPast.starts;
                        startEntity.LastYearTrainerWinPercent = ysPast.winPercentage;
                    }
                }

                if (starter.driver != null && starter.driver.statistics != null)
                {
                    if (starter.driver.statistics.years.TryGetValue(currYear, out var ysCurr))
                    {
                        startEntity.DriverMoney = ysCurr.earnings;
                        if (ysCurr.placement.Count > 0)
                        {
                            startEntity.DriverWins = ysCurr.placement[1];
                            startEntity.DriverSeconds = ysCurr.placement[2];
                            startEntity.DriverThirds = ysCurr.placement[3];
                        }
                        startEntity.DriverStarts = ysCurr.starts;
                        startEntity.DriverWinPercent = ysCurr.winPercentage;
                    }

                    if (starter.driver.statistics.years.TryGetValue(pastYear, out var ysPast))
                    {
                        startEntity.DriverMoneyLastYear = ysPast.earnings;
                        if (ysPast.placement.Count > 0)
                        {
                            startEntity.LastYearDriverWins = ysPast.placement[1];
                            startEntity.LastYearDriverSeconds = ysPast.placement[2];
                            startEntity.LastYearDriverThirds = ysPast.placement[3];
                        }
                        startEntity.LastYearDriverStarts = ysPast.starts;
                        startEntity.LastYearDriverWinPercent = ysPast.winPercentage;
                    }
                }
                startEntity.TrainerId = null;
                if (starter.horse.trainer != null)
                {
                    long trainerId = starter.horse.trainer.id;
                    if (trainerId != 0)
                    {
                        if (!knownTrainers.Contains(starter.horse.trainer.id))
                        {
                            var trainer = Converter.ParseTrainer(starter.horse.trainer);
                            trainer = dbContext.AssureTrainer(trainer);
                            knownTrainers.Add(trainer.Id);


                        }
                        startEntity.TrainerId = trainerId;
                    }
                }
                if (assureResults)
                {
                    startEntity = dbContext.AssureRaceResult(startEntity);
                }
                else
                {
                    dbContext.AddRaceResult(startEntity);
                }
                dbContext.SaveChanges();
                if (starter.horse.results.records != null)
                {
                    foreach (var s in starter.horse.results.records)
                    {
                        var key = s.race.id + "_" + horse.Id;
                        if (knownHorseStarts.Contains(key))
                            continue;
                        knownHorseStarts.Add(key);
                        RecentHorseStart recentInfo = new RecentHorseStart();
                        recentInfo.Date = s.date.GetValueOrDefault();
                        recentInfo.DQ = s.disqualified.GetValueOrDefault();
                        recentInfo.Galloped = s.galloped.GetValueOrDefault();
                        recentInfo.KmTimeMilliseconds = s.kmTime != null ? (long)s.kmTime.ParsedTimeSpan().TotalMilliseconds : 0;
                        recentInfo.Sport = raceEntity.Sport;
                        recentInfo.Distance = s.start.distance;
                        recentInfo.Track = s.start.postPosition;
                        recentInfo.WinOdds = s.odds.GetValueOrDefault();
                        //recentInfo.RaceResultFKId = startEntity.Id;
                        recentInfo.RaceId = s.race.id;
                        recentInfo.HorseId = horse.Id;
                        if (s.race.startMethod == "volte")
                        {
                            recentInfo.StartMethod = StartTypeEnum.Volt;
                        }
                        else if (s.race.startMethod == "auto")
                        {
                            recentInfo.StartMethod = StartTypeEnum.Auto;
                        }
                        else
                        {
                            recentInfo.StartMethod = StartTypeEnum.Unknown;
                        }
                        recentInfo.Distance = s.start.distance;
                        dbContext.AddRecentStart(recentInfo);
                    }
                }
     
                if (NumHistoryGames > 0)
                {
                    var history = Parser.GetHorseRecords(raceEntity.RaceId, starter.number);
                    if (history.horse == null)
                    {
                        Console.WriteLine("--------------------------");
                        Console.WriteLine("Failed at getting history for horse number: " + starter.number + " in race " + raceEntity.RaceId);
                        continue;
                    }
                    foreach (var historyRace in history.horse.results.records.Take(NumHistoryGames))
                    {
                        var raceHist = Converter.ParseHistoryResult(historyRace);
                        if (raceHist == null)
                        {
                            Console.WriteLine($"Failed at parsing racehistory for " + historyRace.date);
                            continue;
                        }
                        var raceRes = Converter.ParseRaceFromHistory(historyRace);

                        if (Converter.ParseShoes(historyRace.start.horse, out var front, out var back))
                        {
                            raceHist.FrontShoes = front;
                            raceHist.BackShoes = back;
                        }

                        var driverRes = Converter.ParseDriver(historyRace.start.driver);
                        driverRes = dbContext.AssureDriver(driverRes);
                        raceRes = dbContext.AssureRace(raceRes);
                        var track = Converter.ParseArena(historyRace.track);
                        track = dbContext.AssureArena(track);
                        raceRes = dbContext.AssureRace(raceRes);
                        raceHist.Horse = horse;
                        raceHist.Race = raceRes;
                        raceHist.Driver = driverRes;
                        raceHist = dbContext.AssureRaceResult(raceHist);

                        /*
                        //var historyRaceEntity = Converter.ParseHistoryResult(historyRace);
                        var historyTrackEntity = Converter.ParseArena(historyRace.track);
                        historyTrackEntity = dbContext.AssureArena(historyTrackEntity);

                        if (ForceReloadHistory || (
                            !dbContext.Races.Any(dbArena => dbArena.RaceId == historyRace.race.id) && !dbContext.Races.Local.Any(dbRace => dbRace.RaceId == historyRace.race.id))
                            )
                        {
                            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": Downloading details for race {historyRace.race.id}");
                            var raceWinInfo = Parser.GetGame(GameTypeEnum.vinnare, historyRace.race.id);
                            if (raceWinInfo == null)
                                continue;
                            var onlyRace = raceWinInfo.races[0];
                            var historyRaceEntity = Converter.ParseRace(onlyRace);
                            historyRaceEntity = dbContext.AssureRace(historyRaceEntity);
                            foreach (var startHorse in onlyRace.starts.Where(s => s.horse != null))
                            {

                                var raceResultEntity = Converter.ParseRaceResult(startHorse);
                                var horseResultEntity = Converter.ParseHorse(startHorse.horse);
                                var driverResultEntity = dbContext.AssureDriver(Converter.ParseDriver(startHorse.driver));

                                horseResultEntity = dbContext.AssureHorse(horseResultEntity);

                                raceResultEntity.Race = historyRaceEntity;
                                raceResultEntity.Horse = horseResultEntity;
                                raceResultEntity.Driver = driverResultEntity;

                                raceResultEntity = dbContext.AssureRaceResult(raceResultEntity);
                            }
                        }
                        else
                        {
                            //Console.WriteLine("Already got details for raceId " + historyRace.race.id);
                        }
                        historyTrackEntity = dbContext.AssureArena(historyTrackEntity);
                        */
                    }
                }

            }

            var validRaceResults = raceEntity.RaceResults.FirstOrDefault(rr => rr.FinishPosition == 1);
            if (validRaceResults != null)
            {
                if (validRaceResults.FinishTimeMilliseconds == 0)
                {
                    Console.WriteLine($"RaceId {raceEntity.RaceId} has 0s a winner-time");
                }
                raceEntity.WinnerFinishTime = validRaceResults.FinishTimeMilliseconds;
            }
            return raceEntity;
        }
        static string[] InvalidNorwayArenas = new string[]
        {
            "Övrevoll"
        };
        static bool getGames = true;
        static List<string> bigGameTypes = new List<string>()
        {
            "V75",
            "V86",
            "V64",
            "V65",
            "GS75",
        };
        static void LoadByGameDays()
        {
         
                List<string> gameTypesToLoad = new List<string>();
            //gameTypesToLoad.Add("V5");
            //gameTypesToLoad.Add("V4");
            gameTypesToLoad.Add("V75");
            gameTypesToLoad.Add("V64");
            gameTypesToLoad.Add("V86");
            gameTypesToLoad.Add("V65");
            gameTypesToLoad.Add("GS75");
            List<string> countryCodes = new List<string>();
            countryCodes.Add("SE");
            //countryCodes.Add("NO");
            DateTime startDate = new DateTime(2020, 9, 20);
            int days = (int)(DateTime.Now - startDate).TotalDays;
            DateTime endDate = new DateTime(2012, 12, 24);
            int numDays = 1;// ((int)(startDate-endDate).TotalDays);
            int offsetDays = 0;

            LoadKnown();
            for (int i = offsetDays; i < offsetDays + numDays; i++)
            {
                HashSet<string> validRaceIdsToday = new HashSet<string>();
                using (var dbContext = new AtgContext())
                {
                    DateTime timestamp = startDate.AddDays(-i).Date;
                    var gameDayGames = Parser.GetGameDay(timestamp);
                    Console.WriteLine("Processing date " + timestamp);

                    if (gameDayGames == null || gameDayGames.games == null)
                    {
                        Console.WriteLine("ERROR GETTING GAME DAY " + timestamp);
                        Thread.Sleep(2000);
                        continue;
                    }
                    int raceCounter = 0;

                    Console.WriteLine($"Got {gameDayGames.games.Count} games on {gameDayGames.tracks.Count} tracks, processing...");
                    foreach (var track in gameDayGames.tracks.Where(t => countryCodes.Contains(t.countryCode) && !InvalidNorwayArenas.Contains(t.name)))
                    {
                        var td = Converter.ParseTrackDay(track, timestamp);
                        var arena = new Arena();
                        arena.Id = track.id;
                        arena.Name = track.name;
                        arena.Country = track.countryCode;
                        if (!knownArenas.Contains(track.id))
                        {
                            arena.Id = dbContext.AddArena(arena);
                            knownArenas.Add(arena.Id);
                        }
                        td.ArenaId = arena.Id;
                        td = dbContext.AssureTrackDay(td, arena.Id);
                        if (track.races != null)
                        {
                            foreach (var id in track.races)
                            {
                                validRaceIdsToday.Add(id.id);
                            }
                        }
                    }
                    if (getGames)
                    {
                        foreach (var kvp in gameDayGames.games)
                        {
                            //Console.WriteLine($"Loading {kvp.Key} for {timestamp.Date}");

                            try
                            {
                                foreach (var id in kvp.Value)
                                {
                                    if (!knownAvailableGames.Contains(id.id))
                                    {
                                        var ag = dbContext.AddAvailableGame(id.id, timestamp);
                                        knownAvailableGames.Add(id.id);
                                    }

                                    if (!gameTypesToLoad.Contains(kvp.Key))
                                        continue;
                                    var gameInfo = Parser.GetGame(id.id);
                                    if (gameInfo == null || gameInfo.pools == null)
                                    {
                                        Thread.Sleep(5000);
                                        Console.Write("NO GAMEINFO FOUND " + id.id + "-------------");
                                        continue;
                                    }

                                    var gt = OddsHelper.ParseGameType(kvp.Key);
                                    var cgEntity = Converter.ParseGame(gameInfo.pools[kvp.Key], gt);
                                    cgEntity = dbContext.AssureGame(cgEntity);
                                    long totSystemsLeft = cgEntity.Systems;
                             
                                    foreach (var race in gameInfo.races)
                                    {
                                        if (raceCounter % 100 == 0)
                                        {
                                            Console.WriteLine("Finished " + raceCounter + " races--------------");
                                        }

                                        raceCounter++;


                                        if (!validRaceIdsToday.Remove(race.id))
                                        {
                                            continue;
                                        }
                                        var raceEntity = LoadRace(race.id, dbContext);

                                        GameDistribution raceDist = null;
                                        if (race.pools.ContainsKey(kvp.Key))
                                        {
                                            raceDist = Converter.ParseGameDistribution(race.pools[kvp.Key]);
                                        }
                                        raceEntity = dbContext.AssureRace(raceEntity);
                                        cgEntity.AssureRace(raceEntity, raceCounter);
                                        if (raceDist != null && totSystemsLeft > 0 && bigGameTypes.Contains(cgEntity.GameType.ToString()))
                                        {
                                            raceEntity.SystemsLost = totSystemsLeft - raceDist.SystemsLeft;
                                            raceEntity.SystemsLostPercent = Math.Round(raceEntity.SystemsLost / (double)totSystemsLeft, 4);
                                            totSystemsLeft = raceDist.SystemsLeft;
                                        }
                                        if (raceDist == null)
                                        {
                                            Console.WriteLine("No racedist for race " + raceEntity.RaceId);
                                        }
                                        if (race.sport == "trot" )
                                        {
                                            if (raceEntity.RaceResults.Count == 0)

                                            {
                                                Console.WriteLine($"RaceId {race.id} had no parsed starters from {race.starts.Count} starts, not gettin GD");
                                            }
                                            else
                                            {
                                                foreach (var starter in race.starts)
                                                {
                                                    if (starter.horse == null)
                                                        continue;
                                                    var rrEnt = raceEntity.RaceResults.SingleOrDefault(rr => rr.Track == starter.number);

                                                  
                                                    if (NumHistoryGames > 0)
                                                    {
                                                        var horse = Converter.ParseHorse(starter.horse);
                                                        horse = AssureHorse(horse, dbContext);
                                                        var history = Parser.GetHorseRecords(raceEntity.RaceId, starter.number);
                                                        if (history.horse == null)
                                                        {
                                                            Console.WriteLine("--------------------------");
                                                            Console.WriteLine("Failed at getting history for horse number: " + starter.number + " in race " + raceEntity.RaceId);
                                                            continue;
                                                        }
                                                        foreach (var historyRace in history.horse.results.records.Take(NumHistoryGames))
                                                        {
                                                            var raceHist = Converter.ParseHistoryResult(historyRace);
                                                            if (raceHist == null)
                                                            {
                                                                Console.WriteLine($"Failed at parsing racehistory for " + historyRace.date);
                                                                continue;
                                                            }
                                                            var raceRes = Converter.ParseRaceFromHistory(historyRace);

                                                            if (Converter.ParseShoes(historyRace.start.horse, out var front, out var back))
                                                            {
                                                                raceHist.FrontShoes = front;
                                                                raceHist.BackShoes = back;
                                                            }

                                                            var driverRes = Converter.ParseDriver(historyRace.start.driver);
                                                            driverRes = dbContext.AssureDriver(driverRes);
                                                            raceRes = dbContext.AssureRace(raceRes);
                                                            var track = Converter.ParseArena(historyRace.track);
                                                            track = dbContext.AssureArena(track);
                                                            raceRes = dbContext.AssureRace(raceRes);
                                                            raceHist.HorseId = horse.Id;
                                                            raceHist.Race = raceRes;
                                                            raceHist.Driver = driverRes;
                                                            raceHist = dbContext.AssureRaceResult(raceHist);

                                                            /*
                                                            //var historyRaceEntity = Converter.ParseHistoryResult(historyRace);
                                                            var historyTrackEntity = Converter.ParseArena(historyRace.track);
                                                            historyTrackEntity = dbContext.AssureArena(historyTrackEntity);

                                                            if (ForceReloadHistory || (
                                                                !dbContext.Races.Any(dbArena => dbArena.RaceId == historyRace.race.id) && !dbContext.Races.Local.Any(dbRace => dbRace.RaceId == historyRace.race.id))
                                                                )
                                                            {
                                                                Console.WriteLine(DateTime.Now.ToShortTimeString() + $": Downloading details for race {historyRace.race.id}");
                                                                var raceWinInfo = Parser.GetGame(GameTypeEnum.vinnare, historyRace.race.id);
                                                                if (raceWinInfo == null)
                                                                    continue;
                                                                var onlyRace = raceWinInfo.races[0];
                                                                var historyRaceEntity = Converter.ParseRace(onlyRace);
                                                                historyRaceEntity = dbContext.AssureRace(historyRaceEntity);
                                                                foreach (var startHorse in onlyRace.starts.Where(s => s.horse != null))
                                                                {

                                                                    var raceResultEntity = Converter.ParseRaceResult(startHorse);
                                                                    var horseResultEntity = Converter.ParseHorse(startHorse.horse);
                                                                    var driverResultEntity = dbContext.AssureDriver(Converter.ParseDriver(startHorse.driver));

                                                                    horseResultEntity = dbContext.AssureHorse(horseResultEntity);

                                                                    raceResultEntity.Race = historyRaceEntity;
                                                                    raceResultEntity.Horse = horseResultEntity;
                                                                    raceResultEntity.Driver = driverResultEntity;

                                                                    raceResultEntity = dbContext.AssureRaceResult(raceResultEntity);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //Console.WriteLine("Already got details for raceId " + historyRace.race.id);
                                                            }
                                                            historyTrackEntity = dbContext.AssureArena(historyTrackEntity);
                                                            */
                                                        }
                                                    }

                                                    var pool = race.starts.Single(rsj => rsj.horse.name == starter.horse.name).pools[kvp.Key];
                                                    var dist = Converter.ParseGameDistribution(pool);
                                                    
                                                    if (rrEnt == null)
                                                        continue;
                                                    if (rrEnt.WinOdds == 0)
                                                    {
                                                        if (starter.pools.TryGetValue("vinnare", out var pd))
                                                        {
                                                            rrEnt.WinOdds = pd.odds;
                                                        }
                                                    }
                                                    dist.Result = rrEnt;
                                                    dist.Game = cgEntity;
                                                    if (raceDist != null)
                                                    {
                                                        dist.SystemsLeft = raceDist.SystemsLeft;
                                                    }
                                                    dist = dbContext.AssureDistribution(dist);

                                                    if (NumHistoryGames > 0)
                                                    {
                                                        // Console.WriteLine("Saving changes after parsing starter " + startEntity.Horse.Name);
                                                        // dbContext.SaveChanges();
                                                    }
                                                }
                                            }
                                        }
                                        raceCounter++;
                                    }
                                    Console.WriteLine(DateTime.Now.ToLongTimeString() + ": Saving changes for game");
                                }
                            }
                            catch (SqlException ex)
                            {
                                Console.WriteLine($"Cant do game: {kvp.Key}: {ex.ToString()}");

                            }
                        }
                    }
                    dbContext.SaveChanges();
                }
                using (var dbContext = new AtgContext())
                { 
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Finished games for day getting rest of " + validRaceIdsToday.Count + " races for today");
                    foreach(var validRace in validRaceIdsToday)
                    {
                        var race = LoadRace(validRace, dbContext);
                        if (race == null)
                        {
                            Console.WriteLine(DateTime.Now + ": Cant load race " + validRace + " skipping");
                            continue;
                        }
                    }
                    dbContext.SaveChanges();
                }
                Console.WriteLine(DateTime.Now+": Finished day");


            }
            Console.ReadKey();
        }

        static void LoadGame(GameTypeEnum gt, DateTime time)
        {
            DateTime timestamp = time.Date;
            var gameDayGames = Parser.GetGameDay(timestamp);
            var gameDataParsed = gameDayGames.games.SingleOrDefault(kvp => kvp.Key == gt.ToString()).Value.FirstOrDefault();
            if (gameDataParsed == null)
            {
                Console.WriteLine("Didnt find " + gt + " for today");
            }
            var gameId = gameDataParsed.id;
            Console.WriteLine("Found gameid " + gameId);
            var gameInfo = Parser.GetGame(gameId);
            if (gameInfo == null || gameInfo.pools == null)
            {
                Console.Write("NO GAMEINFO FOUND " + gameId + "-------------");
                return;
            }
            var key = gt.ToString();
            var cgEntity = Converter.ParseGame(gameInfo.pools[key], gt);
            using (var dbContext = new AtgContext())
            {
                cgEntity = dbContext.AssureGame(cgEntity);
                long totSystemsLeft = cgEntity.Systems;

                foreach (var race in gameInfo.races)
                {
                    if (raceCounter % 100 == 0)
                    {
                        Console.WriteLine("Finished " + raceCounter + " races--------------");
                    }

                    raceCounter++;

                    GameDistribution raceDist = null;

                    PoolDistJson vinnarJson = null;
                    if (race.pools.ContainsKey("vinnare"))
                    {
                        vinnarJson = race.pools["vinnare"];
                    }
                    if (race.pools.ContainsKey(key))
                    {
                        raceDist = Converter.ParseGameDistribution(race.pools[key]);
                    }

                    var raceEntity = LoadRace(race.id, dbContext, true, vinnarJson);

                   
                    raceEntity = dbContext.AssureRace(raceEntity);
                    cgEntity.AssureRace(raceEntity, raceCounter);
                    if (raceDist != null && totSystemsLeft > 0 && bigGameTypes.Contains(cgEntity.GameType.ToString()))
                    {
                        raceEntity.SystemsLost = totSystemsLeft - raceDist.SystemsLeft;
                        raceEntity.SystemsLostPercent = Math.Round(raceEntity.SystemsLost / (double)totSystemsLeft, 4);
                        totSystemsLeft = raceDist.SystemsLeft;
                    }
                    if (raceDist == null)
                    {
                        Console.WriteLine("No racedist for race " + raceEntity.RaceId);
                    }
                    if (race.sport == "trot")
                    {
                        if (raceEntity.RaceResults.Count == 0)

                        {
                            Console.WriteLine($"RaceId {race.id} had no parsed starters from {race.starts.Count} starts, not gettin GD");
                        }
                        else
                        {
                            foreach (var starter in race.starts)
                            {
                                if (starter.horse == null)
                                    continue;



                                var pool = race.starts.Single(rsj => rsj.horse.name == starter.horse.name).pools[key];
                                var dist = Converter.ParseGameDistribution(pool);

                                var rrEnt = raceEntity.RaceResults.SingleOrDefault(rr => rr.Track == starter.number);
                                if (rrEnt == null)
                                    continue;

                         
                                if (rrEnt.WinOdds == 0)
                                {
                                    if (starter.pools.TryGetValue("vinnare", out var pd))
                                    {
                                        rrEnt.WinOdds = pd.odds;
                                    }
                                }
                                if (rrEnt.Distribution == 0)
                                {
                                    if (pool != null)
                                    {
                                        rrEnt.Distribution = pool.betDistribution;
                                    }
                                }
                                dist.Result = rrEnt;
                                dist.Game = cgEntity;
                                if (raceDist != null)
                                {
                                    dist.SystemsLeft = raceDist.SystemsLeft;
                                }
                                dist = dbContext.AssureDistribution(dist);


                            }
                        }
                    }
                    raceCounter++;
                }
                dbContext.SaveChanges();
            }
            Console.WriteLine(DateTime.Now.ToLongTimeString() + ": Saving changes for game");
        }
  
        
    }
}
