using ATG.DB.Entities;
using ATG.ML.MLModels;
using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class RandomSorter : ISorter<RaceEntryModel>
    {
        public IOrderedEnumerable<RaceEntryModel> Sort(IEnumerable<RaceEntryModel> entries, RaceModel race)
        {
            var r = new Random();
            return entries.OrderBy(rem => r.Next());
        }
    }
}
