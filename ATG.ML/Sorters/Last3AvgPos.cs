using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class Last3AvgPos : ISorter<RaceEntryModel>
    {
        public readonly bool UnkownLast;
        public Last3AvgPos(bool unknownLast = true)
        {
            UnkownLast = unknownLast;
        }
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {

            return entries.OrderBy(rem => rem.Last3AvgFinishPosition != 0 ? rem.Last3AvgFinishPosition : (UnkownLast ? 16 : 0));
        }
    }
}
