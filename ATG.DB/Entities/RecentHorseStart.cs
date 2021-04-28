using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("RecentHorseStart")]
    public class RecentHorseStart
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Horse")]
        public long HorseId { get; set; }

        public Horse Horse { get; set; }
        public long KmTimeMilliseconds { get; set; }
        public bool Galloped { get; set; }
        public bool DQ { get; set; }
        public DateTime Date { get; set; }
        public string RaceId { get; set; }
        public long Distance { get; set; }
        public string Sport { get; set; }
        public StartTypeEnum StartMethod { get; set; }
        public int Track { get; set; }
        public double WinOdds { get; set; }
    }
}
