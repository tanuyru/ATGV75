using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("RaceResult")]
    public class RaceResult
    {
        public static int GetStartGroupSort(StartTypeEnum st, int posForDist, int distHandicap)
        {
            if (st == StartTypeEnum.Auto)
            {
                if (posForDist < 6)
                {
                    return 0;
                }
                else if (posForDist < 9)
                {
                    return 1;
                }    
                else
                {
                    return 2;
                }
            }
            else if (st == StartTypeEnum.Volt)
            {
                int handicap = distHandicap * 10;
           
                
                if (posForDist == 1)
                {
                    return 0 + handicap;
                }
                else if (posForDist < 4)
                {
                    return 1 + handicap;
                }
                else if (posForDist < 6)
                {
                    return 2 + handicap;
                }
                else if (posForDist < 8)
                {
                    return 3 + handicap;
                }
                else
                {
                    return 4 + handicap;
                }
                
                
            }
            return 0;
        }
        [Key]
        public long Id { get; set; }

        public HorseStats HorseStats { get; set; }
        [ForeignKey("HorseStats")]
        public long? HorseStatsId { get; set; }
        public int FinishPosition { get; set; }

        [ForeignKey("Horse")]
        public long HorseId { get; set; }
        public Horse Horse { get; set; }

        [ForeignKey("Trainer")]
        public long? TrainerId { get; set; }
        public TrainerDriver Trainer { get; set; }
        [ForeignKey("Race")]
        public long RaceFKId { get; set; }
        public Race Race { get; set; }
        [ForeignKey("Driver")]
        public long? DriverId { get; set; }
        public TrainerDriver Driver { get; set; }
        public string Sulky { get; set; }
        public int StartNumber { get; set; }
        public int PositionForDistance { get; set; }
        public long KmTimeMilliSeconds { get; set; }
        public double WinOdds { get; set; }
        public double ImpliedWinProb { get; set; }
        public double PlatsOdds { get; set; }
        public int WonPrizeMoney { get; set; }
        public bool DQ { get; set; }
        public bool Galopp { get; set; }
        public bool? FrontShoes { get; set; }
        public bool? BackShoes { get; set; }
        public bool? FrontChange { get; set; }
        public bool? BackChange { get; set; }
        public bool Scratched { get; set; }
        public bool? DriverChanged { get; set; }

        public double FinishTimeMilliseconds { get; set; }
        public double FinishTimeAfterWinner { get; set; }

        public double NormalizedFinishTime { get; set; }
        public double NormalizedFinishTimesPlaced { get; set; }
        public int Distance { get; set; }
        public int DistanceHandicap { get; set; }

        public double Distribution { get; set; }
        public double SmallDistribution { get; set; }
    }
}
