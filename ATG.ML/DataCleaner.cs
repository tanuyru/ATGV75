using ATG.DB;
using ATG.DB.Entities;
using ATG.ML.Models;
using ATG.ML.Sorters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Travsport.DB;

namespace ATG.ML
{
    public class DataCleaner
    {
        public static int UpdateRaceTimes(int numRaces = 1000)
        {
            int counter = 0;
            int skipped = 0;
            using (var context = new TravsportContext())
            {
                Console.WriteLine("Loading races");
                var races = context.Races.Include(r => r.RaceResults).Where(r => r.InvalidReason == null && r.Sport == "trot" && r.WinnerFinishTime > 0 && r.LastPlaceFinishTime == 0).Take(numRaces).ToList();
                Console.WriteLine("Loaded " + races.Count + " races...");
                foreach(var r in races)
                {
                    if (!r.RaceResults.Any(rr => rr.FinishPosition > 0))
                    {
                        r.InvalidReason = "No firstplace result";
                        skipped++;
                        continue;
                    }    
                    var validResults = r.RaceResults.Where(rr => !rr.DQ && !rr.Scratched && rr.KmTimeMilliSeconds > 0);
                    if (validResults.Count() > 2)
                    {
                        counter++;
                        var bestTime = validResults.Min(rr => rr.FinishTimeMilliseconds);
                        var worstPlaceTime = validResults.Where(rr => rr.FinishPosition > 0).Max(rr => rr.FinishTimeMilliseconds);
                        var worstTime = validResults.Max(rr => rr.FinishTimeMilliseconds);
                        r.LastFinishTime = worstTime;
                        r.LastPlaceFinishTime = worstPlaceTime;
                        r.WinnerFinishTime = bestTime;
                        var winner = validResults.FirstOrDefault(rr => rr.FinishPosition == 1);
                        
                        if (winner != null)
                        {
                            if (winner.KmTimeMilliSeconds == 0)
                            {
                                Console.WriteLine("RaceId " + r.Id + " has winner without kmtime");
                            }
                            else
                            {
                                r.WinnerKmTimeMilliseconds = winner.KmTimeMilliSeconds;
                                r.WinnerDriverId = winner.DriverId;
                                r.WinnerHorseId = winner.HorseId;
                                r.WinnerTrainerId = winner.TrainerId;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Race {r.Id} has no winner amongst {validResults.Count()} valid results and {r.RaceResults.Count} total results");
                        }
                        var totDiff = worstTime - bestTime;
                        var placeDiff = worstPlaceTime - bestTime;
                        if (totDiff == 0)
                        {
                            Console.WriteLine("RaceId " + r.Id + " has no diff between " + worstTime + " and " + bestTime);
                        }
                        if (placeDiff == 0)
                        {
                            //Console.WriteLine("RaceId " + r.Id + " has no diff between " + worstPlaceTime + " and " + bestTime);
                        }
                        foreach(var rr in validResults)
                        {
                            rr.FinishTimeAfterWinner = Math.Round(rr.FinishTimeMilliseconds - bestTime);
                            if (totDiff > 0)
                                rr.NormalizedFinishTime = rr.FinishTimeMilliseconds.Normalize(bestTime, totDiff);
                            
                            if (placeDiff > 0)
                                rr.NormalizedFinishTimesPlaced = rr.FinishTimeMilliseconds.Normalize(bestTime, placeDiff);
                            
                        }
                        if (r.WinnerKmTimeMilliseconds > 0)
                        {
                            if (r.First1000KmTime.HasValue && r.First1000KmTime.Value > 0)
                            {
                                r.First1000SpeedRatio = r.WinnerKmTimeMilliseconds / r.First1000KmTime.Value;
                            }
                            if (r.First500KmTime.HasValue && r.First500KmTime.Value > 0)
                            {
                                r.First500SpeedRatio = r.WinnerKmTimeMilliseconds / r.First500KmTime.Value;
                            }
                            if (r.Last500KmTime.HasValue && r.Last500KmTime.Value > 0)
                            {
                                r.Last500SpeedRatio = r.WinnerKmTimeMilliseconds / r.Last500KmTime.Value;
                            }

                            if (r.First1000KmTime.HasValue && r.Last500KmTime.HasValue && r.First1000KmTime.Value > 0 && r.Last500KmTime.Value > 0)
                            {
                                r.StartSpeedFigure = ((double)r.Last500KmTime.Value) / ((double)r.First1000KmTime.Value);
                            }
                            if (r.Leader500HorseId.HasValue)
                            {
                                var rrLeader = r.RaceResults.SingleOrDefault(rrLeader => rrLeader.HorseId == r.Leader500HorseId.Value);
                                if (rrLeader == null)
                                {
                                    Console.WriteLine($"RaceId {r.Id} has leaderHorse {r.Leader500HorseId.Value}, not a racer in race?");
                                }
                                else
                                {
                                    r.First500Position = rrLeader.PositionForDistance;
                                    r.First500Handicap = rrLeader.DistanceHandicap;
                                }
                            }


                            if (r.Leader1000HorseId.HasValue)
                            {
                                var rrLeader = r.RaceResults.SingleOrDefault(rrLeader => rrLeader.HorseId == r.Leader1000HorseId.Value);
                                if (rrLeader == null)
                                {
                                    Console.WriteLine($"RaceId {r.Id} has leaderHorse {r.Leader1000HorseId.Value}, not a racer in race?");
                                }
                                else
                                {
                                    r.First1000Position = rrLeader.PositionForDistance;
                                    r.First1000Handicap = rrLeader.DistanceHandicap;
                                }
                            }


                        }
                    }
                    else
                    {
                        r.InvalidReason = "Only " + validResults.Count() + " valid results";
                        skipped++;
                    }
                }
                Console.WriteLine($"{DateTime.Now}: Counter {counter}, skipped {skipped}, saving..");
              
                Console.WriteLine(DateTime.Now + ": Finished save after " + numRaces + " was called");
            }
            return counter;
        }
        public static void SetDistributions()
        {
            using (var context = new AtgContext())
            {
                var game = context.ComboGames
                    .Include(cg => cg.Races).ThenInclude(r => r.Race.RaceResults).ThenInclude(rr => rr.Distributions).ToList();
                 
                int saveEveryXRace = 1000;
                int counter = 0;
                Console.WriteLine($"Found {game.Count} games, fixing...");
                foreach (var r in game.SelectMany(cg => cg.Races).Select(gr => gr.Race))
                {
                    if (!r.RaceResults.Any(rr => rr.FinishTimeMilliseconds > 0))
                        continue;
                    var bestTime = r.RaceResults.Where(rr => rr.FinishTimeMilliseconds > 0).Min(rr => rr.FinishTimeMilliseconds);
                    foreach(var rr in r.RaceResults.Where(rr => rr.FinishTimeMilliseconds > 0))
                    {
                        rr.FinishTimeAfterWinner = (float)(rr.FinishTimeMilliseconds - bestTime);
                        if (rr.Distributions.Any())
                        {
                            rr.Distribution = rr.Distributions.First().Distribution;
                        }
                    }
                    counter++;
                    if (counter % saveEveryXRace == 0)
                    {
                        Console.WriteLine($"{DateTime.Now}: Saving at {counter}..");
                        context.SaveChanges();
                    }
                }
                context.SaveChanges();
            }
        }
        public static void DeleteGame(string gameId)
        {
            using (var context = new AtgContext())
            {
                var game = context.ComboGames
                    .Include(cg => cg.Races).ThenInclude(r => r.Race.RaceResults)
                    .Include(cg => cg.Payouts)
                    .Include(cg => cg.Distributions)
                    .Single(cg => cg.GameId == gameId);
                foreach(var r in game.Races.Select(gr => gr.Race))
                {
                    context.RaceResults.RemoveRange(r.RaceResults);
                    context.Races.Remove(r);
                    
                        
                }
                context.GameDistributions.RemoveRange(game.Distributions);
                context.ComboGames.Remove(game);
                context.SaveChanges();
            }
        }
        public static void DeleteTravsportGame(long arenaId, DateTime startDate)
        {
            using (var context = new TravsportContext())
            {
                var date = startDate.Date;
                var races = context.Races.Where(r => r.ArenaId.HasValue && r.ArenaId.Value == arenaId && r.StartTime.Date == date).Include(r => r.RaceResults).ToList();
                var rrs = races.SelectMany(r => r.RaceResults).ToList();
                context.RaceResults.RemoveRange(rrs);
                context.Races.RemoveRange(races);

                Console.WriteLine("Removing " + races.Count + " races and "+rrs.Count+" rrs");
                Console.ReadKey();
                context.SaveChanges();
            }
        }
        public static void CleanData()
        {
            using (var context = new AtgContext())
            {
                var races = context.Races.Include(r => r.RaceResults).Where(r => r.Sport == "trot" && r.StartType == Shared.Enums.StartTypeEnum.Auto).ToList();
                Console.Write("Finished loading races "+races.Count);
                int twenty = 0;
                int forty = 0;
                int sixty = 0;
                int bad = 0;
                int guessed = 0;
                int saveEveryXRace = 500;
                int counter = 0;
                foreach (var r in races)
                {
               

                    if (!r.RaceResults.Any(rr => rr.KmTimeMilliSeconds > 0))
                        continue;
                    if (r.RaceResults.All(rr => rr.FinishPosition == 0))
                        continue;
                    Dictionary<int, int> addedDistPerTrack = new Dictionary<int, int>();

                    double prevTime = -1;
                    int firstHandicapTrack = 0;
                    int first40mHandicapTrack = 0;
                    var valids = r.RaceResults.Where(rr => rr.KmTimeMilliSeconds > 0 && rr.FinishPosition < 20);
                    /*
                    firstHandicapTrack = GetFirstHandicapHorse(valids, r.Distance);
                    if (firstHandicapTrack > 0)
                    {
                        first40mHandicapTrack = GetFirst40mHandicapHorse(valids, r.Distance, firstHandicapTrack);
                        if (first40mHandicapTrack > 0 && first40mHandicapTrack <= firstHandicapTrack)
                        {
                            var tmp = firstHandicapTrack;
                            firstHandicapTrack = first40mHandicapTrack;
                            first40mHandicapTrack = GetFirst40mHandicapHorse(valids, r.Distance, firstHandicapTrack);
                            if (first40mHandicapTrack == 0)
                            {
                                first40mHandicapTrack = tmp;
                            }
                            
                            if (first40mHandicapTrack <= firstHandicapTrack)
                            {
                                Console.WriteLine($"20m " + firstHandicapTrack + " 40m " + first40mHandicapTrack + " " + r.RaceId);
                            }

                            while (LowerHandicapTracks(valids, r.Distance, firstHandicapTrack, first40mHandicapTrack, out var t, out var f))
                            {
                                firstHandicapTrack = t;
                                first40mHandicapTrack = f;
                            }
                            
                        }
                    }
                    */
                    if (firstHandicapTrack > 0)
                    {
                        //Console.WriteLine($"20m " + firstHandicapTrack + " 40m " + first40mHandicapTrack+" " + r.RaceId);
                    }
                    foreach (var rr in valids.OrderBy(rr => rr.FinishPosition))
                    {
                        if (rr.KmTimeMilliSeconds == 0)
                        {
                            continue;
                        }
                        int dist = r.Distance;
                        if (first40mHandicapTrack > 0 && rr.Track >= first40mHandicapTrack)
                        {
                            dist += 40;
                        }
                        else if (firstHandicapTrack > 0 && rr.Track >= firstHandicapTrack)
                        {
                            dist += 20;
                        }
                        bool guess = false;
                        var finishTime = rr.KmTimeMilliSeconds * (dist / 1000.0);

                        if (prevTime > 0 && finishTime < prevTime)
                        {
                            guessed++;
                            guess = true;
                            Console.WriteLine($"Still error after using 20 " + firstHandicapTrack + " and 40 " + first40mHandicapTrack + " " + r.RaceId);
                        }
                        rr.FinishTimeMilliseconds = finishTime;
                        rr.EstimatedFinishTime = guess;
                        prevTime = finishTime;
                    }
                    var bestTime = r.RaceResults.Where(rr => rr.FinishTimeMilliseconds > 0).Min(rr => rr.FinishTimeMilliseconds);
                    foreach(var rr in valids)
                    {
                        rr.FinishTimeAfterWinner = (float)(rr.FinishTimeMilliseconds - bestTime);
                    }
                    counter++;
                    if (counter % saveEveryXRace == 0)
                    {
                        Console.WriteLine(DateTime.Now+"Saving after " + counter + " races");
                        context.SaveChanges();
                        Console.WriteLine(DateTime.Now+"Done saving");
                    }
                }

                context.SaveChanges();
                Console.WriteLine($"Found 20s: {twenty}, fortys {forty}, sixty {sixty}, bad {bad} guessed {guessed}");
            }
        }
        public static int GetFirstHandicapHorse(IEnumerable<RaceResult> rrs, int baseDist)
        {
            int lowestHandicapTrack = 0;
            double prevTime = -1;
            foreach (var rr in rrs.OrderBy(rr => rr.FinishPosition))
            {
                if (rr.KmTimeMilliSeconds == 0)
                {
                    continue;
                }
                bool guess = false;
                var finishTime = rr.KmTimeMilliSeconds * (baseDist / 1000.0);

                if (prevTime > 0 && finishTime < prevTime)
                {
                    if (lowestHandicapTrack == 0 || rr.Track < lowestHandicapTrack)
                        lowestHandicapTrack = rr.Track;
                }
                prevTime = finishTime;
            }
            return lowestHandicapTrack;
        }

        public static int GetFirst40mHandicapHorse(IEnumerable<RaceResult> rrs, int baseDist, int handicapTrack20)
        {
            int lowestHandicapTrack = 0;
            double prevTime = -1;
            int lastTrack = -1;
            foreach (var rr in rrs.OrderBy(rr => rr.FinishPosition))
            {
                if (rr.KmTimeMilliSeconds == 0)
                {
                    continue;
                }
                int dist = baseDist;
                if (rr.Track >= handicapTrack20)
                {
                    dist += 20;
                }
                var finishTime = rr.KmTimeMilliSeconds * (dist / 1000.0);

                if (prevTime > 0 && finishTime < prevTime)
                {
                    if (lowestHandicapTrack == 0 || rr.Track < lowestHandicapTrack)
                        lowestHandicapTrack = rr.Track;
                }
                prevTime = finishTime;
                lastTrack = rr.Track;

            }
            return lowestHandicapTrack;
        }
        public static bool LowerHandicapTracks(IEnumerable<RaceResult> rrs, int baseDist, int handicapTrack20, int handicapTrack40, out int twenty, out int forty)
        {
            double prevTime = -1;
            int lastTrack = -1;
            twenty = handicapTrack20;
            forty = handicapTrack40;
            int prevTrack = 0;
            foreach (var rr in rrs.OrderBy(rr => rr.FinishPosition))
            {
                if (rr.KmTimeMilliSeconds == 0)
                {
                    continue;
                }
                int dist = baseDist;
                if (rr.Track >= handicapTrack40)
                {
                    dist += 40;
                }
                else if (rr.Track >= handicapTrack20)
                {
                    dist += 20;
                }
                var finishTime = rr.KmTimeMilliSeconds * (dist / 1000.0);

                if (prevTime > 0 && finishTime < prevTime)
                {
                    if (rr.Track < twenty)
                        twenty = rr.Track;
                    else if (rr.Track > twenty && rr.Track < forty)
                        forty = rr.Track;
                    
                }
                prevTrack = rr.Track;
                prevTime = finishTime;
                lastTrack = rr.Track;

            }
            return twenty != handicapTrack20 || forty != handicapTrack40;
        }
    }
}
