using ATG.ML.MLModels;
using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class TTWRandSort : ISorter<TimeAfterWinnerModel>
    {
        public IOrderedEnumerable<TimeAfterWinnerModel> Sort(IEnumerable<TimeAfterWinnerModel> entries, RaceModel race)
        {
            var r = new Random();
            return entries.OrderBy(rem => r.Next());
        }
    }
}
