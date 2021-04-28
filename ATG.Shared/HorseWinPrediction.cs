using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.Shared
{
    public class HorseWinPrediction
    {
        public long HorseId { get; set; }
        public float Time { get; set; }
        public float WinOdds { get; set; }
        public float Distribution { get; set; }
        public int FinishPosition { get; set; }
    }
}
