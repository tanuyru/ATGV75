using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.ML.MLModels
{
    public class RelativeModelWrapper
    {
        public RelativeModel Model { get; set; }
        public DateTime RaceDate { get; set; }
        public long HorseId { get; set; }
        public long RaceId { get; set; }
        public int RaceNumber { get; set; }
        public int FinishPosition { get; set; }
        public float WinOdds { get; set; }
    }
}
