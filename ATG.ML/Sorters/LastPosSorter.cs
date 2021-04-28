using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class LastPosSorter : ISorter<RaceEntryModel>
    {
        public readonly bool UnkownLast;
        public LastPosSorter(bool unknownLast = true)
        {
            UnkownLast = unknownLast;
        }
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            
            return entries.OrderBy(rem => rem.LastFinishPosition != 0 ? rem.LastFinishPosition : (UnkownLast ? 16 : 0));
        }
    }
}
