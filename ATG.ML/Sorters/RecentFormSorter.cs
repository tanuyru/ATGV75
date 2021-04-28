using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class RecentFormSorter : ISorter<RaceEntryModel>
    {
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
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
