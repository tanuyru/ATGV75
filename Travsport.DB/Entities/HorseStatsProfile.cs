using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("HorseStatsProfile")]
    public class HorseStatsProfile
    {
        public long TempId { get; set; }
        [Key]
        public long Id { get; set; }
        public float First500Factor { get; set; } = 1;
        public float First1000Factor { get; set; } = 1;
        public float Last500Factor { get; set; } = 1;

        public float ShortDistanceFactor { get; set; } = 1;
        public float MediumDistanceFactor { get; set; } = 1;
        public float LongDistanceFactor { get; set; } = 1;
        public float AutoStartFactor { get; set; } = 1;
        public float VoltStartFactor { get; set; } = 1;
        public float TrackConditionFactor { get; set; } = 1;
        public float LightTrackConditionFactor { get; set; } = 1;
        public float HeavierTrackConditionFactor { get; set; } = 1;
        public float HeavyTrackConditionFactor { get; set; } = 1;
        public float WinterTrackConditionFactor { get; set; } = 1;

    }
}
