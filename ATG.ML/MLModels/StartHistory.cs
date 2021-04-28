using ATG.DB.Entities;
using ATG.Shared.Enums;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Text;
using TS = Travsport.DB.Entities;
namespace ATG.ML.MLModels
{
    public class StartHistory
    {
        public StartHistory()
        {

        }
        public StartHistory(TS.RaceResult start)
        {
            RaceId = start.Race.RaceId;
            KmTimeMilliseconds = start.KmTimeMilliSeconds;
            Distance = start.Distance;
            StartType = start.Race.StartType;
            Track = start.PositionForDistance;
            DQ = start.DQ;
            Galloped = start.Galopp;
            WinOdds = (float)start.WinOdds;
            RaceDate = start.Race.StartTime;
            PlacePosition = start.FinishPosition;
            TrainerId = start.TrainerId;
            if (start.DriverId.HasValue)
                DriverId = start.DriverId.Value;
            HorseId = start.HorseId;
            FinishTime = (float)start.FinishTimeMilliseconds;
            PrizeMoneyWon = start.WonPrizeMoney;
            Handicap = start.DistanceHandicap;
            TimeAfterWinner = (float)start.FinishTimeAfterWinner;
        }
        public StartHistory(RaceResult start)
        {
            RaceId = start.Race.RaceId;
            KmTimeMilliseconds = start.KmTimeMilliSeconds;
            Distance = start.Distance;
            StartType = start.Race.StartType;
            Track = start.Track;
            DQ = start.DQ;
            Galloped = start.Galopp;
            WinOdds = (float)start.WinOdds;
            RaceDate = start.Race.StartTime;
            PlacePosition = start.FinishPosition;
            TrainerId = start.TrainerId;
            DriverId = start.DriverId;
            HorseId = start.HorseId;
            FinishTime =(float) start.FinishTimeMilliseconds;
            PrizeMoneyWon = start.PrizeMoney;
            Handicap = start.DistanceHandicap;
        }
        public StartHistory(RecentHorseStart start)
        {
            RaceId = start.RaceId;
            KmTimeMilliseconds = start.KmTimeMilliseconds;
            Distance = (int)start.Distance;
            StartType = start.StartMethod;
            Track = start.Track;
            DQ = start.DQ;
            Galloped = start.Galloped;
            WinOdds = (float)start.WinOdds;
            RaceDate = start.Date;
        }

        public float TimeAfterWinner { get; set; }

        public long HorseId { get; set; }
        public long DriverId { get; set; }
        public long? TrainerId { get; set; }
        public string RaceId { get; set; }
        public int Handicap { get; set; }
        public long KmTimeMilliseconds { get; set; }
        public int Distance { get; set; }
        public StartTypeEnum StartType { get; set; }
        public int Track { get; set; }
        public int PlacePosition { get; set; }
        public bool DQ { get; set; }
        public bool Galloped { get; set; }
        public float WinOdds { get; set; }
        public DateTime RaceDate { get; set; }
        public float PrizeMoneyWon { get; set; }
        public float FinishTime { get; set; }
    }
}
