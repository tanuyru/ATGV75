using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("Race")]
    public class Race
    {
        [Key]
        public long Id { get; set; }

        public long RaceDayId { get; set; }
        public string RaceId { get; set; }
        public string Name { get; set; }
        public int Distance { get; set; }

        public string TrackCondition { get; set; }
        public string Sport { get; set; }
        [ForeignKey("Arena")]
        public long? ArenaId { get; set; }
        public Arena Arena { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public int RaceOrder { get; set; }
        public StartTypeEnum StartType { get; set; }
        public string MediaId { get; set; }

        [ForeignKey("Leader500Horse")]
        public long? Leader500HorseId { get; set; }

        [ForeignKey("Leader1000Horse")]
        public long? Leader1000HorseId { get; set; }
        public Horse Leader500Horse { get; set; }
        public Horse Leader1000Horse { get; set; }

        public long? First500KmTime { get; set; }
        public long? First1000KmTime { get; set; }
        public long? Last500KmTime { get; set; }

        public double? First500SpeedRatio { get; set; }
        public double? First1000SpeedRatio { get; set; }
        public double? Last500SpeedRatio { get; set; }

        public double? StartSpeedFigure { get; set; }
        public int? First500Handicap { get; set; }
        public int? First500Position { get; set; }

        public int? First1000Handicap { get; set; }
        public int? First1000Position { get; set; }

        public double WinnerFinishTime { get; set; }
        public double WinnerKmTimeMilliseconds { get; set; }
        public long? WinnerHorseId { get; set; }
        public long? WinnerDriverId { get; set; }
        public long? WinnerTrainerId { get; set; }
        public double LastFinishTime { get; set; }
        public double LastPlaceFinishTime { get; set; }
        public double SystemsLostPercent { get; set; }
        public long SystemsLost { get; set; }
        
        public string InvalidReason { get; set; }
        public ICollection<GameRace> Races { get; set; }

        public ICollection<RaceResult> RaceResults { get; set; }

        public int DetailStatsVersion { get; set; }

    }
}
