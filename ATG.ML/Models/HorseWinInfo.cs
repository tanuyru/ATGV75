using ATG.DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Models
{
    public class HorseWinInfo
    {
        public HorseWinInfo()
        { }

        public HorseWinInfo(IEnumerable<RecentHorseStart> rrs)
        {
            int tot = rrs.Count();
            HorseWinPercent = 0.5f;// rrs.Count(rr => rr.RaceResult.FinishPosition == 1) / (float)tot;
            if (tot > 1)
            {
               // MedianSpeed = rrs.Skip(tot / 2).First().RaceResult.KmTimeMilliSeconds;
            }
            else if (tot > 0)
            {
                //MedianSpeed = rrs.First().RaceResult.KmTimeMilliSeconds;
            }

        }
        public float HorseWinPercent { get; set; }
        public float MedianSpeed { get; set; }
        public float DaysSinceLastRace { get; set; }
    }
}
