using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.ML.Models
{
    public class RaceEntryModel
    {
        public string HorseName { get; set; }
        public string DriverName { get; set; }
        public long AvgKmTime { get; set; }
        public long ShortAvgKmTime { get; set; }
        public long MediumAvgKmTime { get; set; }
        public long LongAvgKmTime { get; set; }
        public long AvgRecentTime { get; set; }

        public double Distribution { get; set; }

        public int DistRank { get; set; }
        public int FinishPosition { get; set; }
        public int StartGroup { get; set; }
        public string RaceId { get; set; }
        public int StartNumber { get; set; }
        public long HorseId { get; set; }
        public RaceModel Race { get; set; }
        public int LastFinishPosition { get; set; }
        public double Last3AvgFinishPosition { get; set; }
        public int DistVsFinishDiff
        {
            get
            {
                return Math.Abs(FinishPosition - DistRank);
            }
        }
    }
}
