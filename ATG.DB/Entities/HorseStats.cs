using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.DB.Entities
{
    public class HorseStats
    {
        public long HorseId { get; set; }
        public string Name { get; set; }
        
        public long? ShortAuto { get; set; }
        public long? ShortVolt { get; set; }
        public long? ShortTime { get; set; }
        public long? MediumAuto { get; set; }
        public long? MediumVolt { get; set; }
        public long? MediumTime { get; set; }
        public long? LongAuto { get; set; }
        public long? LongVolt { get; set; }
        public long? LongTime { get; set; }
        public long? LastMonth { get; set; }
        public int? NumShort { get; set; }
        public int? NumMedium { get; set; }
        public int? NumLong { get; set; }

        public long GetAvgTotal()
        {
            int tot = 0;
            long sum = 0;
            if (NumShort.HasValue)
            {
                sum += ShortTime.Value;
                tot += NumShort.Value;
            }
            if (NumMedium.HasValue)
            {
                sum += MediumTime.Value;
                tot += NumMedium.Value;
            }
            if (NumLong.HasValue)
            {
                tot += NumLong.Value;
                sum += LongTime.Value;
            }
            return sum / tot;
        }
    }
}
