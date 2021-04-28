using ATG.DB.Entities;
using ATG.ML.MLModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Models
{
    public class TrackWinInfo
    {
        public TrackWinInfo()
        { }

        public TrackWinInfo(IEnumerable<RaceResult> rrs)
        {
            if (!rrs.Any())
                return;
            var trackGroups = rrs.GroupBy(rr => rr.Track);
            int totRaces = rrs.Select(rr => rr.Race.Id).Distinct().Count();
            Dictionary<int, int> totWithAtLeastStarters = new Dictionary<int, int>();
            for(int i = 1; i < 22; i++)
            {
                var atLeastI = rrs.Where(rr => rr.Track >= i).Select(rr => rr.RaceFKId).Distinct().Count();
                totWithAtLeastStarters.Add(i, atLeastI);
            }
            foreach(var g in trackGroups)
            {
                var totWithTracks = totWithAtLeastStarters[g.Key];
                var trackDic = new Dictionary<int, float>();
                var numWins = g.Count(rr => rr.FinishPosition == 1);
                WinPerTrack.Add(g.Key, (float)numWins / (float)totWithTracks);
                var dists = g.Select(rr => RaceResultModel.GetDistanceBucket(rr.Race.Distance)).Distinct();
                foreach(var d in dists)
                {
                    var forDist = g.Where(rr => RaceResultModel.GetDistanceBucket(rr.Race.Distance) == d);
                    int numDist = forDist.Count();
                    int numWinDist = forDist.Count(rr => rr.FinishPosition == 1);
                    float winRatio = numWinDist / (float)numDist;
                    trackDic.Add(d, winRatio);
                }
                WinPerTrackAndDistance.Add(g.Key, trackDic);
            }
            AverageWinTime = rrs.Where(rr => rr.FinishPosition == 1).Select(rr => rr.KmTimeMilliSeconds).Average();
        }
        public Dictionary<int, Dictionary<int, float>> WinPerTrackAndDistance { get; set; } = new Dictionary<int, Dictionary<int, float>>();

        public Dictionary<int, float> WinPerTrack { get; set; } = new Dictionary<int, float>();
        public double AverageWinTime { get; set; }
        public double AverageRelTime { get; set; }
    }
}
