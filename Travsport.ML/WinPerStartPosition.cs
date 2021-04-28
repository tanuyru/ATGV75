using ATG.Shared.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Travsport.DB.Entities;
using Travsport.DB.Entities.Util;

namespace Travsport.ML
{
    public class WinPerStartPosition
    {
        public static int GetDistanceHandicapBucket(int handicap)
        {
            if (handicap == 20)
            {
                return 1;
            }
            else if (handicap > 20)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }
        [JsonProperty("StartWinDictionary")]
        public Dictionary<StartTypeEnum, Dictionary<int, Dictionary<int, Dictionary<int, float>>>> winPerDistPosHandicap { get; set; } =
             new Dictionary<StartTypeEnum, Dictionary<int, Dictionary<int, Dictionary<int, float>>>>();
        public DateTime? MaxDate { get; set; }
        public WinPerStartPosition()
        {

        }
        public WinPerStartPosition(IEnumerable<RaceResult> results)
        {
            var validResults = results.Where(rr => !rr.Scratched);
            Dictionary<StartTypeEnum, Dictionary<int, Dictionary<int, Dictionary<int, int>>>> tot = new Dictionary<StartTypeEnum, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>();
            Dictionary<StartTypeEnum, Dictionary<int, Dictionary<int, Dictionary<int, int>>>> won = new Dictionary<StartTypeEnum, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>();

            foreach (var rr in validResults)
            {
                if (!tot.TryGetValue(rr.Race.StartType, out var distDic))
                {
                    distDic = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                    tot.Add(rr.Race.StartType, distDic);
                }
                int db = HorseRaceResult.GetDistanceBucket(rr.Distance);
                if (!distDic.TryGetValue(db, out var posDic))
                {
                    posDic = new Dictionary<int, Dictionary<int, int>>();
                    distDic.Add(db, posDic);
                }
                if (!posDic.TryGetValue(rr.PositionForDistance, out var handicapDic))
                {
                    handicapDic = new Dictionary<int, int>();
                    posDic.Add(rr.PositionForDistance, handicapDic);
                }
                var hb = GetDistanceHandicapBucket(rr.DistanceHandicap);
                if (!handicapDic.ContainsKey(hb))
                {
                    handicapDic.Add(hb, 0);
                }
                handicapDic[hb]++;

                if (rr.FinishPosition == 1)
                {
                    if (!won.TryGetValue(rr.Race.StartType, out var wonDistDic))
                    {
                        wonDistDic = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                        won.Add(rr.Race.StartType, wonDistDic);
                    }

                    if (!wonDistDic.TryGetValue(db, out var wonPosDic))
                    {
                        wonPosDic = new Dictionary<int, Dictionary<int, int>>();
                        wonDistDic.Add(db, wonPosDic);
                    }
                    if (!wonPosDic.TryGetValue(rr.PositionForDistance, out var hDic))
                    {
                        hDic = new Dictionary<int, int>();
                        wonPosDic.Add(rr.PositionForDistance, hDic);
                    }

                    if (!hDic.ContainsKey(hb))
                    {
                        hDic.Add(hb, 0);
                    }
                    hDic[hb]++;
                }
            }

            foreach (var kvp1 in tot)
            {
                var distDic = new Dictionary<int, Dictionary<int, Dictionary<int, float>>>();
                winPerDistPosHandicap.Add(kvp1.Key, distDic);

                foreach (var kvp2 in kvp1.Value)
                {
                    var dic = new Dictionary<int, Dictionary<int, float>>();
                    distDic.Add(kvp2.Key, dic);
                    foreach (var kvp3 in kvp2.Value)
                    {
                        var lastDic = new Dictionary<int, float>();
                        dic.Add(kvp3.Key, lastDic);
                        foreach (var kvp4 in kvp3.Value)
                        {
                            if (won.TryGetValue(kvp1.Key, out var wonDistDic) && wonDistDic.TryGetValue(kvp2.Key, out var wonDic) && wonDic.TryGetValue(kvp3.Key, out var wonLastDic) && wonLastDic.TryGetValue(kvp4.Key, out var numTot))
                            {

                                lastDic.Add(kvp4.Key, numTot / (float)kvp4.Value);
                            }
                            else
                            {
                                lastDic.Add(kvp4.Key, 0);
                            }
                        }
                    }
                }
            }
        }
     
        public void GetKeys(out List<StartTypeEnum> starts, out List<int> distanceBuckets, out List<int> handicapBuckets, out List<int> positions)
        {
            starts = winPerDistPosHandicap.Keys.ToList();

            distanceBuckets = winPerDistPosHandicap.Values.SelectMany(kvp => kvp.Keys).ToList();

            positions = winPerDistPosHandicap.Values.SelectMany(kvp => kvp.Values).SelectMany(kvp => kvp.Keys).ToList();

            handicapBuckets = winPerDistPosHandicap.Values.SelectMany(kvp => kvp.Values).SelectMany(kvp => kvp.Values).SelectMany(kvp => kvp.Keys).ToList();
        }
        public float GetWinRatio(StartTypeEnum startType, int distanceBucket, int handicapBucket, int position)
        {
            if (winPerDistPosHandicap.TryGetValue(startType, out var d1) && d1.TryGetValue(distanceBucket, out var d2) && d2.TryGetValue(position, out var d3) && d3.TryGetValue(handicapBucket, out var winRatio))
            {
                return winRatio;
            }
            return 0;
        }
    }
}
