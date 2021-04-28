using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("TrainerDriver")]
    public class TrainerDriver
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        public int BirthYear { get; set; }
        [ForeignKey("HomeArena")]
        public long? HomeArenaId { get; set; }
        public Arena HomeArena { get; set; }
        public string ShortName { get; set; }
        public bool Linkable { get; set; }
    }
}
