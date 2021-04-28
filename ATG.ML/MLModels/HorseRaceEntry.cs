using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.ML.MLModels
{
    public class HorsRaceTime
    {
        [ColumnName("Score")]
        public float KmTime { get; set; }
    }
    public class HorseRaceEntry
    {
        [LoadColumn(0)]
        public long HorseId { get; set; }
        [LoadColumn(1)]
        public long DriverId { get; set; }

        [LoadColumn(2)]
        public float HorseAge { get; set; }
        //public string HorseGender { get; set; }
        //public long FatherHorseId { get; set; }
        //public long MotherHorseId { get; set; }
        [LoadColumn(3)]
        public float AvgKmTime { get; set; }
        [LoadColumn(4)]
        public float LastFinishPosition { get; set; }
        [LoadColumn(5)]
        public float AvgFinishPosition { get; set; }
        [LoadColumn(6)]
        public float HorseWinPercent { get; set; }
        [LoadColumn(7)]
        public float HorsePlacePercent { get; set; }
        //public double DriverWinPercent { get; set; }
        //public double DriverPlacePercent { get; set; }

        //public long TrainerId { get; set; }
        //public long OwnerId { get; set; }

        [LoadColumn(8)]
        public float ShoesFront { get; set; }
        [LoadColumn(9)]
        public float ShoesBack { get; set; }

        [LoadColumn(10)]
        public float FrontChanged { get; set; }
        [LoadColumn(11)]
        public float BackChanged { get; set; }
        [LoadColumn(12)]
        public float Track { get; set; }
        [LoadColumn(13)]
        public float IsVoltStart { get; set; }
        [LoadColumn(14)]
        public float Distance { get; set; }
        [LoadColumn(15)]
        public long ArenaId { get; set; }
        [LoadColumn(16)]
        public float DaysSinceLastRace { get; set; }
        [LoadColumn(17)]
        public float RaceAvgPosition { get; set; }
        [LoadColumn(18)]
        public float RaceAvgKmTime { get; set; }
        [LoadColumn(19)]
        public float RaceTopAvgKmTime { get; set; }
        [LoadColumn(20)]
        public float RaceBestKmTime { get; set; }
        [LoadColumn(21)]
        public float BestHorseOnTrack { get; set; }
        [LoadColumn(22)]
        public float NumHorsesInRace { get; set; }

        [LoadColumn(23)]
        public float WinOdds { get; set; }

        [LoadColumn(24)]
        public float KmTime { get; set; }
    }
}
