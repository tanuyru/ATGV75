using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.ML.MLModels
{
    public class TimeResult
    {
        [ColumnName("Score")]
        public float TimeAfterWinner { get; set; }
    }
    public class TimeAfterWinnerModel
    {
        [LoadColumn(0)]
        public float ArenaDistanceRel { get; set; }
        [LoadColumn(1)]
        public float DaysSinceLastRace { get; set; }

        [LoadColumn(2)]
        public float DriverWinPercentRel { get; set; }
        [LoadColumn(3)]
        public float EquipmentChange { get; set; }
        [LoadColumn(4)]
        public int GallopedLast { get; set; }
        [LoadColumn(5)]
        public float HorseWinPercentRel { get; set; }

        [LoadColumn(6)]
        public float MedianRelSpeed { get; set; }

        [LoadColumn(7)]
        public float RecentRelSpeed { get; set; }




        [LoadColumn(8)]
        public float TrackWinAtDistance { get; set; }

        [LoadColumn(9)]
        public float Distribution { get; set; }

        [LoadColumn(10)]
        public float FinishPosition { get; set; }

        [LoadColumn(11)]
        public long RaceId { get; set; }
        [LoadColumn(12)]
        public float TimeAfterWinner { get; set; }


    }
}
