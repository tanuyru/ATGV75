using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("TrackDay")]
    public class TrackDay
    {
        [Key]
        public long Id { get; set; }
        [ForeignKey("Arena")]
        public long ArenaId { get; set; }
        public Arena Arena { get; set; }
        public DateTime Date { get; set; }
        public int NumRaces { get; set; }
        public bool NewStartList { get; set; }
        public bool OldStartList { get; set; }
       // public ICollection<Race> Races { get; set; }
        public string BetType { get; set; }
        public string ShortBetType { get; set; }
        public string RaceDayGameTypes { get; set; }
        public long PrizeMoney { get; set; }
    }
}
