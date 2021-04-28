using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class MinDistSorter : ISorter<RaceEntryModel>
    {
        public readonly double MinDistToTake;
        public MinDistSorter(float minDist)
        {
            MinDistToTake = minDist;
        }
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            var dist = DistSort(entries, race).ToList();
            var form = FormSort(entries, race).ToList();
            HashSet<string> usedHorses = new HashSet<string>();
            List<RaceEntryModel> models = new List<RaceEntryModel>();

            for (int i = 0; i < dist.Count && dist[i].Distribution > MinDistToTake; i++)
            {
                models.Add(dist[i]);
                usedHorses.Add(dist[i].HorseName);
            }
            foreach(var f in form)
            {
                if (usedHorses.Contains(f.HorseName))
                    continue;
                models.Add(f);
            }
            return models.OrderBy(rem => models.IndexOf(rem));
        }

        private IOrderedEnumerable<RaceEntryModel> DistSort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            return entries.OrderByDescending(rem => rem.Distribution);
        }

        public IOrderedEnumerable<RaceEntryModel> FormSort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            if (!entries.Any(rem => rem.AvgRecentTime > 0))
            {
                return entries.OrderBy(rem => rem.AvgKmTime == 0 ? 0 : rem.AvgKmTime);
            }
            var avg = entries.Where(rem => rem.AvgRecentTime > 0).Average(rem => rem.AvgRecentTime);
            return entries.OrderBy(rem => rem.AvgRecentTime == 0 ? avg : rem.AvgRecentTime);
        }
    }
}
