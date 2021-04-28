using ATG.Shared.Enums;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Travsport.DB.Entities
{
    [Table("ComboGame")]
    public class ComboGame
    {
        [Key]
        public long Id { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public long Turnover { get; set; }
        
        public long Systems { get; set; }
        public bool MissingRaceTimes { get; set; }
        public GameTypeEnum GameType { get; set; }
        public ICollection<GameRace> Races { get; set; }
        public ICollection<GameDistribution> Distributions { get; set; }
        public ICollection<GamePayout> Payouts { get; set; } = new List<GamePayout>();
        public GameRace AssureRace(Race race, int index)
        {
            var existing = Races.FirstOrDefault(gr => gr.Race.RaceId == race.RaceId);
            if (existing != null)
            {
                return existing;
            }
            
            GameRace gameRace = new GameRace();
            gameRace.Game = this;
            gameRace.Race = race;
            gameRace.GameRaceIndex = index;
            Races.Add(gameRace);
            return gameRace;
        }
    }
}
