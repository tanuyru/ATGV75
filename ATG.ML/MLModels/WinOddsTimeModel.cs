using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.ML.MLModels
{
    public class WinOddsTimeResult
    {
        [ColumnName("Score")]
        public float WinOddsProbability { get; set; }
    }
    public class WinOddsTimeModel
    {
        public float WinOddsProbability { get; set; }
        public float TimeAfterWinner { get; set; }
    }
}
