using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("Race")]
    public class Race
    {
        [Key]
        public long Id { get; set; }

        public string RaceId { get; set; }
        public string Name { get; set; }
        public int Distance { get; set; }

        public string Sport { get; set; }
        [ForeignKey("Arena")]
        public long? ArenaId { get; set; }
        public Arena Arena { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public int RaceOrder { get; set; }
        public StartTypeEnum StartType { get; set; }
        public string MediaId { get; set; }

        public int? Leader500StartNumber { get; set; }
        public int? Leader1000StartNumber { get; set; }


        public long? First500KmTime { get; set; }
        public long? First1000KmTime { get; set; }
        public long? Last500KmTime { get; set; }

        public double WinnerFinishTime { get; set; }

        public double SystemsLostPercent { get; set; }
        public long SystemsLost { get; set; }
        public ICollection<GameRace> Races { get; set; }

        public ICollection<RaceResult> RaceResults { get; set; } = new List<RaceResult>();


    }
}
