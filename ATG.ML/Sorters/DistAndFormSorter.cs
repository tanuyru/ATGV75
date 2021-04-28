using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class DistAndFormSorter : ISorter<RaceEntryModel>
    {
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            var dist = DistSort(entries, race).ToList();
            var form = FormSort(entries, race).ToList();
            HashSet<string> usedHorses = new HashSet<string>();
            List<RaceEntryModel> models = new List<RaceEntryModel>();
            while(dist.Count > 0 || form.Count > 0)
            {

                while (dist.Count > 0 && usedHorses.Contains(dist[0].HorseName))
                {
                    dist.RemoveAt(0);
                }
                if (dist.Count > 0)
                {
                    models.Add(dist[0]);
                    usedHorses.Add(dist[0].HorseName);
                    dist.RemoveAt(0);
                }
                while (form.Count > 0 && usedHorses.Contains(form[0].HorseName))
                {
                    form.RemoveAt(0);
                }
                if (form.Count > 0)
                {
                    usedHorses.Add(form[0].HorseName);
                    models.Add(form[0]);
                    form.RemoveAt(0);
                }
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
