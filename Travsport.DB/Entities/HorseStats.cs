using ATG.Shared.Enums;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Travsport.DB.Entities.Util;

namespace Travsport.DB.Entities
{
    [Table("HorseStats")]
    public class HorseStats
    {
        private static long tempId = 1;
        public static long GetNextTempId()
        {
            var next = tempId;
            tempId++;
            return next;
        }
        
        enum HorseProfileKey
        {
            TimeAfterWinner,
            KmTime,
        }
       // static HorseProfileKey CurrKey = HorseProfileKey.TimeAfterWinner;
        static TimeSpan MonthShapeTime = TimeSpan.FromDays(30);
        static TimeSpan StatsShapeTime = TimeSpan.FromDays(30);
        public static float Normalize(float d, float min, float diff)
        {
            if (diff == 0)
                return d;
            return (d - min) / diff;
        }
        public HorseStats()
        { }
        public HorseStats(long horseId, long raceResultId,  RaceStats race, IEnumerable<HorseRaceResult> horseHistory, IEnumerable<HorseRaceResult> driverHistory)
        {
            HorseId = horseId;

            RaceResultId = raceResultId;
            RaceDate = race.RaceTimestamp;
            var validHorseHistory = horseHistory;

            var autoHistory = validHorseHistory.Where(rr => rr.StartType == StartTypeEnum.Auto).ToList();
            var voltHistory = validHorseHistory.Where(rr => rr.StartType == StartTypeEnum.Volt).ToList();

            var shortHorseHistory = validHorseHistory.Where(hh => hh.Distance == 0).ToList();
            var mediumHorseHistory = validHorseHistory.Where(hh => hh.Distance == 1).ToList();
            var longHorseHistory = validHorseHistory.Where(hh => hh.Distance == 2).ToList();

            var shortAutoHistory = shortHorseHistory.Where(horseHistory => horseHistory.StartType == StartTypeEnum.Auto).ToList();
            var shortVoltHistory = shortHorseHistory.Where(horseHistory => horseHistory.StartType == StartTypeEnum.Volt).ToList();
            var mediumAutoHistory = mediumHorseHistory.Where(horseHistory => horseHistory.StartType == StartTypeEnum.Auto).ToList();
            var mediumVoltHistory = mediumHorseHistory.Where(horseHistory => horseHistory.StartType == StartTypeEnum.Volt).ToList();
            var longAutoHistory = longHorseHistory.Where(horseHistory => horseHistory.StartType == StartTypeEnum.Auto).ToList();
            var longVoltHistory = longHorseHistory.Where(horseHistory => horseHistory.StartType == StartTypeEnum.Volt).ToList();

            var from = race.RaceTimestamp.Add(-MonthShapeTime);
            var monthHistory = validHorseHistory.Where(hh => hh.RaceTimestamp >= from);

            var fromStatsShape = race.RaceTimestamp.Add(-StatsShapeTime);
            var statsShape = horseHistory.Where(hh => hh.RaceTimestamp >= fromStatsShape);
           
            NumShortAuto = shortAutoHistory.Count;
            NumShortVolt = shortVoltHistory.Count;
            NumMediumAuto = mediumAutoHistory.Count;
            NumMediumVolt = mediumVoltHistory.Count;
            NumLongAuto = longAutoHistory.Count;
            NumLongVolt = longVoltHistory.Count;

            NumShort = NumShortAuto + NumShortVolt;
            NumMedium = NumMediumAuto + NumMediumVolt;
            NumLong = NumLongAuto + NumLongVolt;

            NumVolts = NumShortVolt + NumMediumVolt + NumLongVolt;
            NumAutos = NumShortAuto + NumMediumAuto + NumLongAuto;

            NumShortLastMonth = monthHistory.Count(rr => rr.Distance == 0);
            NumMediumLastMonth = monthHistory.Count(rr => rr.Distance == 1);
            NumLongLastMonth = monthHistory.Count(rr => rr.Distance == 2);

            NumLastMonth = NumShortLastMonth + NumMediumLastMonth + NumLongLastMonth;

            NumShape = statsShape.Count();
            NumTotals = NumVolts + NumAutos;

            if (NumShort > 0)
            {
                if (NumShortAuto > 0)
                {
                    var times = shortAutoHistory.Select(rr => rr.KmTime);
                    MedianShortAuto = GetMedian(times);
                    AverageShortAuto = (long)times.Average();
                    BestShortAuto = (long)times.Min();
                }
                if (NumShortVolt > 0)
                {
                    var times = shortVoltHistory.Select(rr => rr.KmTime);
                    MedianShortVolt = GetMedian(times);
                    AverageShortVolt = (long)times.Average();
                    BestShortVolt = times.Min();
                }
                if (NumShortAuto + NumShortVolt > 0)
                {
                    BestShortTime = shortHorseHistory.Select(rr => rr.KmTime).Min();
                }
            }

            if (NumMedium > 0)
            {
                if (NumMediumAuto > 0)
                {
                    var times = mediumAutoHistory.Select(rr => rr.KmTime);
                    MedianMediumAuto = GetMedian(times);
                    AverageMediumAuto = (long)times.Average();
                    BestMediumAuto = (long)times.Min();
                }
                if (NumMediumVolt > 0)
                {
                    var times = mediumVoltHistory.Select(rr => rr.KmTime);
                    MedianMediumVolt = GetMedian(times);
                    AverageMediumVolt = (long)times.Average();
                    BestMediumVolt = times.Min();
                }
                if (NumMediumAuto + NumMediumVolt > 0)
                {
                    BestMediumTime = mediumHorseHistory.Select(rr => rr.KmTime).Min();
                }
            }

            if (NumLong > 0)
            {
                if (NumLongAuto > 0)
                {
                    var times = longAutoHistory.Select(rr => rr.KmTime);
                    MedianLongAuto = GetMedian(times);
                    AverageLongAuto = (long)times.Average();
                    BestLongAuto = (long)times.Min();
                }
                if (NumLongVolt > 0)
                {
                    var times = longVoltHistory.Select(rr => rr.KmTime);
                    MedianLongVolt = GetMedian(times);
                    AverageLongVolt = (long)times.Average();
                    BestLongVolt = times.Min();
                }
                if (NumLongAuto + NumLongVolt > 0)
                {
                    BestLongTime = longHorseHistory.Select(rr => rr.KmTime).Min();
                }
            }

            if (NumTotals > 0)
            {
                Best = validHorseHistory.Select(rr => rr.KmTime).Min();
                Average = (long)validHorseHistory.Select(rr => rr.KmTime).Average();
                Median = GetMedian(validHorseHistory.Select(rr => rr.KmTime));

            }
            if (NumAutos > 0)
            {
                BestAuto = autoHistory.Select(rr => rr.KmTime).Min();
                AverageAuto = (long)autoHistory.Select(rr => rr.KmTime).Average();
                MedianAuto = GetMedian(autoHistory.Select(rr => rr.KmTime));
            }
            if (NumVolts > 0)
            {
                BestVolt = voltHistory.Select(rr => rr.KmTime).Min();
                AverageVolt = (long)voltHistory.Select(rr => rr.KmTime).Average();
                MedianVolt = GetMedian(voltHistory.Select(rr => rr.KmTime));
            }

            if (NumLastMonth > 0)
            {
                if (NumShortLastMonth > 0)
                {
                    var shortMonthHistory = monthHistory.Where(rr => rr.Distance == 0).Select(hh => hh.KmTime);
                    
                    BestLastMonthShort = shortMonthHistory.Min();
                    BestLastMonth = BestLastMonthShort;
                    AverageLastMonthShort = (long)shortMonthHistory.Average();
                    MedianLastMonthShort = (long)GetMedian(shortMonthHistory);

                }
                if (NumMediumLastMonth > 0)
                {
                    var mediumMonthHistory = monthHistory.Where(rr => rr.Distance == 1).Select(hh => hh.KmTime);
                    BestLastMonthMedium = mediumMonthHistory.Min();
                    if (!BestLastMonth.HasValue || BestLastMonth.Value > BestLastMonthMedium.Value)
                        BestLastMonth = BestLastMonthMedium.Value;
                    AverageLastMonthMedium = (long)mediumMonthHistory.Average();
                    MedianLastMonthMedium = (long)GetMedian(mediumMonthHistory);

                }
                if (NumLongLastMonth > 0)
                {
                    var longMonthHistory = monthHistory.Where(rr => rr.Distance == 2).Select(hh => hh.KmTime);
                    BestLastMonthLong = longMonthHistory.Min();
                    if (!BestLastMonth.HasValue || BestLastMonth.Value > BestLastMonthLong.Value)
                        BestLastMonth = BestLastMonthLong.Value;
                    AverageLastMonthLong = (long)longMonthHistory.Average();
                    MedianLastMonthLong = (long)GetMedian(longMonthHistory);
                }

            }

            if (NumTotals > 0)
            {
                var last = horseHistory.OrderByDescending(rr => rr.RaceTimestamp).First();
                TimeAfterWinnerLast = last.TimeAfterWinnerCappedLast;

                WinPercent = horseHistory.Count(rr => rr.Won) / (float)NumTotals;
                Top3Percent = horseHistory.Count(rr => rr.Top3) / (float)NumTotals;
                PlacePercent = horseHistory.Count(rr => rr.Placed) / (float)NumTotals;
                MoneyPerStart = horseHistory.Sum(rr => rr.Money) / (float)NumTotals;

                TimeAfterWinnerAverage = horseHistory.Average(rr => rr.TimeAfterWinnerCappedLast);
                TimeAfterWinnerMedian = GetMedian(horseHistory.Select(rr => (long)rr.TimeAfterWinnerCappedLast));
            }
            if (NumShape > 0)
            {
                WinShape = statsShape.Count(rr => rr.Won) / (float)NumShape;
                PlaceShape = statsShape.Count(rr => rr.Placed) / (float)NumShape;
                Top3Shape = statsShape.Count(rr => rr.Top3) / (float)NumShape;
                MoneyPerStartShape = statsShape.Sum(rr => rr.Money) / (float)NumShape;
                TimeAfterWinnerShapeAverage = statsShape.Average(rr => rr.TimeAfterWinnerCappedLast);
                TimeAfterWinnerShapeMedian = GetMedian(statsShape.Select(rr => (long)rr.TimeAfterWinnerCappedLast));
            }

            NumDriverHistory = driverHistory.Count();
            if (NumDriverHistory > 0)
            {
                DriverWinPercent = driverHistory.Count(horseHistory => horseHistory.Won) / (float)NumDriverHistory;
                DriverPlacePercent = driverHistory.Count(hh => hh.Placed) / (float)NumDriverHistory;
                DriverTop3Percent = driverHistory.Count(horseHistory => horseHistory.Top3) / (float)NumDriverHistory;
                DriverMoneyPerStart = driverHistory.Sum(hh => hh.Money) / (float)NumDriverHistory;

                var voltDriverHistory = driverHistory.Where(hh => hh.StartType == StartTypeEnum.Volt);
                var autoDriverHistory = driverHistory.Where(hh => hh.StartType == StartTypeEnum.Auto);

                NumDriverHistoryAuto = autoDriverHistory.Count();
                NumDriverHistoryVolt = voltDriverHistory.Count();
                if (NumDriverHistoryAuto > 0)
                {
                    DriverWinPercentAuto = autoDriverHistory.Count(horseHistory => horseHistory.Won) / (float)NumDriverHistoryAuto;
                    DriverPlacePercentAuto = autoDriverHistory.Count(hh => hh.Placed) / (float)NumDriverHistoryAuto;
                    DriverTop3PercentAuto = autoDriverHistory.Count(horseHistory => horseHistory.Top3) / (float)NumDriverHistoryAuto;
                    DriverMoneyPerStartAuto = autoDriverHistory.Sum(hh => hh.Money) / (float)NumDriverHistoryAuto;

                }
                if (NumDriverHistoryVolt > 0)
                {
                    DriverWinPercentVolt = voltDriverHistory.Count(horseHistory => horseHistory.Won) / (float)NumDriverHistoryVolt;
                    DriverPlacePercentVolt = voltDriverHistory.Count(hh => hh.Placed) / (float)NumDriverHistoryVolt;
                    DriverTop3PercentVolt = voltDriverHistory.Count(horseHistory => horseHistory.Top3) / (float)NumDriverHistoryVolt;
                    DriverMoneyPerStartVolt = voltDriverHistory.Sum(hh => hh.Money) / (float)NumDriverHistoryVolt;


                }

            }

            if (NumTotals > 0)
            {  
                var lightHistory = horseHistory.Where(rr => rr.TrackCondition == TrackConditionEnum.Light);
                var heavierHistory = horseHistory.Where(rr => rr.TrackCondition == TrackConditionEnum.Heavier);
                var heavyHistory = horseHistory.Where(rr => rr.TrackCondition == TrackConditionEnum.Heavy);
                var winterHistory = horseHistory.Where(rr => rr.TrackCondition == TrackConditionEnum.Winter);
                NumLight = lightHistory.Count();
                NumHeavier = heavierHistory.Count();
                NumHeavy = heavyHistory.Count();
                NumWinter = winterHistory.Count();
                TimeAfterWinnerLastCapProfile = CreateProfile(
                        (rr) => rr.TimeAfterWinnerCappedLast,
                        validHorseHistory,
                        autoHistory,
                        voltHistory,
                        longHorseHistory,
                        mediumHorseHistory,
                        shortHorseHistory,
                        lightHistory,
                        heavierHistory,
                        heavyHistory,
                        winterHistory);
                if (TimeAfterWinnerLastCapProfile != null)
                {
                    TimeAfterWinnerLastCapProfileIdTemp = GetNextTempId();
                    TimeAfterWinnerLastCapProfile.TempId = TimeAfterWinnerLastCapProfileIdTemp.Value;
                }
                TimeAfterWinnerPlaceCapProfile = CreateProfile(
                        (rr) => rr.TimeAfterWinnerPlaceCapped,
                        validHorseHistory,
                        autoHistory,
                        voltHistory,
                        longHorseHistory,
                        mediumHorseHistory,
                        shortHorseHistory,
                        lightHistory,
                        heavierHistory,
                        heavyHistory,
                        winterHistory);
                if (TimeAfterWinnerPlaceCapProfile != null)
                {
                    TimeAfterWinnerPlaceCapProfileIdTemp = GetNextTempId();
                    TimeAfterWinnerPlaceCapProfile.TempId = TimeAfterWinnerPlaceCapProfileIdTemp.Value;
                }

                TimeAfterWinnerMinMaxProfile = CreateProfile(
                        (rr) => rr.TimeAfterWinnerNormalizedMinMax,
                        validHorseHistory,
                        autoHistory,
                        voltHistory,
                        longHorseHistory,
                        mediumHorseHistory,
                        shortHorseHistory,
                        lightHistory,
                        heavierHistory,
                        heavyHistory,
                        winterHistory);

                if (TimeAfterWinnerMinMaxProfile != null)
                {
                    TimeAfterWinnerMinMaxProfileIdTemp = GetNextTempId();
                    TimeAfterWinnerMinMaxProfile.TempId = TimeAfterWinnerMinMaxProfileIdTemp.Value;
                }

                TimeAfterWinnerNormMinPlacedCapProfile = CreateProfile(
                        (rr) => rr.TimeAfterWinnerNormalizedMinPlaced,
                        validHorseHistory,
                        autoHistory,
                        voltHistory,
                        longHorseHistory,
                        mediumHorseHistory,
                        shortHorseHistory,
                        lightHistory,
                        heavierHistory,
                        heavyHistory,
                        winterHistory);
                if (TimeAfterWinnerNormMinPlacedCapProfile != null)
                {
                    TimeAfterWinnerNormMinPlacedCapProfileIdTemp = GetNextTempId();
                    TimeAfterWinnerNormMinPlacedCapProfile.TempId = TimeAfterWinnerNormMinPlacedCapProfileIdTemp.Value;
                }

                KmTimeValidProfile = CreateProfile(
                        (rr) => rr.KmTime,
                        validHorseHistory,
                        autoHistory,
                        voltHistory,
                        longHorseHistory,
                        mediumHorseHistory,
                        shortHorseHistory,
                        lightHistory,
                        heavierHistory,
                        heavyHistory,
                        winterHistory);
                if (KmTimeValidProfile != null)
                {
                    KmTimeValidProfileIdTemp = GetNextTempId();
                    KmTimeValidProfile.TempId = KmTimeValidProfileIdTemp.Value;
                }

            }

        }
        private HorseStatsProfile CreateProfile(
            Func<HorseRaceResult, float> selectorFunc,
            IEnumerable<HorseRaceResult> validResults,
            IEnumerable<HorseRaceResult> autoHistory,
            IEnumerable<HorseRaceResult> voltHistory,
            IEnumerable<HorseRaceResult> longHistory,
            IEnumerable<HorseRaceResult> mediumHistory,
            IEnumerable<HorseRaceResult> shortHistory,
            IEnumerable<HorseRaceResult> lightHistory,
            IEnumerable<HorseRaceResult> heavierHistory,
            IEnumerable<HorseRaceResult> heavyHistory,
            IEnumerable<HorseRaceResult> winterHistory)
        {
            var profile = new HorseStatsProfile();

            var avgTime = validResults.Average(rr => selectorFunc(rr));
            if (avgTime == 0)
                return profile;
          //  var minTime = allTimes.Min();
           // var diffTime = allTimes.Max() - minTime;
            // All results are equal, no need to create profile, can use "default"?
           
            if (autoHistory.Any())
                profile.AutoStartFactor = autoHistory.Average(rr => selectorFunc(rr)) / avgTime;


            if (voltHistory.Any())
                profile.VoltStartFactor = voltHistory.Average(rr => selectorFunc(rr)) / avgTime;

            if (longHistory.Any())
                profile.LongDistanceFactor = longHistory.Average(rr => selectorFunc(rr)) / avgTime;
            if (mediumHistory.Any())
                profile.MediumDistanceFactor = mediumHistory.Average(rr => selectorFunc(rr)) / avgTime;
            if (shortHistory.Any())
                profile.ShortDistanceFactor = shortHistory.Average(rr => selectorFunc(rr)) / avgTime;

            if (lightHistory.Any())
                profile.LightTrackConditionFactor = lightHistory.Average(rr => selectorFunc(rr)) / avgTime;
            if (heavierHistory.Any())
                profile.HeavierTrackConditionFactor = heavierHistory.Average(rr => selectorFunc(rr)) / avgTime;
            if (heavyHistory.Any())
                profile.HeavyTrackConditionFactor = heavyHistory.Average(rr => selectorFunc(rr)) / avgTime;
            if (winterHistory.Any())
                profile.WinterTrackConditionFactor = winterHistory.Average(rr => selectorFunc(rr)) / avgTime;
            if (profile.AutoStartFactor < 0 || profile.VoltStartFactor < 0 || profile.LongDistanceFactor < 0 || profile.MediumDistanceFactor < 0 ||
                profile.ShortDistanceFactor < 0 || profile.LightTrackConditionFactor < 0 || profile.HeavierTrackConditionFactor < 0 ||profile.HeavyTrackConditionFactor < 0 ||
                profile.WinterTrackConditionFactor < 0)
            {
                Console.WriteLine("Factor is below zero!?");
            }
            return profile;
        }


        public static float GetMedian(IEnumerable<float> source)
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
        public static long GetMedian(IEnumerable<long> source)
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

      
 
        [Key]
        public long Id { get; set; }

        [ForeignKey("RaceResult")]
        public long RaceResultId { get; set; }
        public long HorseId { get; set; }
        public DateTime RaceDate { get; set; }

        public RaceResult RaceResult { get; set; }
    
        public bool? Keyed { get; set; }
        #region Speedfigures

        #region Best
        public long? BestShortAuto { get; set; }
        public long? BestShortVolt { get; set; }
        public long? BestShortTime { get; set; }

        public long? BestMediumAuto { get; set; }
        public long? BestMediumVolt { get; set; }
        public long? BestMediumTime { get; set; }

        public long? BestLongAuto { get; set; }
        public long? BestLongVolt { get; set; }
        public long? BestLongTime { get; set; }


        public long? BestAuto { get; set; }
        public long? BestVolt { get; set; }
        public long? Best { get; set; }


        public long? BestLastMonthShort { get; set; }
        public long? BestLastMonthMedium { get; set; }
        public long? BestLastMonthLong { get; set; }
        public long? BestLastMonth { get; set; }
        #endregion

        #region Median
        public long? MedianLongAuto { get; set; }
        public long? MedianLongVolt { get; set; }
        public long? MedianLongTime { get; set; }

        public long? MedianShortAuto { get; set; }
        public long? MedianShortVolt { get; set; }
        public long? MedianShortTime { get; set; }


        public long? MedianMediumAuto { get; set; }
        public long? MedianMediumVolt { get; set; }
        public long? MedianMediumTime { get; set; }

        public long? MedianAuto { get; set; }
        public long? MedianVolt { get; set; }
        public long? Median { get; set; }


        public long? MedianLastMonthShort { get; set; }
        public long? MedianLastMonthMedium { get; set; }
        public long? MedianLastMonthLong { get; set; }
        public long? MedianLastMonth { get; set; }
        #endregion

        #region Average
        public long? AverageMediumAuto { get; set; }
        public long? AverageMediumVolt { get; set; }
        public long? AverageMediumTime { get; set; }

        public long? AverageLongAuto { get; set; }
        public long? AverageLongVolt { get; set; }
        public long? AverageLongTime { get; set; }

        public long? AverageShortAuto { get; set; }
        public long? AverageShortVolt { get; set; }
        public long? AverageShortTime { get; set; }

        public long? AverageAuto { get; set; }
        public long? AverageVolt { get; set; }
        public long? Average { get; set; }


        public long? AverageLastMonthShort { get; set; }
        public long? AverageLastMonthMedium { get; set; }
        public long? AverageLastMonthLong { get; set; }
        public long? AverageLastMonth { get; set; }
        #endregion

        #region Number of races
        public int NumShortAuto { get; set; }
        public int NumShortVolt { get; set; }

        public int NumMediumAuto { get; set; }
        public int NumMediumVolt { get; set; }

        public int NumLongAuto { get; set; }
        public int NumLongVolt { get; set; }

        public int NumShort { get; set; }
        public int NumMedium { get; set; }
        public int NumLong { get; set; }


        public int NumVolts { get; set; }
        public int NumAutos { get; set; }

        public int NumShortLastMonth { get; set; }
        public int NumMediumLastMonth { get; set; }
        public int NumLongLastMonth { get; set; }
        public int NumLastMonth { get; set; }

        public int NumShape { get; set; }

        public int NumLight { get; set; }
        public int NumHeavier { get; set; }
        public int NumHeavy { get; set; }
        public int NumWinter { get; set; }
        public int NumTotals { get; set; }
        #endregion



        #endregion

        #region Horse stats

        public float WinPercent { get; set; }
        public float Top3Percent { get; set; }
        public float PlacePercent { get; set; }
        public float MoneyPerStartShape { get; set; }
        public float MoneyPerStart { get; set; }
        public float WinShape { get; set; }
        public float PlaceShape { get; set; }
        public float Top3Shape { get; set; }
        public float TimeAfterWinnerShapeAverage { get; set; }
        public float TimeAfterWinnerShapeMedian { get; set; }
        public float TimeAfterWinnerLast { get; set; }
        public float TimeAfterWinnerMedian { get; set; }
        public float TimeAfterWinnerAverage { get; set; }

        #endregion

        #region Driver stats
        public int NumDriverHistory { get; set; }
        public float DriverWinPercent { get; set; }
        public float DriverTop3Percent { get; set; }
        public float DriverPlacePercent { get; set; }
        public float DriverMoneyPerStart { get; set; }

        public int NumDriverHistoryAuto { get; set; }
        public float DriverWinPercentAuto { get; set; }
        public float DriverTop3PercentAuto { get; set; }
        public float DriverPlacePercentAuto { get; set; }
        public float DriverMoneyPerStartAuto { get; set; }

        public int NumDriverHistoryVolt { get; set; }
        public float DriverWinPercentVolt { get; set; }
        public float DriverTop3PercentVolt { get; set; }
        public float DriverPlacePercentVolt { get; set; }
        public float DriverMoneyPerStartVolt { get; set; }

        #endregion


        #region Profile

        public HorseStatsProfile TimeAfterWinnerLastCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerPlaceCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerMinMaxProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerNormMinPlacedCapProfile { get; set; }
        public HorseStatsProfile KmTimeValidProfile { get; set; }

        [NotMapped]
        public long? TimeAfterWinnerLastCapProfileIdTemp { get; set; }

        [NotMapped]
        public long? TimeAfterWinnerPlaceCapProfileIdTemp { get; set; }

        [NotMapped]
        public long? TimeAfterWinnerMinMaxProfileIdTemp { get; set; }

        [NotMapped]
        public long? TimeAfterWinnerNormMinPlacedCapProfileIdTemp { get; set; }
        [NotMapped]
        public long? KmTimeValidProfileIdTemp { get; set; }


        [ForeignKey("TimeAfterWinnerLastCapProfile")]
        public long? TimeAfterWinnerLastCapProfileId { get; set; }

        [ForeignKey("TimeAfterWinnerPlaceCapProfile")]
        public long? TimeAfterWinnerPlaceCapProfileId { get; set; }

        [ForeignKey("TimeAfterWinnerMinMaxProfile")]
        public long? TimeAfterWinnerMinMaxProfileId { get; set; }

        [ForeignKey("TimeAfterWinnerNormMinPlacedCapProfile")]
        public long? TimeAfterWinnerNormMinPlacedCapProfileId { get; set; }


        [ForeignKey("KmTimeValidProfile")]
        public long? KmTimeValidProfileId { get; set; }

        #endregion
    }
}
