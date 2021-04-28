using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("Arena")]
    public class Arena
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string Name { get; set; }
        public string Country { get; set; }
        /// <summary>
        /// Maybe really not connected to condition, should be on race!?
        /// </summary>
        public string Condition { get; set; }

        public ICollection<Race> Races { get; set; }

        public ICollection<Horse> HomeHorses { get; set; }
        public ICollection<Driver> HomeDrivers { get; set; }
    }
}
