using ATG.ML.MLModels;
using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class RRRandSorter : ISorter<RaceResultModel>
    {
        public IOrderedEnumerable<RaceResultModel> Sort(IEnumerable<RaceResultModel> entries, RaceModel race)
        {
            var r = new Random();
            return entries.OrderBy(e => r.Next());
        }
    }
}
