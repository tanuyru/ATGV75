using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("AvailableGame")]
    public class AvailableGame
    {
        [Key]
        public long Id { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public string GameId { get; set; }
    }
}
