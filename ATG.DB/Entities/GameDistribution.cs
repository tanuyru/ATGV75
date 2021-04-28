
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("GameDistribution")]
    public class GameDistribution
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Game")]
        public long GameId { get; set; }
        public ComboGame Game { get; set; }

        [ForeignKey("RaceResult")]
        public long RaceResultId { get; set; }
        public RaceResult Result { get; set; }

        public double Distribution { get; set; }
        public int SystemsLeft { get; set; }

    }
}
