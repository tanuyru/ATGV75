using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("Driver")]
    public class Driver
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public long Id { get; set; }
        public string Name { get; set; }
        public int BirthYear { get; set; }
        [ForeignKey("HomeArena")]
        public long? HomeArenaId { get; set; }
        public Arena HomeArena { get; set; }
        public string Dummy { get; set; }
        public string ShortName { get; set; }
    }
}
