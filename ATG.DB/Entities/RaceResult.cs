using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("RaceResult")]
    public class RaceResult
    {
        [Key]
        public long Id { get; set; }

        public int FinishPosition { get; set; }

        [ForeignKey("Horse")]
        public long HorseId { get; set; }
        public Horse Horse { get; set; }

        [ForeignKey("Trainer")]
        public long? TrainerId { get; set; }
        public Trainer Trainer { get; set; }
        [ForeignKey("Race")]
        public long RaceFKId { get; set; }
        public Race Race { get; set; }
        [ForeignKey("Driver")]
        public long DriverId { get; set; }
        public Driver Driver { get; set; }
        public int Track { get; set; }
        public int Position { get; set; }
        public TimeSpan KmTime { get; set; }
        public long KmTimeMilliSeconds { get; set; }
        public double WinOdds { get; set; }
        public double PlatsOdds { get; set; }
        public int PrizeMoney { get; set; }
        public bool DQ { get; set; }
        public bool Galopp { get; set; }
        public bool? FrontShoes { get; set; }
        public bool? BackShoes { get; set; }
        public bool? FrontChange { get; set; }
        public bool? BackChange { get; set; }
        public bool Scratched { get; set; }

        public long TimeBehindWinner { get; set; }

        public double HorseMoneyTotal { get; set; }
        public double HorseMoneyLastYear { get; set; }
        public double HorseMoneyThisYear { get; set; }

        public double MoneyPerStart { get; set; }
        public double MoneyPerStartTotal { get; set; }
        public double LastYearMoneyPerStart { get; set; }
        public int Starts { get; set; }
        public int Wins { get; set; }
        public int Seconds { get; set; }
        public int Thirds { get; set; }

        public double HorseTotalMoney { get; set; }
        public double HorseTotalWinPercent { get; set; }
        public double HorseTotalPlacepercent { get; set; }
        public int LastYearStarts { get; set; }
        public int LastYearWins { get; set; }
        public int LastYearSeconds { get; set; }
        public int LastYearThirds { get; set; }

        public double AverageOddsLastFive { get; set; }
        public double TrainerWinPercent { get; set; }
        public double HorseWinPercent { get; set; }
        public double DriverWinPercent { get; set; }
        public double LastYearHorseWinPercent { get; set; }
        public double TrainerMoneyThisYear { get; set; }
        public double TrainerMoneyLastYear { get; set; }
        public double TrainerStarts { get; set; }
        public double TrainerWins { get; set; }
        public double TrainerSeconds { get; set; }
        public double TrainerThirds { get; set; }

        public double LastYearTrainerStarts { get; set; }
        public double LastYearTrainerWins { get; set; }
        public double LastYearTrainerSeconds { get; set; }
        public double LastYearTrainerThirds { get; set; }

        public double LastYearTrainerWinPercent { get; set; }

        public double DriverMoneyLastYear { get; set; }
        public double DriverMoney { get; set; }
        public int DriverStarts { get; set; }
        public int DriverWins { get; set; }
        public int DriverSeconds { get; set; }
        public int DriverThirds { get; set; }

        public int LastYearDriverStarts { get; set; }
        public int LastYearDriverWins { get; set; }
        public int LastYearDriverSeconds { get; set; }
        public int LastYearDriverThirds { get; set; }
        public double LastYearDriverWinPercent { get; set; }
        public ICollection<GameDistribution> Distributions { get; set; }

        public double FinishTimeMilliseconds { get; set; }
        public bool EstimatedFinishTime { get; set; }
        public float FinishTimeAfterWinner { get; set; }

        public int Distance { get; set; }
        public int DistanceHandicap { get; set; }

        public double Distribution { get; set; }
    }
}
