using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
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
    }
}
