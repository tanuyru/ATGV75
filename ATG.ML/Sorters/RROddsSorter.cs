using ATG.ML.MLModels;
using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class RROddsSorter : ISorter<RaceResultModel>
    {
        public IOrderedEnumerable<RaceResultModel> Sort(IEnumerable<RaceResultModel> entries, RaceModel race)
        {
            return entries.OrderBy(e => e.WinOdds == 0 ? 99 : e.WinOdds);
        }
    }
}
