using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class AvgTimeTotalSorter : ISorter<RaceEntryModel>
    {
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            var avg = entries.Where(rem => rem.AvgKmTime > 0).Average(rem => rem.AvgKmTime);
            return entries.OrderBy(rem => rem.AvgKmTime == 0 ? avg : rem.AvgKmTime);
        }
    }
}
