using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Models
{
    public static class Extensions
    {
        public static float GetMedian(this IEnumerable<float> source)
        {
            // Create a copy of the input, and sort the copy
            float[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                float a = temp[count / 2 - 1];
                float b = temp[count / 2];
                return (a + b) / 2;
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }
        public static List<float> Normalize(this IEnumerable<float> source)
        {
            var max = source.Max();
            var min = source.Min();
            var diff = max - min;

            List<float> result = new List<float>();
            foreach (var d in source)
            {
                var part = (d - min) / diff;
                result.Add(part);
            }
            return result;
        }

        public static float Normalize(this float d, float min, float diff)
        {
            return (d - min) / diff;
        }
        public static float Normalize(this float d, IEnumerable<float> all)
        {
            if (!all.Any())
            {
                return 0.5f;
            }
            var min = all.Min();
            var diff = all.Max() - min;
            return (d - min) / diff;
        }
        public static double Normalize(this double d, double min, double diff)
        {
            return (d - min) / diff;
        }
        public static double GetMedian(this IEnumerable<long> source)
        {
            // Create a copy of the input, and sort the copy
            long[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                long a = temp[count / 2 - 1];
                long b = temp[count / 2];
                return (a + b) / 2;
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }
        public static List<double> Normalize(this IEnumerable<double> source)
        {
            var max = source.Max();
            var min = source.Min();
            var diff = max - min;

            List<double> result = new List<double>();
            foreach(var d in source)
            {
                var part = (d - min) / diff;
                result.Add(part);
            }
            return result;
        }
    
    }
}
