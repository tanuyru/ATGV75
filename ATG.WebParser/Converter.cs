using ATG.DB.Entities;
using ATG.Shared.Enums;
using ATG.WebParser.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.WebParser
{
    public static class Converter
    {
        public static Arena ParseArena(TrackJson t)
        {
            Arena a = new Arena();
            a.Country = t.countryCode;
            a.Id = t.id;
            a.Name = t.name;
            a.Condition = t.condition;
           
            return a;
        }
        public static ComboGame ParseGame(PoolJson g, GameTypeEnum gt)
        {
            ComboGame cg = new ComboGame();
            cg.GameId = g.id;
            cg.GameType = gt;
            cg.Name = g.id;
            cg.Systems = (long)g.systemCount;
            cg.Turnover =(long) g.turnover;
            cg.Races = new List<GameRace>();
            if (g.result != null && g.result.payouts != null)
            {
                foreach(var p in g.result.payouts)
                {
                    var pay = new GamePayout();
                    pay.Game = cg;
                    pay.NumWins = p.Key;
                    pay.Payout = p.Value.payout;
                    cg.Payouts.Add(pay);
                }
            }
            return cg;
        }

        public static Race ParseRaceFromHistory(HorseHistoryRace race)
        {
            Race r = new Race();
            r.Distance = race.start.distance;
            r.MediaId = race.mediaId;
            r.Name = race.race.id;
            r.RaceId = race.race.id;
            if (race.race.number.HasValue)
                r.RaceOrder = race.race.number.Value;
            r.Races = new List<GameRace>();
            if (race.date.HasValue)
            {
                r.StartTime = race.date.Value;
                r.ScheduledStartTime = race.date.Value;
            }
            r.Sport = race.race.sport;
            if (race.race.startMethod == "auto")
            {
                r.StartType = StartTypeEnum.Auto;
            }
            else if (race.race.startMethod == "volte")
            {
                r.StartType = StartTypeEnum.Volt;
            }
            else
            {
                r.StartType = StartTypeEnum.Unknown;
            }
            return r;
        }

        public static RaceResult ParseHistoryResult(HorseHistoryRace json)
        {
            RaceResult rr = new RaceResult();

           
            if (json?.kmTime != null)
            {
                rr.KmTime = json.kmTime.ParsedTimeSpan();
            }
            else
            {
                rr.KmTime = TimeSpan.Zero;
            }
            rr.Position = json.start.postPosition;
            rr.Track = json.start.postPosition;
            rr.KmTimeMilliSeconds = (long)rr.KmTime.TotalMilliseconds;
            if (json.place != null && int.TryParse(json.place, out var finish))
            {
                rr.FinishPosition = finish;
            }
            if (json.odds.HasValue)
            {
                rr.WinOdds = json.odds.Value;
            }
            rr.Distributions = new List<GameDistribution>();

            return rr;
        }
        public static bool ParseShoes(HorseSetupJson setup, out bool front, out bool back)
        {
            front = true;
            back = true;
            if (setup.shoes != null && setup.shoes.front.HasValue && setup.shoes.back.HasValue)
            {
                front = setup.shoes.front.Value;
                back = setup.shoes.back.Value;
                return true;
            }
            return false;
        }
        public static Race ParseRace(RaceJson r)
        {
            Race race = new Race();
            race.Distance = r.distance;
            race.MediaId = r.mediaId;
            race.Name = r.name;
            race.RaceId = r.id;
            race.Sport = r.sport;
            race.RaceOrder = r.number;
            if (r.scheduledStartTime.HasValue)
            {
                race.ScheduledStartTime = r.scheduledStartTime.Value;
            }
            if (r.startTime.HasValue)
            {
                race.StartTime = r.startTime.Value;
            }
            if (r.startMethod == "auto")
            {
                race.StartType = StartTypeEnum.Auto;
            }
            else if (r.startMethod == "volte")
            {
                race.StartType = StartTypeEnum.Volt;
            }
            else
            {
                race.StartType = StartTypeEnum.Unknown;
            }
            race.Races = new List<GameRace>();
            return race;
        }

        public static Horse ParseHorse(HorseJson h, bool parsePedigree = false)
        {
            Horse horse = new Horse();
            horse.BirthYear = DateTime.Now.Year - h.age;
            horse.Id = h.id;
            horse.Name = h.name;
            horse.Country = h.nationality;
            horse.Gender = h.sex;
            if (parsePedigree && h.pedigree != null)
            {
                if (h.pedigree.mother != null)
                {
                    horse.Mother = ParseHorse(h.pedigree.mother);
                }
                if (h.pedigree.father != null)
                {
                    horse.Father = ParseHorse(h.pedigree.father);
                }
                if (h.pedigree.grandfather != null)
                {
                    horse.GrandFather = ParseHorse(h.pedigree.grandfather);
                }
                if (h.pedigree.grandmother != null)
                {
                    horse.GrandMother = ParseHorse(h.pedigree.grandmother);
                }


                if (h.owner != null)
                {
                    horse.Owner = ParseOwner(h.owner);
                }
                if (h.trainer != null)
                {
                    horse.Trainer = ParseTrainer(h.trainer);
                }
                if (h.breeder != null)
                {
                    horse.Breeder = ParseBreeder(h.breeder);
                }
            }
            return horse;
        }
        public static Owner ParseOwner(OwnerJson d)
        {
            Owner driver = new Owner();
            driver.Id = d.id;
            driver.Name = d.name;
            driver.Country = d.location;
            return driver;
        }
        public static Trainer ParseTrainer(OwnerJson d)
        {
            Trainer driver = new Trainer();
            driver.Id = d.id;
            driver.Name = d.firstName+" "+d.lastName;
            driver.Location = d.location;
            driver.ShortName = d.shortName;
            return driver;
        }
        public static Breeder ParseBreeder(OwnerJson d)
        {
            Breeder driver = new Breeder();
            driver.Id = d.id;
            driver.Name = d.name;
            driver.Location = d.location;
            return driver;
        }
        public static Driver ParseDriver(DriverJson d)
        {
            Driver driver = new Driver();
            driver.BirthYear = d.birth;
            driver.Id = d.id;
            driver.Name = d.firstName + " " + d.lastName;
            driver.ShortName = d.shortName;
            return driver;
        }

        public static AvailableGame ParseAvailableGame(string id, DateTime defDate)
        {
            AvailableGame ag = new AvailableGame();
            ag.GameId = id;
            ag.ScheduledStartTime = defDate;
            return ag;
        }
        public static TrackDay ParseTrackDay(DayTrackJson json, DateTime date)
        {
            TrackDay td = new TrackDay();
            td.Date = date;
            if (json.races != null)
            {
                td.NumRaces = json.races.Count;
            }
            return td;
        }
        public static RaceResult ParseRaceResult(RaceStartJson json, int raceDistance, PoolDistJson vinnarJson)
        {
            
            RaceResult rr = new RaceResult();
            
            
            if (json.result?.kmTime != null)
            {
                rr.KmTime = json.result.kmTime.ParsedTimeSpan();
            }
            else
            {
                rr.KmTime = TimeSpan.Zero;
            }
            if (json.result != null)
            rr.Position = json.postPosition;
            rr.Track = json.number;
            rr.Galopp = json.galloped ?? false;
            rr.DQ = json.disqualified ?? false;
            rr.Distance = json.distance;
            rr.DistanceHandicap = json.distance - raceDistance;
            if (json.horse.shoes != null && json.horse.shoes.reported)
            {
                rr.BackShoes = json.horse.shoes.back.hasShoe;
                rr.FrontShoes = json.horse.shoes.front.hasShoe;
                rr.FrontChange = json.horse.shoes.front.changed;
                rr.BackChange = json.horse.shoes.back.changed;
            }
            if(json.scratched.HasValue)
            {
                rr.Scratched = json.scratched.Value;
            }
            rr.KmTimeMilliSeconds = (long)rr.KmTime.TotalMilliseconds;
            if (json.result != null)
            {
                rr.FinishPosition = json.result.finishOrder;
            }
            if (json.result != null && json.result.finalOdds != 0)
            {
                rr.WinOdds = json.result.finalOdds;
            }
            else if (json.pools != null)
            {
                if (json.pools.TryGetValue("vinnare", out var pooldist))
                {
                    rr.WinOdds = pooldist.odds / 100.0;
                }
            }
            else if(vinnarJson != null)
            {
                rr.WinOdds = 0;
            }
            if (json.result != null && json.result.prizeMoney.HasValue)
            {
                rr.PrizeMoney = (int)json.result.prizeMoney.Value;
            }
            rr.Distributions = new List<GameDistribution>();
       
            return rr;
        }
        public static GameDistribution ParseGameDistribution(PoolDistJson json)
        {
            GameDistribution gd = new GameDistribution();
            gd.Distribution = (json.betDistribution/10000);
            if (json.result != null && json.result.systems != null)
            {
                gd.SystemsLeft = (int)json.result.systemsDouble();
            }
            return gd;
        }
    }
}
