using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("Breeder")]
    public class Breeder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public ICollection<Horse> Horses { get; set; }

    }
}
