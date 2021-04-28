using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ATG.DB.Entities
{
    [Table("Horse")]
    public class Horse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Name { get; set; }
        public int BirthYear { get; set; }
        public Arena HomeArena { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public double Money { get; set; }

        public long StartPoints { get; set; }

        [ForeignKey("Father")]


        public long? FatherHorseId { get; set; }
        [ForeignKey("Mother")]
        public long? MotherHorseId { get; set; }

        [ForeignKey("GrandFather")]
        public long? GrandFatherHorseId { get; set; }
        [ForeignKey("GrandMother")]
        public long? GrandMotherHorseId { get; set; }

        public Horse Father { get; set; }
        public Horse Mother { get; set; }
        public Horse GrandFather { get; set; }
        public Horse GrandMother { get; set; }

        [ForeignKey("Owner")]
        public long? OwnerId { get; set; }
        public Owner Owner { get; set; }

        [ForeignKey("Trainer")]
        public long? TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [ForeignKey("Breeder")]
        public long? BreederId { get; set; }
        public Breeder Breeder { get; set; }

        /*
        public ICollection<Horse> FatherChildren { get; set; }
        public ICollection<Horse> MotherChildren { get; set; }
        public ICollection<Horse> GrandFatherChildren { get; set; }
        public ICollection<Horse> GrandMotherChildren { get; set; }
        */

        public ICollection<RaceResult> RaceResults { get; set; }
    }
}
