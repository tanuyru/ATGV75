using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.WebParser.Json
{
    public class NoRaceJson
    {
        public static long ParseTimeMilliseconds(string timeString, out bool gallop, out bool dq)
        {
            dq = false;
            gallop = false;
            if (timeString == null)
            {
                dq = true;
                return 0;
            }
            gallop = timeString.Contains("g");
            var splits = timeString.Split(' ');
            var timePart = splits[0].Trim();
            if (timePart.EndsWith('a'))
            {
                timePart = timePart.Substring(0, timePart.Length - 1);
            }
            timePart = timePart.Trim();
            if (!float.TryParse(timePart, out float seconds))
            {
                dq = true;
                return 0;
            }
            return (long)((60 + seconds) * 1000);
        }
        public bool success { get; set; }
        public NoRaceJsonResult result { get; set; }
       
    }
    public class NoRaceJsonResult
    {
        public long GetFirst500()
        {
            return NoRaceJson.ParseTimeMilliseconds(first500MetersTime, out var tmp, out var tmp2);
        }
        public long GetFirst1900()
        {
            return NoRaceJson.ParseTimeMilliseconds(first1000MetersTime, out var tmp, out var tmp2);
        }
        public long Last500()
        {
            return NoRaceJson.ParseTimeMilliseconds(last500MetersTime, out var tmp, out var tmp2);
        }

        public int GetFirst500Number()
        {
            var split = first500MetersHorseName.Split(' ');
            if (int.TryParse(split[0], out var start))
            {
                return start;
            }
            return 0;
        }
        public int GetFirst1000Number()
        {
            var split = first1000MetersHorseName.Split(' ');
            if (int.TryParse(split[0], out var start))
            {
                return start;
            }
            return 0;
        }

        public string first500MetersTime { get; set; }
        public string first1000MetersTime { get; set; }
        public string first500MetersHorseName { get; set; }
        public string first1000MetersHorseName { get; set; }
        public string last500MetersTime { get; set; }

        public List<NoHorseResultJson> results { get; set; }
    }

    public class NoHorseResultJson
    {
        public int startNumber { get; set; }
        public string kmTime { get; set; }
        public int order { get; set; } // FinishPos
        public long ParseKmTime(out bool gallop, out bool dq)
        {
            gallop = false;
            if (string.IsNullOrEmpty(kmTime))
            {
                // Assume dq but not sure?
                dq = true;
                return 0;
            }
            
            return NoRaceJson.ParseTimeMilliseconds(kmTime, out gallop, out dq);
        }

    }
}
