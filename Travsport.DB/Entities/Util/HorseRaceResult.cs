using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travsport.DB.Entities.Util
{
    public enum TrackConditionEnum
    {
        Light,
        Heavier,
        Heavy,
        Winter
    }
    public class HorseRaceResult
    {
        public static int GetDistanceBucket(int distance)
        {
            if (distance < 1800)
            {
                return 0;
            }
            else if (distance >= 1800 && distance <= 2300)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        public HorseRaceResult(Race race, RaceResult rr)
        {
            HorseId = rr.HorseId;
            RaceTimestamp = race.StartTime;
            Distance = GetDistanceBucket(race.Distance);
            Handicap = rr.DistanceHandicap;
            if (rr.DQ || KmTime == 0)
            {
                KmTime = (long)((race.LastFinishTime / race.Distance) * 1000.0);
            }
            else
            {
                KmTime = rr.KmTimeMilliSeconds;
            }
            Galopp = rr.Galopp;
            DQ = rr.DQ;
            FinishPlacePosition = rr.FinishPosition;
            Track = rr.PositionForDistance;
            StartType = race.StartType;
            Money = rr.WonPrizeMoney;
            Placed = rr.FinishPosition > 0;
            Top3 = rr.FinishPosition > 0 && rr.FinishPosition < 4;
            Won = rr.FinishPosition == 1;
            if (race.TrackCondition == "Lätt bana")
            {
                TrackCondition = TrackConditionEnum.Light;
            }
            else if (race.TrackCondition == "Något tung bana")
            {
                TrackCondition = TrackConditionEnum.Heavier;
            }
            else if (race.TrackCondition == "Tung bana")
            {
                TrackCondition = TrackConditionEnum.Heavy;
            }
            else
            {
                TrackCondition = TrackConditionEnum.Winter;
            }

            var lastAfterWinner = (float)(race.LastFinishTime - race.WinnerFinishTime);
            var placeAfterWinner = (float)(race.LastPlaceFinishTime - race.WinnerFinishTime);

            TimeAfterWinner = (float)rr.FinishTimeAfterWinner;
            if (TimeAfterWinner == 0 || TimeAfterWinner > lastAfterWinner)
            {
                TimeAfterWinnerCappedLast = lastAfterWinner;
            }
            else
            {
                TimeAfterWinnerCappedLast = TimeAfterWinner;
            }

            if (TimeAfterWinner == 0 || TimeAfterWinner > placeAfterWinner)
            {
                TimeAfterWinnerPlaceCapped = placeAfterWinner;
            }
            else
            {
                TimeAfterWinnerPlaceCapped = TimeAfterWinner;
            }


            TimeAfterWinnerNormalizedMinMax = HorseStats.Normalize(TimeAfterWinnerCappedLast, 0, lastAfterWinner);
            TimeAfterWinnerNormalizedMinPlaced = HorseStats.Normalize(TimeAfterWinnerPlaceCapped, 0, placeAfterWinner);
       
        }
        public long HorseId;
        public DateTime RaceTimestamp;
        
        public int Distance;
        public int Handicap;

        public long KmTime;


        public float TimeAfterWinner;
        public float TimeAfterWinnerPlaceCapped;
        public float TimeAfterWinnerCappedLast;

        public float TimeAfterWinnerNormalizedMinMax;
        public float TimeAfterWinnerNormalizedMinPlaced;

        public bool Galopp;
        public bool DQ;
        public int FinishPlacePosition;
        public int Track;
        public StartTypeEnum StartType;
        public float Money;
        public bool Placed;
        public bool Top3;
        public bool Won;
        public TrackConditionEnum TrackCondition;

    }
}
