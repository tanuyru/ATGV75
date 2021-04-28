using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Travsport.DB.Entities
{
    public class RaceStats
    {
        static List<long> openStrechIds = new List<long>()
        {
            1,
            23
        };
        public RaceStats()
        { }

        public RaceStats(Race race)
        {
            RaceId = race.Id;
            RaceTimestamp = race.StartTime;
            WinnerTime = (float)race.WinnerFinishTime;
            LastPlacedTime = (float)race.LastPlaceFinishTime;
            LastTime = (float)race.LastFinishTime;
            TrackCondition = race.TrackCondition;
            Distance = race.Distance;
            if (race.ArenaId.HasValue)
            {
                OpenStrech = openStrechIds.Contains(race.ArenaId.Value);
            }
        }
      
        public long RaceId { get; set; }
        public DateTime RaceTimestamp { get; set; }
        public float First500SpeedRatio { get; set; }
        public float First1000SpeedRatio { get; set; }
        public float Last500SpeedRatio { get; set; }

        /// <summary>
        /// What line have the fastest horsies?
        /// </summary>
        public float FirstHandicapSpeedFigure { get; set; }
        public float SecondHandicapSpeedFigure { get; set; }
        public float ThirdHandicapSpeedFigure { get; set; }
        public float FourthHandicapSpeedFigure { get; set; }

        public float WinnerTime { get; set; }
        public float LastPlacedTime { get; set; }
        public float LastTime { get; set; }

        public string TrackCondition { get; set; }
        public int Distance { get; set; }
        public bool OpenStrech { get; set; }

    }
}
