using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class TrackSorter : ISorter<RaceEntryModel>
    {
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            return entries.OrderBy(rem => rem.StartNumber);
        }
    }
}
