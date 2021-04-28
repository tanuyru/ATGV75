using ATG.DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.WebParser.Json
{
    public class GameDayJson
    {
        public List<DayTrackJson> tracks { get; set; }
        public Dictionary<string, List<GameDayGame>> games { get; set; }
    }
    public class DayTrackJson
    {
        public long id { get; set; }
        public string countryCode { get; set; }
        public bool? trackChanged { get; set; }
        public string sport { get; set; }
        public string name { get; set; }
        public DateTime? startTime { get; set; }
        public string biggestGameType { get; set; }
        public List<DayRaceJson> races { get; set; }
    }
    public class DayRaceJson
    {
        public string id { get; set; }
        public int number { get; set; }

    }
    public class GameDayGame
    {
        public string id { get; set; }
       // public List<string> races { get; set; }
    }
    public class HorseInfoJson
    {
        public HorseRecordRaceJson horse { get; set; }
    }
    public class HorseRecordRaceJson
    {
        public DateTime? date { get; set; }
        public double odds { get; set; }
        public string place { get; set; }
        public string id { get; set; }
        public string mediaId { get; set; }
        public RaceJson race { get; set; }
        public TrackJson track { get; set; }
        public int money { get; set; }
        public HorseResultJson results { get; set; }
 
    }
    public class HorseResultJson
    {
        public List<HorseHistoryRace> records { get; set; }

        public double GetAverageKmTime()
        {
            if (records == null || FinishedRaces().Count() == 0)
                return 0;
            return FinishedRaces().Average(rec => rec.kmTime.ParsedTimeSpan().TotalMilliseconds);
        }
        public double GetAveragePosition()
        {
            if (records == null || FinishedRaces().Count() == 0)
                return 0;
            return FinishedRaces().Average(rec => rec.GetPlaceInt());
        }
        public int GetLastPos()
        {
            if (records == null || FinishedRaces().Count() == 0)
                return 0;
            return FinishedRaces().First().GetPlaceInt();
        }
        public bool GallopedLast()
        {
            if (records == null || records.Count() == 0)
                return false;
            return records.First().galloped.HasValue && records.First().galloped.Value;
        }
        public bool DqLast()
        {
            if (records == null || records.Count() == 0)
                return false;
            return records.First().disqualified.HasValue && records.First().disqualified.Value;
        }
        public double AvgOdds()
        {
            if (records == null || records.Count(hh => hh.odds.HasValue && hh.odds.Value != 0) == 0)
                return 0;
            return records.Where(hh => hh.odds.HasValue && hh.odds.Value != 0).Average(hh => hh.odds.Value);
        }
        public DateTime? LastStart()
        {
            if (records == null || records.Count() == 0)
                return null;
            return records.First().date;
        }
        public IEnumerable<HorseHistoryRace> FinishedRaces()
        {
            return records.Where(rec => rec.kmTime != null);
        }
    }
    public class HorseStartJson
    {
        public int distance { get; set; }
        public int postPosition { get; set; }
        public HorseSetupJson horse { get; set; }
        public DriverJson driver { get; set; }
    }
    public class HorseSetupJson
    {
        public HorseShoeJson shoes { get; set; }
       
    }
    public class HorseShoeJson
    {
        public bool? front { get; set; }
        public bool? back { get; set; }
    }
    public class GameJson
    {
        public string type { get; set; }

        public string id { get; set; }

        public Dictionary<string, PoolJson> pools { get; set; }

        public List<RaceJson> races { get; set; }

        public bool AllRacesValidTimes()
        {
            if (races == null)
                return false;
            return races.All(r => r.HaveRaceTimes());
        }
    }

    public class PoolJson
    {
        public string type { get; set; }
        public string id { get; set; }
        public double turnover { get; set; }
        public PoolComboResultJson result { get; set; }
        public double systemCount { get; set; }
        public DateTime? scheduledStartTime { get; set; }

    }

    public class PayoutJson
    {
        public int systems { get; set; }
        public double payout { get; set; }
    }

    public class PoolComboResultJson
    {
        public string type { get; set; }
        public string systems { get; set; }
        public double systemsDouble()
        {
            if (string.IsNullOrEmpty(systems))
                return 0;
            var noComma = systems.Replace(',', '.');
            if (double.TryParse(systems, out var d))
            {
                return d;
            }
            return 0;
        }
        public ValueJson value { get; set; }
        public Dictionary<int, PayoutJson> payouts { get; set; }
    }
    public class ValueJson
    {
        public double amount { get; set; }
    }

    public class RaceStartJson
    {
        public int number { get; set; }
        public int postPosition { get; set; }
        public int distance { get; set; }
        public bool? disqualified { get; set; }
        public bool? galloped { get; set; }
        public bool? scratched { get; set; }
        public List<VideoJson> videos { get; set; }
        public Dictionary<string, PoolDistJson> pools { get; set; }
        public HorseJson horse { get; set; }
        public DriverJson driver { get; set; }

        public RaceResultJson result { get; set; }

       

    }
    public class RaceResultJson
    {
        public int startNumber { get; set; }
        public int finishOrder { get; set; }
        public double finalOdds { get; set; }
        public double? prizeMoney { get; set; }
        public KmTidJson kmTime { get; set; }
    }
    public class KmTidJson
    {
        public int minutes { get; set; }
        public int seconds { get; set; }
        public int tenths { get; set; }
        public TimeSpan ParsedTimeSpan()
        {
            return new TimeSpan(0, 0, minutes, seconds, tenths * 100);
        }
    }
    public class DriverJson
    {
        public long id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int birth { get; set; }
        public string location { get; set; }
        public string shortName { get; set; }
        public TrackJson homeTrack { get; set; }
        public HistoryStats statistics { get; set; }
    }
    public class HistoryStats
    {
        public Dictionary<int, YearlyStats> years { get; set; }

        public YearlyStats life { get; set; }

  
    }
    public class YearlyStats
    {
        public int starts { get; set; }
        public double earnings { get; set; }
        public int winPercentage { get; set; }
        public double? placePercentage { get; set; }
        public double? bonusEarnings { get; set; }
        public double? earningsPerStart { get; set; }
        public int? startPoints { get; set; }
        public Dictionary<int, int> placement { get; set; }
    }
    public class PoolDistJson
    {
        public string type { get; set; }
        public double betDistribution { get; set; }
        public double odds { get; set; }
        public double minOdds { get; set; }
        public double maxOdds { get; set; }
        public PoolComboResultJson result { get; set; }
    }
    public class HorseJson
    {
        public long id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public string sex { get; set; }
        public string nationality { get; set; }
        public TrackJson homeTrack { get; set; }
        public HorseShoesRaceStartJson shoes { get; set; }
        public HorsePedigree pedigree { get; set; }

        public OwnerJson owner { get; set; }

        public OwnerJson trainer { get; set; }

        public OwnerJson breeder { get; set; }
        public HorseResultJson results { get; set; }

        public HistoryStats statistics { get; set; }
    }

    public class OwnerJson
    {
        public long id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string shortName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public HistoryStats statistics { get; set; }

    }
    public class HorsePedigree
    {
        public HorseJson father { get; set; }
        public HorseJson mother { get; set; }
        public HorseJson grandfather { get; set; }
        public HorseJson grandmother { get; set; }
    }
    public class HorseShoesRaceStartJson
    {
       public bool reported { get; set; }

        public ShoeSetup front { get; set; }
        public ShoeSetup back { get; set; }
    }
    public class ShoeSetup
    {
        public bool hasShoe { get; set; }
        public bool changed { get; set; }
    }
    public class VideoJson
    {
        public string mediaId { get; set; }
        public DateTime? timestamp { get; set; }
    }
    public class RaceJson
    {
        public string type { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? scheduledStartTime { get; set; }
        public int number { get; set; }
        public string startMethod { get; set; }
        public string sport { get; set; }
        public int distance { get; set; }
        public TrackJson track { get; set; }
        public string mediaId { get; set; }

        public List<RaceStartJson> starts { get; set; }
        public Dictionary<string, PoolDistJson> pools { get; set; }

        public bool HaveRaceTimes()
        {
            if (starts == null)
                return false;
            return starts.All(s => s.result != null && s.result.kmTime != null);
        }
    }

    public class HorseHistoryRace
    {
        public bool? link { get; set; }
        public bool? disqualified { get; set; }
        public bool? galloped { get; set; }
        public DateTime? date { get; set; }
        public double? odds { get; set; }
        public string place { get; set; }
        public int GetPlaceInt()
        {
            if (place == null)
                return 0;
            if (int.TryParse(place, out var i))
                return i;
            return 0;
        }
        public string mediaId { get; set; }
        public double? firstPrize { get; set; }
        public TrackJson track { get; set; }
        public HorseStartJson start { get; set; }
        public KmTidJson kmTime { get; set; }
        public HorseHistoryRaceInfo race { get; set; }

    }
    public class HorseHistoryRaceInfo
    {
        public string id { get; set; }
        public string sport { get; set; }
        public string type { get; set; }
        public string startMethod { get; set; }
        public int? number { get; set; }
    }
    public class TrackJson
    {
        public int id { get; set; }
        public string name { get; set; }
        public string countryCode { get; set; }

        public string condition { get; set; }
    }
}
