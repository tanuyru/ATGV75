using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class FormDistributionSorter : ISorter<RaceEntryModel>
    {
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            if (!entries.Any(rem => rem.AvgRecentTime > 0))
            {
                return entries.OrderBy(rem => rem.AvgKmTime);
            }
            var avg = entries.Where(rem => rem.AvgRecentTime > 0).Average(rem => rem.AvgRecentTime);
            return entries.OrderBy(rem => rem.AvgRecentTime == 0 ? (rem.AvgKmTime == 0 ? avg : rem.AvgKmTime) : rem.AvgRecentTime);
        }
    }
}
