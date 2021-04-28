using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("GamePayout")]
    public class GamePayout
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Game")]
        public long GameId { get; set; }
        public ComboGame Game { get; set; }
        public int NumWins { get; set; }
        public double Payout { get; set; }
    }
}
