using ATG.ML.MLModels;
using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class TTWDistSorter : ISorter<TimeAfterWinnerModel>
    {
        public IOrderedEnumerable<TimeAfterWinnerModel> Sort(IEnumerable<TimeAfterWinnerModel> entries, RaceModel race)
        {
            return entries.OrderByDescending(rem => rem.Distribution);
        }
    }
}
