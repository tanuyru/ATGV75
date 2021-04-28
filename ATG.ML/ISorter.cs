using ATG.ML.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ATG.ML
{
    public interface ISorter<T>
    {
        IOrderedEnumerable<T> Sort(IEnumerable<T> entries, RaceModel race);
    }
}
