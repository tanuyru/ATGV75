using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("GameRace")]
    public class GameRace
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Game")]
        public long GameId { get; set; }
        public ComboGame Game { get; set; }

        [ForeignKey("Race")]
        public long RaceId { get; set; }
        public Race Race { get; set; }
        public int GameRaceIndex { get; set; }
    }
}
