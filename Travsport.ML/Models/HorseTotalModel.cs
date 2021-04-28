using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using Travsport.DB.Entities;
using Travsport.DB.Entities.Util;

namespace Travsport.ML.Models
{
    public class HorseTotalModel
    {
        #region Column sets
        public static List<string> RelativeBestSpeedColumns = new List<string>()
        {
            "RelBestNormalized",
            "RelInGroupBestNormalized",
            "RelBestOnDistanceNormalized",
            "RelInGroupBestOnDistanceNormalized",

            "RelBestOnStartTypeNormalized",
            "RelInGroupBestOnStartTypeNormalized",
            "RelBestOnDistanceAndStartTypeNormalized",
            "RelInGroupBestOnDistanceAndStartTypeNormalized",

        };
        public static List<string> RelativeMedianSpeedColumns = new List<string>()
        {
            "RelMedianNormalized",
            "RelInGroupMedianNormalized",
            "RelMedianOnDistanceNormalized",
            "RelAverageOnDistanceNormalized",

            "RelMedianOnStartTypeNormalized",
            "RelInGroupMedianOnStartTypeNormalized",
            "RelMedianOnDistanceAndStartTypeNormalized",
            "RelInGroupMedianOnDistanceAndStartTypeNormalized",

        };
        public static List<string> RelativeAverageSpeedColumns = new List<string>()
        {
            "RelAverageNormalized",
            "RelInGroupAverageNormalized",
            "RelAverageOnDistanceNormalized",
            "RelInGroupAverageOnDistanceNormalized",

            "RelAverageOnStartTypeNormalized",
            "RelInGroupAverageOnStartTypeNormalized",
            "RelAverageOnDistanceAndStartTypeNormalized",
            "RelInGroupAverageOnDistanceAndStartTypeNormalized",

        };
        public static List<string> RelativeDriverStatsTotalColumns = new List<string>()
        {
            "RelDriverWinPercent",
            "RelDriverTop3Percent",
            "RelDriverPlacePercent",
            "RelDriverMoneyPerStart",
        };
        public static List<string> RelativeDriverStatsStatTypeColumns = new List<string>()
        {
            "RelDriverWinPercentStartType",
            "RelDriverTop3PercentStartType",
            "RelDriverPlacePercentStartType",
            "RelDriverMoneyPerStartStartType",
        };
        public static List<string> RelativeFactorsColumns = new List<string>()
        {
            "RelDistanceFactorNormalized",
            "RelStartFactorNormalized",
            "RelTrackConditionFactorNormalized",
        };
        public static List<string> StartStatsColumns = new List<string>()
        {
            "WinFromStartPos",
           // "NumStartingHorses",
           // "NumHorsesInEqualOrBetterStartPosition",
        };

        #endregion
        #region Full sets?
        public static List<string> RelativeColumns = new List<string>(
            RelativeBestSpeedColumns
            .Union(RelativeMedianSpeedColumns)
            .Union(RelativeAverageSpeedColumns)
            .Union(RelativeDriverStatsTotalColumns)
            .Union(RelativeDriverStatsStatTypeColumns)
            .Union(RelativeFactorsColumns)
            .Union(StartStatsColumns));

        public static List<string> BestColumns = new List<string>()
        {
            "WinFromStartPos",
            "RelBestOnDistanceAndStartTypeNormalized",
            "RelDriverWinPercentStartType",
            "RelDriverTop3PercentStartType",

        };
        #endregion
        public enum ProfileEnum
        {
            KmTime,
            TimeAfterWinnerLastCapped,
            TimeAfterWinnerPlacedCapped,
        }
        public enum CompareAgainstEnum
        {
            Median,
            Average
        }
        #region Static helpers
        public static TrackConditionEnum ParseTrackCondition(string c)
        {
            if (c == "Lätt bana")
                return TrackConditionEnum.Light;
            else if (c == "Något tung bana")
                return TrackConditionEnum.Heavier;
            else if (c == "Tung bana")
                return TrackConditionEnum.Heavy;
            else if (c == "Vinterbana")
                return TrackConditionEnum.Winter;
            throw new NotImplementedException();
        }
        public static float Normalize(float d, IEnumerable<float> all)
        {
            if (!all.Any())
            {
                return 0.5f;
            }
            var min = all.Min();
            var diff = all.Max() - min;
            return (d - min) / diff;
        }
        private static float GetBest(IEnumerable<long?> list)
        {
            var valid = list.Where(f => f.HasValue);
            if (valid.Any())
                return (float)valid.Select(f => f.Value).Min();
            return float.NaN;
        }
        private static float GetBest(IEnumerable<float> list)
        {
            if (list.Any())
                return (float)list.Max(); // Use max?
            return float.NaN;
        }
        private static float GetMin(IEnumerable<float> list)
        {
            if (list.Any())
                return (float)list.Min(); // Use max?
            return float.NaN;
        }
        private static float GetAverage(IEnumerable<long?> list)
        {
            var valid = list.Where(f => f.HasValue);
            if (valid.Any())
                return (float)valid.Select(f => f.Value).Average();
            return float.NaN;
        }
        private static float GetAverage(IEnumerable<float> list)
        {
            if (list.Any())
                return (float)list.Average();
            return float.NaN;
        }
        private static float GetMedian(IEnumerable<float> list)
        {
            if (list.Any())
                return HorseStats.GetMedian(list);
            return float.NaN;
        }

        private static float GetMedian(IEnumerable<long?> list)
        {
            var valid = list.Where(f => f.HasValue);
            if (valid.Any())
                return HorseStats.GetMedian(valid.Select(f => f.Value));
            return float.NaN;
        }
        private static float FloatOrNan(float? nullFloat)
        {
            if (nullFloat.HasValue)
                return nullFloat.Value;
            return float.NaN;
        }

        private static float GetNormalizedRelativeSpeed(long? speed, IEnumerable<long?> all, long def)
        {
            var valids = all.Where(l => l.HasValue).Select(l => l.Value);
            if (!valids.Any())
                return 0.5f;
            if (!speed.HasValue)
            {
                speed = def;
            }
            var min = valids.Min();
            var diff = valids.Max() - min;
            if (diff == 0)
                return def;

            return HorseStats.Normalize(speed.Value, min, diff);
        }
        #endregion
        public HorseTotalModel() { }
        public HorseTotalModel(Race race, HorseStats stats, IEnumerable<HorseStats> rest, ProfileEnum profileEnum, float winFromTrack, TrackConditionEnum trackCondition)
        {
            TimeAfterWinner = (float)stats.RaceResult.FinishTimeAfterWinner;
            if (TimeAfterWinner == 0 || TimeAfterWinner > race.LastFinishTime)
            {
                TimeAfterWinnerCappedLast = (float)(race.LastFinishTime - race.WinnerFinishTime);
            }

            WinFromStartPos = winFromTrack;
            
            NumStartingHorses = rest.Count();
            StartGroup = RaceResult.GetStartGroupSort(race.StartType, stats.RaceResult.PositionForDistance, stats.RaceResult.DistanceHandicap);

            NumHorsesInEqualOrBetterStartPosition = rest.Count(hs =>
                RaceResult.GetStartGroupSort(race.StartType, hs.RaceResult.PositionForDistance, hs.RaceResult.DistanceHandicap) <= StartGroup
                );
            int distBucket = HorseRaceResult.GetDistanceBucket(race.Distance);
            if (distBucket == 0)
            {
                NumStartsOnDistance = NumShort;
                if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    NumStartsOnType = NumAutos;
                    NumStartsOnDistanceAndType = NumShortAuto;
                }
                else if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    NumStartsOnType = NumVolts;
                    NumStartsOnDistanceAndType = NumShortVolt;
                }
            }
            else if (distBucket == 1)
            {
                NumStartsOnDistance = NumMedium;
                if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    NumStartsOnType = NumAutos;
                    NumStartsOnDistanceAndType = NumMediumAuto;
                }
                else if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    NumStartsOnDistanceAndType = NumMediumVolt;
                    NumStartsOnType = NumVolts;

                }
            }
            else if (distBucket == 2)
            {
                NumStartsOnDistance = NumLongVolt;
                if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    NumStartsOnDistanceAndType = NumLongAuto;
                    NumStartsOnType = NumAutos;
                }
                else if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    NumStartsOnType = NumVolts;
                    NumStartsOnDistanceAndType = NumLongVolt;
                }
            }
            NumShortAuto = stats.NumShortAuto;
            NumShortVolt = stats.NumShortVolt;
            NumMediumAuto = stats.NumMediumAuto;
            NumMediumVolt = stats.NumMediumVolt;
            NumLongAuto = stats.NumLongAuto;
            NumLongVolt = stats.NumLongVolt;
            NumShort = stats.NumShort;
            NumMedium = stats.NumMedium;
            NumLong = stats.NumLong;
            NumVolts = stats.NumVolts;
            NumAutos = stats.NumAutos;



            NumShortLastMonth = stats.NumShortLastMonth;
            NumMediumLastMonth = stats.NumMediumLastMonth;
            NumLongLastMonth = stats.NumLongLastMonth;

            NumLastMonth = stats.NumLastMonth;

            NumShape = stats.NumShape;

            NumLight = stats.NumLight;
            NumHeavier = stats.NumHeavier;
            NumHeavy = stats.NumHeavy;
            NumWinter = stats.NumWinter;
            NumTotals = stats.NumTotals;

            #region This starter stats
            // Best
            BestShortAuto = FloatOrNan(stats.BestShortAuto);
            BestShortVolt = FloatOrNan(stats.BestShortVolt);
    
            BestMediumAuto = FloatOrNan(stats.BestMediumAuto);
            BestMediumVolt = FloatOrNan(stats.BestMediumVolt);

            BestLongAuto = FloatOrNan(stats.BestLongAuto);
            BestLongVolt = FloatOrNan(stats.BestLongVolt);

            BestShortTime = FloatOrNan(stats.BestShortTime);
            BestMediumTime = FloatOrNan(stats.BestMediumTime);
            BestLongTime = FloatOrNan(stats.BestLongTime);

            BestAuto = FloatOrNan(stats.BestAuto);
            BestVolt = FloatOrNan(stats.BestVolt);
            Best = FloatOrNan(stats.Best);

            BestLastMonthShort = FloatOrNan(stats.BestAuto);
            BestLastMonthMedium = FloatOrNan(stats.BestLastMonthMedium);
            BestLastMonthLong = FloatOrNan(stats.BestLastMonthLong);
            BestLastMonth = FloatOrNan(stats.BestLastMonth);

            // Median
            MedianLongAuto = FloatOrNan(stats.MedianLongAuto);
            MedianLongVolt = FloatOrNan(stats.MedianLongVolt);
            MedianLongTime = FloatOrNan(stats.MedianLongTime);

            MedianMediumAuto = FloatOrNan(stats.MedianMediumAuto);
            MedianMediumVolt = FloatOrNan(stats.MedianMediumVolt);
            MedianMediumTime = FloatOrNan(stats.MedianMediumTime);

            MedianShortAuto = FloatOrNan(stats.MedianShortAuto);
            MedianShortVolt = FloatOrNan(stats.MedianShortVolt);
            MedianShortTime = FloatOrNan(stats.MedianShortTime);

            MedianLastMonthShort = FloatOrNan(stats.MedianLastMonthShort);
            MedianLastMonthMedium = FloatOrNan(stats.MedianLastMonthMedium);
            MedianLastMonthShort = FloatOrNan(stats.MedianLastMonthShort);
            MedianLastMonth = FloatOrNan(stats.MedianLastMonth);

            MedianAuto = FloatOrNan(stats.MedianAuto);
            MedianVolt = FloatOrNan(stats.MedianVolt);
            Median = FloatOrNan(stats.Median);

            // Average
            AverageLongAuto = FloatOrNan(stats.AverageLongAuto);
            AverageLongVolt = FloatOrNan(stats.AverageLongVolt);
            AverageLongTime = FloatOrNan(stats.AverageLongTime);

            AverageMediumAuto = FloatOrNan(stats.AverageMediumAuto);
            AverageMediumVolt = FloatOrNan(stats.AverageMediumVolt);
            AverageMediumTime = FloatOrNan(stats.AverageMediumTime);

            AverageShortAuto = FloatOrNan(stats.AverageShortAuto);
            AverageShortVolt = FloatOrNan(stats.AverageShortVolt);
            AverageShortTime = FloatOrNan(stats.AverageShortTime);

            AverageLastMonthShort = FloatOrNan(stats.AverageLastMonthShort);
            AverageLastMonthMedium = FloatOrNan(stats.AverageLastMonthMedium);
            AverageLastMonthShort = FloatOrNan(stats.AverageLastMonthShort);
            AverageLastMonth = FloatOrNan(stats.AverageLastMonth);

            AverageAuto = FloatOrNan(stats.AverageAuto);
            AverageVolt = FloatOrNan(stats.AverageVolt);
            Average = FloatOrNan(stats.Average);

            // Horse stats
            WinPercent = stats.WinPercent;
            Top3Percent = stats.Top3Percent;
            PlacePercent = stats.PlacePercent;
            MoneyPerStartShape = stats.MoneyPerStartShape;
            MoneyPerStart = stats.MoneyPerStart;
            WinShape = stats.WinShape;
            PlaceShape = stats.PlaceShape;
            Top3Shape = stats.Top3Shape;
            TimeAfterWinnerShapeAverage = stats.TimeAfterWinnerShapeAverage;
            TimeAfterWinnerShapeMedian = stats.TimeAfterWinnerShapeMedian;
            TimeAfterWinnerLast = stats.TimeAfterWinnerLast;
            TimeAfterWinnerMedian = stats.TimeAfterWinnerMedian;
            TimeAfterWinnerAverage = stats.TimeAfterWinnerAverage;

            // Driver stats
            NumDriverHistory = stats.NumDriverHistory;
            DriverWinPercent = stats.DriverWinPercent;
            DriverTop3Percent = stats.DriverTop3Percent;
            DriverPlacePercent = stats.DriverPlacePercent;
            DriverMoneyPerStart = stats.DriverMoneyPerStart;

            NumDriverHistoryAuto = stats.NumDriverHistoryAuto;
            DriverWinPercentAuto = stats.DriverWinPercentAuto;
            DriverTop3PercentAuto = stats.DriverTop3PercentAuto;
            DriverPlacePercentAuto = stats.DriverPlacePercentAuto;
            DriverMoneyPerStartAuto = stats.DriverMoneyPerStartAuto;

            NumDriverHistoryVolt = stats.NumDriverHistoryVolt;
            DriverWinPercentVolt = stats.DriverWinPercentVolt;
            DriverTop3PercentVolt = stats.DriverTop3PercentVolt;
            DriverPlacePercentVolt = stats.DriverPlacePercentVolt;
            DriverMoneyPerStartVolt = stats.DriverMoneyPerStartVolt;

            // Profile?
            var profile = stats.KmTimeValidProfile;
            if (profileEnum == ProfileEnum.TimeAfterWinnerLastCapped)
            {
                profile = stats.TimeAfterWinnerLastCapProfile;
            }
            else if (profileEnum == ProfileEnum.TimeAfterWinnerPlacedCapped)
            {
                profile = stats.TimeAfterWinnerPlaceCapProfile;
            }
            ShortDistanceFactor = profile.ShortDistanceFactor;
            MediumDistanceFactor = profile.MediumDistanceFactor;
            LongDistanceFactor = profile.LongDistanceFactor;

            AutoStartFactor = profile.AutoStartFactor;
            VoltStartFactor = profile.VoltStartFactor;

            LightTrackConditionFactor = profile.LightTrackConditionFactor;
            HeavierTrackConditionFactor = profile.HeavierTrackConditionFactor;
            HeavyTrackConditionFactor = profile.HeavyTrackConditionFactor;
            WinterTrackConditionFactor = profile.WinterTrackConditionFactor;

            #endregion

            #region Other median stats
            // Best
            MedianBestShortAuto = GetMedian(rest.Select(hs => hs.BestShortAuto));
            MedianBestShortVolt = GetMedian(rest.Select(hs => hs.BestShortVolt));

            MedianBestMediumAuto = GetMedian(rest.Select(hs => hs.BestMediumAuto));
            MedianBestMediumVolt = GetMedian(rest.Select(hs => hs.BestMediumVolt));

            MedianBestLongAuto = GetMedian(rest.Select(hs => hs.BestLongAuto));
            MedianBestLongVolt = GetMedian(rest.Select(hs => hs.BestLongVolt));

            MedianBestShortTime = GetMedian(rest.Select(hs => hs.BestShortTime));
            MedianBestMediumTime = GetMedian(rest.Select(hs => hs.BestMediumTime));
            MedianBestLongTime = GetMedian(rest.Select(hs => hs.BestLongTime));

            MedianBestAuto = GetMedian(rest.Select(hs => hs.BestAuto));
            MedianBestVolt = GetMedian(rest.Select(hs => hs.BestVolt));
            MedianBest = GetMedian(rest.Select(hs => hs.Best));

            MedianBestLastMonthShort = GetMedian(rest.Select(hs => hs.BestAuto));
            MedianBestLastMonthMedium = GetMedian(rest.Select(hs => hs.BestLastMonthMedium));
            MedianBestLastMonthLong = GetMedian(rest.Select(hs => hs.BestLastMonthLong));
            MedianBestLastMonth = GetMedian(rest.Select(hs => hs.BestLastMonth));

            // Median
            MedianMedianLongAuto = GetMedian(rest.Select(hs => hs.MedianLongAuto));
            MedianMedianLongVolt = GetMedian(rest.Select(hs => hs.MedianLongVolt));
            MedianMedianLongTime = GetMedian(rest.Select(hs => hs.MedianLongTime));

            MedianMedianMediumAuto = GetMedian(rest.Select(hs => hs.MedianMediumAuto));
            MedianMedianMediumVolt = GetMedian(rest.Select(hs => hs.MedianMediumVolt));
            MedianMedianMediumTime = GetMedian(rest.Select(hs => hs.MedianMediumTime));

            MedianMedianShortAuto = GetMedian(rest.Select(hs => hs.MedianShortAuto));
            MedianMedianShortVolt = GetMedian(rest.Select(hs => hs.MedianShortVolt));
            MedianMedianShortTime = GetMedian(rest.Select(hs => hs.MedianShortTime));

            MedianMedianLastMonthShort = GetMedian(rest.Select(hs => hs.MedianLastMonthShort));
            MedianMedianLastMonthMedium = GetMedian(rest.Select(hs => hs.MedianLastMonthMedium));
            MedianMedianLastMonthShort = GetMedian(rest.Select(hs => hs.MedianLastMonthShort));
            MedianMedianLastMonth = GetMedian(rest.Select(hs => hs.MedianLastMonth));

            MedianMedianAuto = GetMedian(rest.Select(hs => hs.MedianAuto));
            MedianMedianVolt = GetMedian(rest.Select(hs => hs.MedianVolt));
            MedianMedian = GetMedian(rest.Select(hs => hs.Median));

            // Average
            MedianAverageLongAuto = GetMedian(rest.Select(hs => hs.AverageLongAuto));
            MedianAverageLongVolt = GetMedian(rest.Select(hs => hs.AverageLongVolt));
            MedianAverageLongTime = GetMedian(rest.Select(hs => hs.AverageLongTime));

            MedianAverageMediumAuto = GetMedian(rest.Select(hs => hs.AverageMediumAuto));
            MedianAverageMediumVolt = GetMedian(rest.Select(hs => hs.AverageMediumVolt));
            MedianAverageMediumTime = GetMedian(rest.Select(hs => hs.AverageMediumTime));

            MedianAverageShortAuto = GetMedian(rest.Select(hs => hs.AverageShortAuto));
            MedianAverageShortVolt = GetMedian(rest.Select(hs => hs.AverageShortVolt));
            MedianAverageShortTime = GetMedian(rest.Select(hs => hs.AverageShortTime));

            MedianAverageLastMonthShort = GetMedian(rest.Select(hs => hs.AverageLastMonthShort));
            MedianAverageLastMonthMedium = GetMedian(rest.Select(hs => hs.AverageLastMonthMedium));
            MedianAverageLastMonthShort = GetMedian(rest.Select(hs => hs.AverageLastMonthShort));
            MedianAverageLastMonth = GetMedian(rest.Select(hs => hs.AverageLastMonth));

            MedianAverageAuto = GetMedian(rest.Select(hs => hs.AverageAuto));
            MedianAverageVolt = GetMedian(rest.Select(hs => hs.AverageVolt));
            MedianAverage = GetMedian(rest.Select(hs => hs.Average));

            // Horse stats
            MedianWinPercent = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.WinPercent));
            MedianTop3Percent = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.Top3Percent));
            MedianPlacePercent = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.PlacePercent));
            MedianMoneyPerStart = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.MoneyPerStart));

            MedianMoneyPerStartShape = GetMedian(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.MoneyPerStartShape));
            MedianWinShape = GetMedian(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.WinShape));
            MedianPlaceShape = GetMedian(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.PlaceShape));
            MedianTop3Shape = GetMedian(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.Top3Shape));
            MedianTimeAfterWinnerShapeAverage = GetMedian(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeAverage));
            MedianTimeAfterWinnerShapeMedian = GetMedian(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeMedian));

            MedianTimeAfterWinnerLast = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerLast));
            MedianTimeAfterWinnerMedian = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerMedian));
            MedianTimeAfterWinnerAverage = GetMedian(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerAverage));

            // Driver stats

            MedianDriverWinPercent = GetMedian(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverWinPercent));
            MedianDriverTop3Percent = GetMedian(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverTop3Percent));
            MedianDriverPlacePercent = GetMedian(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverPlacePercent));
            MedianDriverMoneyPerStart = GetMedian(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverMoneyPerStart));

            MedianDriverWinPercentAuto = GetMedian(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverWinPercentAuto));
            MedianDriverTop3PercentAuto = GetMedian(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverTop3PercentAuto));
            MedianDriverPlacePercentAuto = GetMedian(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverPlacePercentAuto));
            MedianDriverMoneyPerStartAuto = GetMedian(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverMoneyPerStartAuto));

            MedianDriverWinPercentVolt = GetMedian(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverWinPercentVolt));
            MedianDriverTop3PercentVolt = GetMedian(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverTop3PercentVolt));
            MedianDriverPlacePercentVolt = GetMedian(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverPlacePercentVolt));
            MedianDriverMoneyPerStartVolt = GetMedian(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverMoneyPerStartVolt));

            // Profile?
            Func<HorseStats, HorseStatsProfile> func = (hs) => hs.KmTimeValidProfile;
            if (profileEnum == ProfileEnum.TimeAfterWinnerLastCapped)
            {
                func = (hs) => hs.TimeAfterWinnerLastCapProfile;
            }
            else if (profileEnum == ProfileEnum.TimeAfterWinnerPlacedCapped)
            {
                func = (hs) => hs.TimeAfterWinnerPlaceCapProfile;
            }
            MedianShortDistanceFactor = GetMedian(rest.Where(hs => hs.NumShort > 0).Select(hs => func(hs).ShortDistanceFactor));
            MedianMediumDistanceFactor = GetMedian(rest.Where(hs => hs.NumMedium > 0).Select(hs => func(hs).MediumDistanceFactor));
            MedianLongDistanceFactor = GetMedian(rest.Where(hs => hs.NumLong > 0).Select(hs => func(hs).LongDistanceFactor));

            MedianAutoStartFactor = GetMedian(rest.Where(hs => hs.NumAutos > 0).Select(hs => func(hs).AutoStartFactor));
            MedianVoltStartFactor = GetMedian(rest.Where(hs => hs.NumVolts > 0).Select(hs => func(hs).VoltStartFactor));

            MedianLightTrackConditionFactor = GetMedian(rest.Where(hs => hs.NumLight > 0).Select(hs => func(hs).LightTrackConditionFactor));
            MedianHeavierTrackConditionFactor = GetMedian(rest.Where(hs => hs.NumHeavier > 0).Select(hs => func(hs).HeavierTrackConditionFactor));
            MedianHeavyTrackConditionFactor = GetMedian(rest.Where(hs => hs.NumHeavy > 0).Select(hs => func(hs).HeavyTrackConditionFactor));
            MedianWinterTrackConditionFactor = GetMedian(rest.Where(hs => hs.NumWinter > 0).Select(hs => func(hs).WinterTrackConditionFactor));

            #endregion

            #region Other average
            // Best
            AverageBestShortAuto = GetAverage(rest.Select(hs => hs.BestShortAuto));
            AverageBestShortVolt = GetAverage(rest.Select(hs => hs.BestShortVolt));

            AverageBestMediumAuto = GetAverage(rest.Select(hs => hs.BestMediumAuto));
            AverageBestMediumVolt = GetAverage(rest.Select(hs => hs.BestMediumVolt));

            AverageBestLongAuto = GetAverage(rest.Select(hs => hs.BestLongAuto));
            AverageBestLongVolt = GetAverage(rest.Select(hs => hs.BestLongVolt));

            AverageBestShortTime = GetAverage(rest.Select(hs => hs.BestShortTime));
            AverageBestMediumTime = GetAverage(rest.Select(hs => hs.BestMediumTime));
            AverageBestLongTime = GetAverage(rest.Select(hs => hs.BestLongTime));

            AverageBestAuto = GetAverage(rest.Select(hs => hs.BestAuto));
            AverageBestVolt = GetAverage(rest.Select(hs => hs.BestVolt));
            AverageBest = GetAverage(rest.Select(hs => hs.Best));

            AverageBestLastMonthShort = GetAverage(rest.Select(hs => hs.BestAuto));
            AverageBestLastMonthMedium = GetAverage(rest.Select(hs => hs.BestLastMonthMedium));
            AverageBestLastMonthLong = GetAverage(rest.Select(hs => hs.BestLastMonthLong));
            AverageBestLastMonth = GetAverage(rest.Select(hs => hs.BestLastMonth));

            // Median
            AverageMedianLongAuto = GetAverage(rest.Select(hs => hs.MedianLongAuto));
            AverageMedianLongVolt = GetAverage(rest.Select(hs => hs.MedianLongVolt));
            AverageMedianLongTime = GetAverage(rest.Select(hs => hs.MedianLongTime));

            AverageMedianMediumAuto = GetAverage(rest.Select(hs => hs.MedianMediumAuto));
            AverageMedianMediumVolt = GetAverage(rest.Select(hs => hs.MedianMediumVolt));
            AverageMedianMediumTime = GetAverage(rest.Select(hs => hs.MedianMediumTime));

            AverageMedianShortAuto = GetAverage(rest.Select(hs => hs.MedianShortAuto));
            AverageMedianShortVolt = GetAverage(rest.Select(hs => hs.MedianShortVolt));
            AverageMedianShortTime = GetAverage(rest.Select(hs => hs.MedianShortTime));

            AverageMedianLastMonthShort = GetAverage(rest.Select(hs => hs.MedianLastMonthShort));
            AverageMedianLastMonthMedium = GetAverage(rest.Select(hs => hs.MedianLastMonthMedium));
            AverageMedianLastMonthShort = GetAverage(rest.Select(hs => hs.MedianLastMonthShort));
            AverageMedianLastMonth = GetAverage(rest.Select(hs => hs.MedianLastMonth));

            AverageMedianAuto = GetAverage(rest.Select(hs => hs.MedianAuto));
            AverageMedianVolt = GetAverage(rest.Select(hs => hs.MedianVolt));
            AverageMedian = GetAverage(rest.Select(hs => hs.Median));

            // Average
            AverageAverageLongAuto = GetAverage(rest.Select(hs => hs.AverageLongAuto));
            AverageAverageLongVolt = GetAverage(rest.Select(hs => hs.AverageLongVolt));
            AverageAverageLongTime = GetAverage(rest.Select(hs => hs.AverageLongTime));

            AverageAverageMediumAuto = GetAverage(rest.Select(hs => hs.AverageMediumAuto));
            AverageAverageMediumVolt = GetAverage(rest.Select(hs => hs.AverageMediumVolt));
            AverageAverageMediumTime = GetAverage(rest.Select(hs => hs.AverageMediumTime));

            AverageAverageShortAuto = GetAverage(rest.Select(hs => hs.AverageShortAuto));
            AverageAverageShortVolt = GetAverage(rest.Select(hs => hs.AverageShortVolt));
            AverageAverageShortTime = GetAverage(rest.Select(hs => hs.AverageShortTime));

            AverageAverageLastMonthShort = GetAverage(rest.Select(hs => hs.AverageLastMonthShort));
            AverageAverageLastMonthMedium = GetAverage(rest.Select(hs => hs.AverageLastMonthMedium));
            AverageAverageLastMonthShort = GetAverage(rest.Select(hs => hs.AverageLastMonthShort));
            AverageAverageLastMonth = GetAverage(rest.Select(hs => hs.AverageLastMonth));

            AverageAverageAuto = GetAverage(rest.Select(hs => hs.AverageAuto));
            AverageAverageVolt = GetAverage(rest.Select(hs => hs.AverageVolt));
            AverageAverage = GetAverage(rest.Select(hs => hs.Average));

            // Horse stats
            AverageWinPercent = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.WinPercent));
            AverageTop3Percent = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.Top3Percent));
            AveragePlacePercent = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.PlacePercent));
            AverageMoneyPerStart = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.MoneyPerStart));

            AverageMoneyPerStartShape = GetAverage(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.MoneyPerStartShape));
            AverageWinShape = GetAverage(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.WinShape));
            AveragePlaceShape = GetAverage(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.PlaceShape));
            AverageTop3Shape = GetAverage(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.Top3Shape));
            AverageTimeAfterWinnerShapeAverage = GetAverage(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeAverage));
            AverageTimeAfterWinnerShapeMedian = GetAverage(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeMedian));

            AverageTimeAfterWinnerLast = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerLast));
            AverageTimeAfterWinnerMedian = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerMedian));
            AverageTimeAfterWinnerAverage = GetAverage(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerAverage));

            // Driver stats

            AverageDriverWinPercent = GetAverage(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverWinPercent));
            AverageDriverTop3Percent = GetAverage(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverTop3Percent));
            AverageDriverPlacePercent = GetAverage(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverPlacePercent));
            AverageDriverMoneyPerStart = GetAverage(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverMoneyPerStart));

            AverageDriverWinPercentAuto = GetAverage(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverWinPercentAuto));
            AverageDriverTop3PercentAuto = GetAverage(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverTop3PercentAuto));
            AverageDriverPlacePercentAuto = GetAverage(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverPlacePercentAuto));
            AverageDriverMoneyPerStartAuto = GetAverage(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverMoneyPerStartAuto));

            AverageDriverWinPercentVolt = GetAverage(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverWinPercentVolt));
            AverageDriverTop3PercentVolt = GetAverage(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverTop3PercentVolt));
            AverageDriverPlacePercentVolt = GetAverage(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverPlacePercentVolt));
            AverageDriverMoneyPerStartVolt = GetAverage(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverMoneyPerStartVolt));

            // Profile?
            func = (hs) => hs.KmTimeValidProfile;
            if (profileEnum == ProfileEnum.TimeAfterWinnerLastCapped)
            {
                func = (hs) => hs.TimeAfterWinnerLastCapProfile;
            }
            else if (profileEnum == ProfileEnum.TimeAfterWinnerPlacedCapped)
            {
                func = (hs) => hs.TimeAfterWinnerPlaceCapProfile;
            }
            AverageShortDistanceFactor = GetAverage(rest.Where(hs => hs.NumShort > 0).Select(hs => func(hs).ShortDistanceFactor));
            AverageMediumDistanceFactor = GetAverage(rest.Where(hs => hs.NumMedium > 0).Select(hs => func(hs).MediumDistanceFactor));
            AverageLongDistanceFactor = GetAverage(rest.Where(hs => hs.NumLong > 0).Select(hs => func(hs).LongDistanceFactor));

            AverageAutoStartFactor = GetAverage(rest.Where(hs => hs.NumAutos > 0).Select(hs => func(hs).AutoStartFactor));
            AverageVoltStartFactor = GetAverage(rest.Where(hs => hs.NumVolts > 0).Select(hs => func(hs).VoltStartFactor));

            AverageLightTrackConditionFactor = GetAverage(rest.Where(hs => hs.NumLight > 0).Select(hs => func(hs).LightTrackConditionFactor));
            AverageHeavierTrackConditionFactor = GetAverage(rest.Where(hs => hs.NumHeavier > 0).Select(hs => func(hs).HeavierTrackConditionFactor));
            AverageHeavyTrackConditionFactor = GetAverage(rest.Where(hs => hs.NumHeavy > 0).Select(hs => func(hs).HeavyTrackConditionFactor));
            AverageWinterTrackConditionFactor = GetAverage(rest.Where(hs => hs.NumWinter > 0).Select(hs => func(hs).WinterTrackConditionFactor));

            #endregion

            #region Other best
            // Best
            BestBestShortAuto = GetBest(rest.Select(hs => hs.BestShortAuto));
            BestBestShortVolt = GetBest(rest.Select(hs => hs.BestShortVolt));

            BestBestMediumAuto = GetBest(rest.Select(hs => hs.BestMediumAuto));
            BestBestMediumVolt = GetBest(rest.Select(hs => hs.BestMediumVolt));

            BestBestLongAuto = GetBest(rest.Select(hs => hs.BestLongAuto));
            BestBestLongVolt = GetBest(rest.Select(hs => hs.BestLongVolt));

            BestBestShortTime = GetBest(rest.Select(hs => hs.BestShortTime));
            BestBestMediumTime = GetBest(rest.Select(hs => hs.BestMediumTime));
            BestBestLongTime = GetBest(rest.Select(hs => hs.BestLongTime));

            BestBestAuto = GetBest(rest.Select(hs => hs.BestAuto));
            BestBestVolt = GetBest(rest.Select(hs => hs.BestVolt));
            BestBest = GetBest(rest.Select(hs => hs.Best));

            BestBestLastMonthShort = GetBest(rest.Select(hs => hs.BestAuto));
            BestBestLastMonthMedium = GetBest(rest.Select(hs => hs.BestLastMonthMedium));
            BestBestLastMonthLong = GetBest(rest.Select(hs => hs.BestLastMonthLong));
            BestBestLastMonth = GetBest(rest.Select(hs => hs.BestLastMonth));

            // Median
            BestMedianLongAuto = GetBest(rest.Select(hs => hs.MedianLongAuto));
            BestMedianLongVolt = GetBest(rest.Select(hs => hs.MedianLongVolt));
            BestMedianLongTime = GetBest(rest.Select(hs => hs.MedianLongTime));

            BestMedianMediumAuto = GetBest(rest.Select(hs => hs.MedianMediumAuto));
            BestMedianMediumVolt = GetBest(rest.Select(hs => hs.MedianMediumVolt));
            BestMedianMediumTime = GetBest(rest.Select(hs => hs.MedianMediumTime));

            BestMedianShortAuto = GetBest(rest.Select(hs => hs.MedianShortAuto));
            BestMedianShortVolt = GetBest(rest.Select(hs => hs.MedianShortVolt));
            BestMedianShortTime = GetBest(rest.Select(hs => hs.MedianShortTime));

            BestMedianLastMonthShort = GetBest(rest.Select(hs => hs.MedianLastMonthShort));
            BestMedianLastMonthMedium = GetBest(rest.Select(hs => hs.MedianLastMonthMedium));
            BestMedianLastMonthShort = GetBest(rest.Select(hs => hs.MedianLastMonthShort));
            BestMedianLastMonth = GetBest(rest.Select(hs => hs.MedianLastMonth));

            BestMedianAuto = GetBest(rest.Select(hs => hs.MedianAuto));
            BestMedianVolt = GetBest(rest.Select(hs => hs.MedianVolt));
            BestMedian = GetBest(rest.Select(hs => hs.Median));

            // Average
            BestAverageLongAuto = GetBest(rest.Select(hs => hs.AverageLongAuto));
            BestAverageLongVolt = GetBest(rest.Select(hs => hs.AverageLongVolt));
            BestAverageLongTime = GetBest(rest.Select(hs => hs.AverageLongTime));

            BestAverageMediumAuto = GetBest(rest.Select(hs => hs.AverageMediumAuto));
            BestAverageMediumVolt = GetBest(rest.Select(hs => hs.AverageMediumVolt));
            BestAverageMediumTime = GetBest(rest.Select(hs => hs.AverageMediumTime));

            BestAverageShortAuto = GetBest(rest.Select(hs => hs.AverageShortAuto));
            BestAverageShortVolt = GetBest(rest.Select(hs => hs.AverageShortVolt));
            BestAverageShortTime = GetBest(rest.Select(hs => hs.AverageShortTime));

            BestAverageLastMonthShort = GetBest(rest.Select(hs => hs.AverageLastMonthShort));
            BestAverageLastMonthMedium = GetBest(rest.Select(hs => hs.AverageLastMonthMedium));
            BestAverageLastMonthShort = GetBest(rest.Select(hs => hs.AverageLastMonthShort));
            BestAverageLastMonth = GetBest(rest.Select(hs => hs.AverageLastMonth));

            BestAverageAuto = GetBest(rest.Select(hs => hs.AverageAuto));
            BestAverageVolt = GetBest(rest.Select(hs => hs.AverageVolt));
            BestAverage = GetBest(rest.Select(hs => hs.Average));

            // Horse stats
            BestWinPercent = GetBest(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.WinPercent));
            BestTop3Percent = GetBest(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.Top3Percent));
            BestPlacePercent = GetBest(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.PlacePercent));
            BestMoneyPerStart = GetBest(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.MoneyPerStart));

            BestMoneyPerStartShape = GetBest(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.MoneyPerStartShape));
            BestWinShape = GetBest(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.WinShape));
            BestPlaceShape = GetBest(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.PlaceShape));
            BestTop3Shape = GetBest(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.Top3Shape));
            BestTimeAfterWinnerShapeAverage = GetMin(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeAverage));
            BestTimeAfterWinnerShapeMedian = GetMin(rest.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeMedian));

            BestTimeAfterWinnerLast = GetMin(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerLast));
            BestTimeAfterWinnerMedian = GetMin(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerMedian));
            BestTimeAfterWinnerAverage = GetMin(rest.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerAverage));

            // Driver stats

            BestDriverWinPercent = GetBest(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverWinPercent));
            BestDriverTop3Percent = GetBest(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverTop3Percent));
            BestDriverPlacePercent = GetBest(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverPlacePercent));
            BestDriverMoneyPerStart = GetBest(rest.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverMoneyPerStart));

            BestDriverWinPercentAuto = GetBest(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverWinPercentAuto));
            BestDriverTop3PercentAuto = GetBest(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverTop3PercentAuto));
            BestDriverPlacePercentAuto = GetBest(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverPlacePercentAuto));
            BestDriverMoneyPerStartAuto = GetBest(rest.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverMoneyPerStartAuto));

            BestDriverWinPercentVolt = GetBest(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverWinPercentVolt));
            BestDriverTop3PercentVolt = GetBest(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverTop3PercentVolt));
            BestDriverPlacePercentVolt = GetBest(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverPlacePercentVolt));
            BestDriverMoneyPerStartVolt = GetBest(rest.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverMoneyPerStartVolt));

            // Profile?
            func = (hs) => hs.KmTimeValidProfile;
            if (profileEnum == ProfileEnum.TimeAfterWinnerLastCapped)
            {
                func = (hs) => hs.TimeAfterWinnerLastCapProfile;
            }
            else if (profileEnum == ProfileEnum.TimeAfterWinnerPlacedCapped)
            {
                func = (hs) => hs.TimeAfterWinnerPlaceCapProfile;
            }
            BestShortDistanceFactor = GetBest(rest.Where(hs => hs.NumShort > 0).Select(hs => func(hs).ShortDistanceFactor));
            BestMediumDistanceFactor = GetBest(rest.Where(hs => hs.NumMedium > 0).Select(hs => func(hs).MediumDistanceFactor));
            BestLongDistanceFactor = GetBest(rest.Where(hs => hs.NumLong > 0).Select(hs => func(hs).LongDistanceFactor));

            BestAutoStartFactor = GetBest(rest.Where(hs => hs.NumAutos > 0).Select(hs => func(hs).AutoStartFactor));
            BestVoltStartFactor = GetBest(rest.Where(hs => hs.NumVolts > 0).Select(hs => func(hs).VoltStartFactor));

            BestLightTrackConditionFactor = GetBest(rest.Where(hs => hs.NumLight > 0).Select(hs => func(hs).LightTrackConditionFactor));
            BestHeavierTrackConditionFactor = GetBest(rest.Where(hs => hs.NumHeavier > 0).Select(hs => func(hs).HeavierTrackConditionFactor));
            BestHeavyTrackConditionFactor = GetBest(rest.Where(hs => hs.NumHeavy > 0).Select(hs => func(hs).HeavyTrackConditionFactor));
            BestWinterTrackConditionFactor = GetBest(rest.Where(hs => hs.NumWinter > 0).Select(hs => func(hs).WinterTrackConditionFactor));


            #endregion

            #region Best in better or equal position
            int startGroup = RaceResult.GetStartGroupSort(race.StartType, stats.RaceResult.PositionForDistance, stats.RaceResult.DistanceHandicap);
            var inSameOrEqualPos = rest.Where(hs => RaceResult.GetStartGroupSort(race.StartType, hs.RaceResult.PositionForDistance, hs.RaceResult.DistanceHandicap) <= startGroup).ToList();

            
            // Best // Best
           BestInfrontBestShortAuto = GetBest(inSameOrEqualPos.Select(hs => hs.BestShortAuto));
           BestInfrontBestShortVolt = GetBest(inSameOrEqualPos.Select(hs => hs.BestShortVolt));

           BestInfrontBestMediumAuto = GetBest(inSameOrEqualPos.Select(hs => hs.BestMediumAuto));
           BestInfrontBestMediumVolt = GetBest(inSameOrEqualPos.Select(hs => hs.BestMediumVolt));

           BestInfrontBestLongAuto = GetBest(inSameOrEqualPos.Select(hs => hs.BestLongAuto));
           BestInfrontBestLongVolt = GetBest(inSameOrEqualPos.Select(hs => hs.BestLongVolt));

           BestInfrontBestShortTime = GetBest(inSameOrEqualPos.Select(hs => hs.BestShortTime));
           BestInfrontBestMediumTime = GetBest(inSameOrEqualPos.Select(hs => hs.BestMediumTime));
           BestInfrontBestLongTime = GetBest(inSameOrEqualPos.Select(hs => hs.BestLongTime));

           BestInfrontBestAuto = GetBest(inSameOrEqualPos.Select(hs => hs.BestAuto));
           BestInfrontBestVolt = GetBest(inSameOrEqualPos.Select(hs => hs.BestVolt));
           BestInfrontBest = GetBest(inSameOrEqualPos.Select(hs => hs.Best));

           BestInfrontBestLastMonthShort = GetBest(inSameOrEqualPos.Select(hs => hs.BestAuto));
           BestInfrontBestLastMonthMedium = GetBest(inSameOrEqualPos.Select(hs => hs.BestLastMonthMedium));
           BestInfrontBestLastMonthLong = GetBest(inSameOrEqualPos.Select(hs => hs.BestLastMonthLong));
           BestInfrontBestLastMonth = GetBest(inSameOrEqualPos.Select(hs => hs.BestLastMonth));

            // Median
           BestInfrontMedianLongAuto = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLongAuto));
           BestInfrontMedianLongVolt = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLongVolt));
           BestInfrontMedianLongTime = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLongTime));

           BestInfrontMedianMediumAuto = GetBest(inSameOrEqualPos.Select(hs => hs.MedianMediumAuto));
           BestInfrontMedianMediumVolt = GetBest(inSameOrEqualPos.Select(hs => hs.MedianMediumVolt));
           BestInfrontMedianMediumTime = GetBest(inSameOrEqualPos.Select(hs => hs.MedianMediumTime));

           BestInfrontMedianShortAuto = GetBest(inSameOrEqualPos.Select(hs => hs.MedianShortAuto));
           BestInfrontMedianShortVolt = GetBest(inSameOrEqualPos.Select(hs => hs.MedianShortVolt));
           BestInfrontMedianShortTime = GetBest(inSameOrEqualPos.Select(hs => hs.MedianShortTime));

           BestInfrontMedianLastMonthShort = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLastMonthShort));
           BestInfrontMedianLastMonthMedium = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLastMonthMedium));
           BestInfrontMedianLastMonthShort = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLastMonthShort));
           BestInfrontMedianLastMonth = GetBest(inSameOrEqualPos.Select(hs => hs.MedianLastMonth));

           BestInfrontMedianAuto = GetBest(inSameOrEqualPos.Select(hs => hs.MedianAuto));
           BestInfrontMedianVolt = GetBest(inSameOrEqualPos.Select(hs => hs.MedianVolt));
           BestInfrontMedian = GetBest(inSameOrEqualPos.Select(hs => hs.Median));

            // Average
           BestInfrontAverageLongAuto = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLongAuto));
           BestInfrontAverageLongVolt = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLongVolt));
           BestInfrontAverageLongTime = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLongTime));

           BestInfrontAverageMediumAuto = GetBest(inSameOrEqualPos.Select(hs => hs.AverageMediumAuto));
           BestInfrontAverageMediumVolt = GetBest(inSameOrEqualPos.Select(hs => hs.AverageMediumVolt));
           BestInfrontAverageMediumTime = GetBest(inSameOrEqualPos.Select(hs => hs.AverageMediumTime));

           BestInfrontAverageShortAuto = GetBest(inSameOrEqualPos.Select(hs => hs.AverageShortAuto));
           BestInfrontAverageShortVolt = GetBest(inSameOrEqualPos.Select(hs => hs.AverageShortVolt));
           BestInfrontAverageShortTime = GetBest(inSameOrEqualPos.Select(hs => hs.AverageShortTime));

           BestInfrontAverageLastMonthShort = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLastMonthShort));
           BestInfrontAverageLastMonthMedium = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLastMonthMedium));
           BestInfrontAverageLastMonthShort = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLastMonthShort));
           BestInfrontAverageLastMonth = GetBest(inSameOrEqualPos.Select(hs => hs.AverageLastMonth));

           BestInfrontAverageAuto = GetBest(inSameOrEqualPos.Select(hs => hs.AverageAuto));
           BestInfrontAverageVolt = GetBest(inSameOrEqualPos.Select(hs => hs.AverageVolt));
           BestInfrontAverage = GetBest(inSameOrEqualPos.Select(hs => hs.Average));

            // Horse stats
           BestInfrontWinPercent = GetBest(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.WinPercent));
           BestInfrontTop3Percent = GetBest(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.Top3Percent));
           BestInfrontPlacePercent = GetBest(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.PlacePercent));
           BestInfrontMoneyPerStart = GetBest(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.MoneyPerStart));

           BestInfrontMoneyPerStartShape = GetBest(inSameOrEqualPos.Where(hs => hs.NumShape > 0).Select(hs => hs.MoneyPerStartShape));
           BestInfrontWinShape = GetBest(inSameOrEqualPos.Where(hs => hs.NumShape > 0).Select(hs => hs.WinShape));
           BestInfrontPlaceShape = GetBest(inSameOrEqualPos.Where(hs => hs.NumShape > 0).Select(hs => hs.PlaceShape));
           BestInfrontTop3Shape = GetBest(inSameOrEqualPos.Where(hs => hs.NumShape > 0).Select(hs => hs.Top3Shape));
           BestInfrontTimeAfterWinnerShapeAverage = GetMin(inSameOrEqualPos.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeAverage));
           BestInfrontTimeAfterWinnerShapeMedian = GetMin(inSameOrEqualPos.Where(hs => hs.NumShape > 0).Select(hs => hs.TimeAfterWinnerShapeMedian));

           BestInfrontTimeAfterWinnerLast = GetMin(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerLast));
           BestInfrontTimeAfterWinnerMedian = GetMin(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerMedian));
           BestInfrontTimeAfterWinnerAverage = GetMin(inSameOrEqualPos.Where(hs => hs.NumTotals > 0).Select(hs => hs.TimeAfterWinnerAverage));

            // Driver stats

           BestInfrontDriverWinPercent = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverWinPercent));
           BestInfrontDriverTop3Percent = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverTop3Percent));
           BestInfrontDriverPlacePercent = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverPlacePercent));
           BestInfrontDriverMoneyPerStart = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistory > 0).Select(hs => hs.DriverMoneyPerStart));

           BestInfrontDriverWinPercentAuto = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverWinPercentAuto));
           BestInfrontDriverTop3PercentAuto = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverTop3PercentAuto));
           BestInfrontDriverPlacePercentAuto = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverPlacePercentAuto));
           BestInfrontDriverMoneyPerStartAuto = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryAuto > 0).Select(hs => hs.DriverMoneyPerStartAuto));

           BestInfrontDriverWinPercentVolt = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverWinPercentVolt));
           BestInfrontDriverTop3PercentVolt = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverTop3PercentVolt));
           BestInfrontDriverPlacePercentVolt = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverPlacePercentVolt));
           BestInfrontDriverMoneyPerStartVolt = GetBest(inSameOrEqualPos.Where(hs => hs.NumDriverHistoryVolt > 0).Select(hs => hs.DriverMoneyPerStartVolt));

            // Profile?
            func = (hs) => hs.KmTimeValidProfile;
            if (profileEnum == ProfileEnum.TimeAfterWinnerLastCapped)
            {
                func = (hs) => hs.TimeAfterWinnerLastCapProfile;
            }
            else if (profileEnum == ProfileEnum.TimeAfterWinnerPlacedCapped)
            {
                func = (hs) => hs.TimeAfterWinnerPlaceCapProfile;
            }
           BestInfrontShortDistanceFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumShort > 0).Select(hs => func(hs).ShortDistanceFactor));
           BestInfrontMediumDistanceFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumMedium > 0).Select(hs => func(hs).MediumDistanceFactor));
           BestInfrontLongDistanceFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumLong > 0).Select(hs => func(hs).LongDistanceFactor));

           BestInfrontAutoStartFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumAutos > 0).Select(hs => func(hs).AutoStartFactor));
           BestInfrontVoltStartFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumVolts > 0).Select(hs => func(hs).VoltStartFactor));

           BestInfrontLightTrackConditionFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumLight > 0).Select(hs => func(hs).LightTrackConditionFactor));
           BestInfrontHeavierTrackConditionFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumHeavier > 0).Select(hs => func(hs).HeavierTrackConditionFactor));
           BestInfrontHeavyTrackConditionFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumHeavy > 0).Select(hs => func(hs).HeavyTrackConditionFactor));
           BestInfrontWinterTrackConditionFactor = GetBest(inSameOrEqualPos.Where(hs => hs.NumWinter > 0).Select(hs => func(hs).WinterTrackConditionFactor));


            #endregion

            #region Relative calcs

            #region Profile factors

            // Profile?
            Func<HorseStats, HorseStatsProfile> profSel = (hs) => hs.KmTimeValidProfile;

   
            if (profileEnum == ProfileEnum.TimeAfterWinnerLastCapped)
            {
                profSel = (hs) => hs.TimeAfterWinnerLastCapProfile;
            }
            else if (profileEnum == ProfileEnum.TimeAfterWinnerPlacedCapped)
            {
                profSel = (hs) => hs.TimeAfterWinnerPlaceCapProfile;
            }
            Func<HorseStatsProfile, float> propSel = null;
            if (distBucket == 0)
            {
                propSel = (p) => p.ShortDistanceFactor;
            }
            else if (distBucket == 1)
            {
                propSel = (p) => p.MediumDistanceFactor;
            }
            else if (distBucket == 2)
            {
                propSel = (p) => p.LongDistanceFactor;
            }
            RelDistanceFactorNormalized = Normalize(propSel(profSel(stats)), rest.Select(hs => propSel(profSel(hs))));

            propSel = null;
            if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Auto)
            {
                propSel = (p) => p.AutoStartFactor;
            }
            else if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Volt)
            {
                propSel = (p) => p.VoltStartFactor;
            }

            RelStartFactorNormalized = Normalize(propSel(profSel(stats)), rest.Select(hs => propSel(profSel(hs))));

            if (trackCondition == TrackConditionEnum.Light)
            {
                propSel = (p) => p.LightTrackConditionFactor;
            }
            else if (trackCondition == TrackConditionEnum.Heavier)
            {
                propSel = (p) => p.HeavierTrackConditionFactor;
            }
            else if (trackCondition == TrackConditionEnum.Heavy)
            {
                propSel = (p) => p.HeavyTrackConditionFactor;
            }
            else if (trackCondition == TrackConditionEnum.Winter)
            {
                propSel = (p) => p.WinterTrackConditionFactor;
            }
            RelTrackConditionFactorNormalized = Normalize(propSel(profSel(stats)), rest.Select(hs => propSel(profSel(hs))));
            #endregion

            #region Best total
            RelBestNormalized = GetNormalizedRelativeSpeed(stats.Best, rest.Select(hs => hs.Best), (long)GetMedian(rest.Select(hs => hs.Best)));
            RelMedianNormalized = GetNormalizedRelativeSpeed(stats.Median, rest.Select(hs => hs.Median), (long)GetMedian(rest.Select(hs => hs.Median)));
            RelAverageNormalized = GetNormalizedRelativeSpeed(stats.Average, rest.Select(hs => hs.Average), (long)GetMedian(rest.Select(hs => hs.Average)));

            RelInGroupBestNormalized = GetNormalizedRelativeSpeed(stats.Best, inSameOrEqualPos.Select(hs => hs.Best), (long)GetMedian(inSameOrEqualPos.Select(hs => hs.Best)));
            RelInGroupMedianNormalized = GetNormalizedRelativeSpeed(stats.Median, inSameOrEqualPos.Select(hs => hs.Median), (long)GetMedian(inSameOrEqualPos.Select(hs => hs.Median)));
            RelInGroupAverageNormalized = GetNormalizedRelativeSpeed(stats.Average, inSameOrEqualPos.Select(hs => hs.Average), (long)GetMedian(inSameOrEqualPos.Select(hs => hs.Average)));
            #endregion

            #region Driver stats
            RelDriverWinPercent = Normalize(DriverWinPercent, rest.Select(hs => hs.DriverWinPercent));
            RelDriverPlacePercent = Normalize(DriverPlacePercent, rest.Select(hs => hs.DriverPlacePercent));
            RelDriverTop3Percent = Normalize(DriverTop3Percent, rest.Select(hs => hs.DriverTop3Percent));

            RelDriverMoneyPerStart = Normalize(DriverMoneyPerStart, rest.Select(hs => hs.DriverMoneyPerStart));

            if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Auto)
            {
                RelDriverWinPercentStartType = Normalize(DriverWinPercentAuto, rest.Select(hs => hs.DriverWinPercentAuto));
                RelDriverPlacePercentStartType = Normalize(DriverPlacePercentAuto, rest.Select(hs => hs.DriverPlacePercentAuto));
                RelDriverTop3PercentStartType = Normalize(DriverTop3PercentAuto, rest.Select(hs => hs.DriverTop3PercentAuto));
                RelDriverMoneyPerStartStartType = Normalize(DriverMoneyPerStartAuto, rest.Select(hs => hs.DriverMoneyPerStartAuto));
            }
            else if (race.StartType == ATG.Shared.Enums.StartTypeEnum.Volt)
            {
                RelDriverWinPercentStartType = Normalize(DriverWinPercentVolt, rest.Select(hs => hs.DriverWinPercentVolt));
                RelDriverPlacePercentStartType = Normalize(DriverPlacePercentVolt, rest.Select(hs => hs.DriverPlacePercentVolt));
                RelDriverTop3PercentStartType = Normalize(DriverTop3PercentVolt, rest.Select(hs => hs.DriverTop3PercentVolt));
                RelDriverMoneyPerStartStartType = Normalize(DriverMoneyPerStartVolt, rest.Select(hs => hs.DriverMoneyPerStartVolt));
            }
            
            #endregion

            #region On distance

            Func<HorseStats, long?> selector = null;
            var st = race.StartType;

            if (distBucket == 0)
            {
                selector = (hs) => hs.BestShortTime;
           }
            else if (distBucket == 1)
            {
                selector = (hs) => hs.BestMediumTime;
            }
            else if (distBucket == 2)
            {
                selector = (hs) => hs.BestLongTime;
            }
            RelBestOnDistanceNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupBestOnDistanceNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            if (distBucket == 0)
            {
                selector = (hs) => hs.MedianShortTime;
            }
            else if (distBucket == 1)
            {
                selector = (hs) => hs.MedianMediumTime;
            }
            else if (distBucket == 2)
            {
                selector = (hs) => hs.MedianLongTime;
            }
            RelMedianOnDistanceNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupMedianOnDistanceNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));

            if (distBucket == 0)
            {
                selector = (hs) => hs.AverageShortTime;
            }
            else if (distBucket == 1)
            {
                selector = (hs) => hs.AverageMediumTime;
            }
            else if (distBucket == 2)
            {
                selector = (hs) => hs.AverageLongTime;
            }
            RelAverageOnDistanceNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupAverageOnDistanceNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            #endregion

            #region On startType

            if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
            {
                selector = (hs) => hs.BestAuto;
            }
            else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
            {
                selector = (hs) => hs.BestVolt;
            }
            RelBestOnStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupBestOnStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
            {
                selector = (hs) => hs.MedianAuto;
            }
            else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
            {
                selector = (hs) => hs.MedianVolt;
            }
            RelMedianOnStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupMedianOnStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            
            if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
            {
                selector = (hs) => hs.AverageAuto;
            }
            else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
            {
                selector = (hs) => hs.AverageVolt;
            }
            RelAverageOnStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupAverageOnStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            #endregion

            #region On dist and startType
            if (distBucket == 0)
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.BestShortAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.BestShortVolt;
                }
            }
            else if (distBucket == 1)
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.BestMediumAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.BestMediumVolt;
                }
            }
            else
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.BestLongAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.BestLongVolt;
                }
            }
            RelBestOnDistanceAndStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
           RelInGroupBestOnDistanceAndStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            
            if (distBucket == 0)
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.MedianShortAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.MedianShortVolt;
                }
            }
            else if (distBucket == 1)
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.MedianMediumAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.MedianMediumVolt;
                }
            }
            else
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.MedianLongAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.MedianLongVolt;
                }
            }
            RelMedianOnDistanceAndStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupMedianOnDistanceAndStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            
            if (distBucket == 0)
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.AverageShortAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.AverageShortVolt;
                }
            }
            else if (distBucket == 1)
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.AverageMediumAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.AverageMediumAuto;
                }
            }
            else
            {
                if (st == ATG.Shared.Enums.StartTypeEnum.Auto)
                {
                    selector = (hs) => hs.AverageLongAuto;
                }
                else if (st == ATG.Shared.Enums.StartTypeEnum.Volt)
                {
                    selector = (hs) => hs.AverageLongVolt;
                }
            }
            RelAverageOnDistanceAndStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), rest.Select(hs => selector(hs)), (long)GetMedian(rest.Select(hs => selector(hs))));
            RelInGroupAverageOnDistanceAndStartTypeNormalized = GetNormalizedRelativeSpeed(selector(stats), inSameOrEqualPos.Select(hs => selector(hs)), (long)GetMedian(inSameOrEqualPos.Select(hs => selector(hs))));
            #endregion
            #endregion
        }

        public long HorseId { get; set; }
        public long RaceId { get; set; }
        public DateTime Timestamp { get; set; }
        public long ArenaId { get; set; }

        #region Start specific stats
        public float NumHorsesInEqualOrBetterStartPosition { get; set; }
        public float NumStartingHorses { get; set; }
        public float WinFromStartPos { get; set; }
        public int StartGroup { get; set; }
        public int NumStartsOnDistance { get; set; }
        public int NumStartsOnType { get; set; }
        public int NumStartsOnDistanceAndType { get; set; }
        public float TimeAfterWinner { get; set; }
        public float TimeAfterWinnerCappedLast { get; set; }
        #endregion

        #region Relative to others stats

        #region Total
        public float RelBestNormalized { get; set; }
        public float RelMedianNormalized { get; set; }
        public float RelAverageNormalized { get; set; }


        public float RelInGroupBestNormalized { get; set; }
        public float RelInGroupMedianNormalized { get; set; }
        public float RelInGroupAverageNormalized { get; set; }
        #endregion

        #region Profile
        public float RelDistanceFactorNormalized { get; set; }
        public float RelStartFactorNormalized { get; set; }
        public float RelTrackConditionFactorNormalized { get; set; }
        #endregion

        #region On same distance
        public float RelBestOnDistanceNormalized { get; set; }
        public float RelMedianOnDistanceNormalized { get; set; }
        public float RelAverageOnDistanceNormalized { get; set; }

        public float RelInGroupBestOnDistanceNormalized { get; set; }
        public float RelInGroupMedianOnDistanceNormalized { get; set; }
        public float RelInGroupAverageOnDistanceNormalized { get; set; }
        #endregion

        #region On same start-type
        public float RelBestOnStartTypeNormalized { get; set; }
        public float RelMedianOnStartTypeNormalized { get; set; }
        public float RelAverageOnStartTypeNormalized { get; set; }

        public float RelInGroupBestOnStartTypeNormalized { get; set; }
        public float RelInGroupMedianOnStartTypeNormalized { get; set; }
        public float RelInGroupAverageOnStartTypeNormalized { get; set; }
        #endregion

        #region Same distance and starttype
        public float RelBestOnDistanceAndStartTypeNormalized { get; set; }
        public float RelMedianOnDistanceAndStartTypeNormalized { get; set; }
        public float RelAverageOnDistanceAndStartTypeNormalized { get; set; }


        public float RelInGroupBestOnDistanceAndStartTypeNormalized { get; set; }
        public float RelInGroupMedianOnDistanceAndStartTypeNormalized { get; set; }
        public float RelInGroupAverageOnDistanceAndStartTypeNormalized { get; set; }
        #endregion

        #region Driver
        public float RelDriverWinPercent { get; set; }
        public float RelDriverTop3Percent { get; set; }
        public float RelDriverPlacePercent { get; set; }
        public float RelDriverMoneyPerStart { get; set; }

        public float RelDriverWinPercentStartType { get; set; }
        public float RelDriverTop3PercentStartType { get; set; }
        public float RelDriverPlacePercentStartType { get; set; }
        public float RelDriverMoneyPerStartStartType { get; set; }

        #endregion


        #endregion

        #region This starter stats
        #region Speedfigures

        #region Best
        public float BestShortAuto { get; set; }
        public float BestShortVolt { get; set; }
        public float BestShortTime { get; set; }

        public float BestMediumAuto { get; set; }
        public float BestMediumVolt { get; set; }
        public float BestMediumTime { get; set; }

        public float BestLongAuto { get; set; }
        public float BestLongVolt { get; set; }
        public float BestLongTime { get; set; }


        public float BestAuto { get; set; }
        public float BestVolt { get; set; }
        public float Best { get; set; }


        public float BestLastMonthShort { get; set; }
        public float BestLastMonthMedium { get; set; }
        public float BestLastMonthLong { get; set; }
        public float BestLastMonth { get; set; }
        #endregion

        #region Median
        public float MedianLongAuto { get; set; }
        public float MedianLongVolt { get; set; }
        public float MedianLongTime { get; set; }

        public float MedianShortAuto { get; set; }
        public float MedianShortVolt { get; set; }
        public float MedianShortTime { get; set; }


        public float MedianMediumAuto { get; set; }
        public float MedianMediumVolt { get; set; }
        public float MedianMediumTime { get; set; }

        public float MedianAuto { get; set; }
        public float MedianVolt { get; set; }
        public float Median { get; set; }


        public float MedianLastMonthShort { get; set; }
        public float MedianLastMonthMedium { get; set; }
        public float MedianLastMonthLong { get; set; }
        public float MedianLastMonth { get; set; }
        #endregion

        #region Average
        public float AverageMediumAuto { get; set; }
        public float AverageMediumVolt { get; set; }
        public float AverageMediumTime { get; set; }

        public float AverageLongAuto { get; set; }
        public float AverageLongVolt { get; set; }
        public float AverageLongTime { get; set; }

        public float AverageShortAuto { get; set; }
        public float AverageShortVolt { get; set; }
        public float AverageShortTime { get; set; }

        public float AverageAuto { get; set; }
        public float AverageVolt { get; set; }
        public float Average { get; set; }


        public float AverageLastMonthShort { get; set; }
        public float AverageLastMonthMedium { get; set; }
        public float AverageLastMonthLong { get; set; }
        public float AverageLastMonth { get; set; }
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
        public float ShortDistanceFactor { get; set; } = 1;
        public float MediumDistanceFactor { get; set; } = 1;
        public float LongDistanceFactor { get; set; } = 1;
        public float AutoStartFactor { get; set; } = 1;
        public float VoltStartFactor { get; set; } = 1;
        public float TrackConditionFactor { get; set; } = 1;
        public float LightTrackConditionFactor { get; set; } = 1;
        public float HeavierTrackConditionFactor { get; set; } = 1;
        public float HeavyTrackConditionFactor { get; set; } = 1;
        public float WinterTrackConditionFactor { get; set; } = 1;

        /*
        public HorseStatsProfile TimeAfterWinnerLastCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerPlaceCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerMinMaxProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerNormMinPlacedCapProfile { get; set; }
        public HorseStatsProfile KmTimeValidProfile { get; set; }
        */

        #endregion
        #endregion


        #region Median other stats
        #region Speedfigures

        #region Best
        public float MedianBestShortAuto { get; set; }
        public float MedianBestShortVolt { get; set; }
        public float MedianBestShortTime { get; set; }

        public float MedianBestMediumAuto { get; set; }
        public float MedianBestMediumVolt { get; set; }
        public float MedianBestMediumTime { get; set; }

        public float MedianBestLongAuto { get; set; }
        public float MedianBestLongVolt { get; set; }
        public float MedianBestLongTime { get; set; }


        public float MedianBestAuto { get; set; }
        public float MedianBestVolt { get; set; }
        public float MedianBest { get; set; }


        public float MedianBestLastMonthShort { get; set; }
        public float MedianBestLastMonthMedium { get; set; }
        public float MedianBestLastMonthLong { get; set; }
        public float MedianBestLastMonth { get; set; }
        #endregion

        #region Median
        public float MedianMedianLongAuto { get; set; }
        public float MedianMedianLongVolt { get; set; }
        public float MedianMedianLongTime { get; set; }

        public float MedianMedianShortAuto { get; set; }
        public float MedianMedianShortVolt { get; set; }
        public float MedianMedianShortTime { get; set; }


        public float MedianMedianMediumAuto { get; set; }
        public float MedianMedianMediumVolt { get; set; }
        public float MedianMedianMediumTime { get; set; }

        public float MedianMedianAuto { get; set; }
        public float MedianMedianVolt { get; set; }
        public float MedianMedian { get; set; }


        public float MedianMedianLastMonthShort { get; set; }
        public float MedianMedianLastMonthMedium { get; set; }
        public float MedianMedianLastMonthLong { get; set; }
        public float MedianMedianLastMonth { get; set; }
        #endregion

        #region Average
        public float MedianAverageMediumAuto { get; set; }
        public float MedianAverageMediumVolt { get; set; }
        public float MedianAverageMediumTime { get; set; }

        public float MedianAverageLongAuto { get; set; }
        public float MedianAverageLongVolt { get; set; }
        public float MedianAverageLongTime { get; set; }

        public float MedianAverageShortAuto { get; set; }
        public float MedianAverageShortVolt { get; set; }
        public float MedianAverageShortTime { get; set; }

        public float MedianAverageAuto { get; set; }
        public float MedianAverageVolt { get; set; }
        public float MedianAverage { get; set; }


        public float MedianAverageLastMonthShort { get; set; }
        public float MedianAverageLastMonthMedium { get; set; }
        public float MedianAverageLastMonthLong { get; set; }
        public float MedianAverageLastMonth { get; set; }
        #endregion




        #endregion

        #region Horse stats

        public float MedianWinPercent { get; set; }
        public float MedianTop3Percent { get; set; }
        public float MedianPlacePercent { get; set; }
        public float MedianMoneyPerStartShape { get; set; }
        public float MedianMoneyPerStart { get; set; }
        public float MedianWinShape { get; set; }
        public float MedianPlaceShape { get; set; }
        public float MedianTop3Shape { get; set; }
        public float MedianTimeAfterWinnerShapeAverage { get; set; }
        public float MedianTimeAfterWinnerShapeMedian { get; set; }
        public float MedianTimeAfterWinnerLast { get; set; }
        public float MedianTimeAfterWinnerMedian { get; set; }
        public float MedianTimeAfterWinnerAverage { get; set; }

        #endregion

        #region Driver stats

        public float MedianDriverWinPercent { get; set; }
        public float MedianDriverTop3Percent { get; set; }
        public float MedianDriverPlacePercent { get; set; }
        public float MedianDriverMoneyPerStart { get; set; }

  
        public float MedianDriverWinPercentAuto { get; set; }
        public float MedianDriverTop3PercentAuto { get; set; }
        public float MedianDriverPlacePercentAuto { get; set; }
        public float MedianDriverMoneyPerStartAuto { get; set; }

     
        public float MedianDriverWinPercentVolt { get; set; }
        public float MedianDriverTop3PercentVolt { get; set; }
        public float MedianDriverPlacePercentVolt { get; set; }
        public float MedianDriverMoneyPerStartVolt { get; set; }

        #endregion


        #region Profile


        public float MedianShortDistanceFactor { get; set; } = 1;
        public float MedianMediumDistanceFactor { get; set; } = 1;
        public float MedianLongDistanceFactor { get; set; } = 1;
        public float MedianAutoStartFactor { get; set; } = 1;
        public float MedianVoltStartFactor { get; set; } = 1;
        public float MedianTrackConditionFactor { get; set; } = 1;
        public float MedianLightTrackConditionFactor { get; set; } = 1;
        public float MedianHeavierTrackConditionFactor { get; set; } = 1;
        public float MedianHeavyTrackConditionFactor { get; set; } = 1;
        public float MedianWinterTrackConditionFactor { get; set; } = 1;

        /*
        public HorseStatsProfile TimeAfterWinnerLastCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerPlaceCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerMinMaxProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerNormMinPlacedCapProfile { get; set; }
        public HorseStatsProfile KmTimeValidProfile { get; set; }
        */

        #endregion

        #endregion


        #region Average other stats
        #region Speedfigures

        #region Best
        public float AverageBestShortAuto { get; set; }
        public float AverageBestShortVolt { get; set; }
        public float AverageBestShortTime { get; set; }

        public float AverageBestMediumAuto { get; set; }
        public float AverageBestMediumVolt { get; set; }
        public float AverageBestMediumTime { get; set; }

        public float AverageBestLongAuto { get; set; }
        public float AverageBestLongVolt { get; set; }
        public float AverageBestLongTime { get; set; }


        public float AverageBestAuto { get; set; }
        public float AverageBestVolt { get; set; }
        public float AverageBest { get; set; }


        public float AverageBestLastMonthShort { get; set; }
        public float AverageBestLastMonthMedium { get; set; }
        public float AverageBestLastMonthLong { get; set; }
        public float AverageBestLastMonth { get; set; }
        #endregion

        #region Median
        public float AverageMedianLongAuto { get; set; }
        public float AverageMedianLongVolt { get; set; }
        public float AverageMedianLongTime { get; set; }

        public float AverageMedianShortAuto { get; set; }
        public float AverageMedianShortVolt { get; set; }
        public float AverageMedianShortTime { get; set; }


        public float AverageMedianMediumAuto { get; set; }
        public float AverageMedianMediumVolt { get; set; }
        public float AverageMedianMediumTime { get; set; }

        public float AverageMedianAuto { get; set; }
        public float AverageMedianVolt { get; set; }
        public float AverageMedian { get; set; }


        public float AverageMedianLastMonthShort { get; set; }
        public float AverageMedianLastMonthMedium { get; set; }
        public float AverageMedianLastMonthLong { get; set; }
        public float AverageMedianLastMonth { get; set; }
        #endregion

        #region Average
        public float AverageAverageMediumAuto { get; set; }
        public float AverageAverageMediumVolt { get; set; }
        public float AverageAverageMediumTime { get; set; }

        public float AverageAverageLongAuto { get; set; }
        public float AverageAverageLongVolt { get; set; }
        public float AverageAverageLongTime { get; set; }

        public float AverageAverageShortAuto { get; set; }
        public float AverageAverageShortVolt { get; set; }
        public float AverageAverageShortTime { get; set; }

        public float AverageAverageAuto { get; set; }
        public float AverageAverageVolt { get; set; }
        public float AverageAverage { get; set; }


        public float AverageAverageLastMonthShort { get; set; }
        public float AverageAverageLastMonthMedium { get; set; }
        public float AverageAverageLastMonthLong { get; set; }
        public float AverageAverageLastMonth { get; set; }
        #endregion




        #endregion

        #region Horse stats

        public float AverageWinPercent { get; set; }
        public float AverageTop3Percent { get; set; }
        public float AveragePlacePercent { get; set; }
        public float AverageMoneyPerStartShape { get; set; }
        public float AverageMoneyPerStart { get; set; }
        public float AverageWinShape { get; set; }
        public float AveragePlaceShape { get; set; }
        public float AverageTop3Shape { get; set; }
        public float AverageTimeAfterWinnerShapeAverage { get; set; }
        public float AverageTimeAfterWinnerShapeMedian { get; set; }
        public float AverageTimeAfterWinnerLast { get; set; }
        public float AverageTimeAfterWinnerMedian { get; set; }
        public float AverageTimeAfterWinnerAverage { get; set; }

        #endregion

        #region Driver stats

        public float AverageDriverWinPercent { get; set; }
        public float AverageDriverTop3Percent { get; set; }
        public float AverageDriverPlacePercent { get; set; }
        public float AverageDriverMoneyPerStart { get; set; }


        public float AverageDriverWinPercentAuto { get; set; }
        public float AverageDriverTop3PercentAuto { get; set; }
        public float AverageDriverPlacePercentAuto { get; set; }
        public float AverageDriverMoneyPerStartAuto { get; set; }


        public float AverageDriverWinPercentVolt { get; set; }
        public float AverageDriverTop3PercentVolt { get; set; }
        public float AverageDriverPlacePercentVolt { get; set; }
        public float AverageDriverMoneyPerStartVolt { get; set; }

        #endregion


        #region Profile


        public float AverageShortDistanceFactor { get; set; } = 1;
        public float AverageMediumDistanceFactor { get; set; } = 1;
        public float AverageLongDistanceFactor { get; set; } = 1;
        public float AverageAutoStartFactor { get; set; } = 1;
        public float AverageVoltStartFactor { get; set; } = 1;
        public float AverageTrackConditionFactor { get; set; } = 1;
        public float AverageLightTrackConditionFactor { get; set; } = 1;
        public float AverageHeavierTrackConditionFactor { get; set; } = 1;
        public float AverageHeavyTrackConditionFactor { get; set; } = 1;
        public float AverageWinterTrackConditionFactor { get; set; } = 1;

        /*
        public HorseStatsProfile TimeAfterWinnerLastCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerPlaceCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerMinMaxProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerNormMinPlacedCapProfile { get; set; }
        public HorseStatsProfile KmTimeValidProfile { get; set; }
        */

        #endregion

        #endregion

        #region Best other stats
        #region Speedfigures

        #region Best
        public float BestBestShortAuto { get; set; }
        public float BestBestShortVolt { get; set; }
        public float BestBestShortTime { get; set; }

        public float BestBestMediumAuto { get; set; }
        public float BestBestMediumVolt { get; set; }
        public float BestBestMediumTime { get; set; }

        public float BestBestLongAuto { get; set; }
        public float BestBestLongVolt { get; set; }
        public float BestBestLongTime { get; set; }


        public float BestBestAuto { get; set; }
        public float BestBestVolt { get; set; }
        public float BestBest { get; set; }


        public float BestBestLastMonthShort { get; set; }
        public float BestBestLastMonthMedium { get; set; }
        public float BestBestLastMonthLong { get; set; }
        public float BestBestLastMonth { get; set; }
        #endregion

        #region Median
        public float BestMedianLongAuto { get; set; }
        public float BestMedianLongVolt { get; set; }
        public float BestMedianLongTime { get; set; }

        public float BestMedianShortAuto { get; set; }
        public float BestMedianShortVolt { get; set; }
        public float BestMedianShortTime { get; set; }


        public float BestMedianMediumAuto { get; set; }
        public float BestMedianMediumVolt { get; set; }
        public float BestMedianMediumTime { get; set; }

        public float BestMedianAuto { get; set; }
        public float BestMedianVolt { get; set; }
        public float BestMedian { get; set; }


        public float BestMedianLastMonthShort { get; set; }
        public float BestMedianLastMonthMedium { get; set; }
        public float BestMedianLastMonthLong { get; set; }
        public float BestMedianLastMonth { get; set; }
        #endregion

        #region Average
        public float BestAverageMediumAuto { get; set; }
        public float BestAverageMediumVolt { get; set; }
        public float BestAverageMediumTime { get; set; }

        public float BestAverageLongAuto { get; set; }
        public float BestAverageLongVolt { get; set; }
        public float BestAverageLongTime { get; set; }

        public float BestAverageShortAuto { get; set; }
        public float BestAverageShortVolt { get; set; }
        public float BestAverageShortTime { get; set; }

        public float BestAverageAuto { get; set; }
        public float BestAverageVolt { get; set; }
        public float BestAverage { get; set; }


        public float BestAverageLastMonthShort { get; set; }
        public float BestAverageLastMonthMedium { get; set; }
        public float BestAverageLastMonthLong { get; set; }
        public float BestAverageLastMonth { get; set; }
        #endregion




        #endregion

        #region Horse stats

        public float BestWinPercent { get; set; }
        public float BestTop3Percent { get; set; }
        public float BestPlacePercent { get; set; }
        public float BestMoneyPerStartShape { get; set; }
        public float BestMoneyPerStart { get; set; }
        public float BestWinShape { get; set; }
        public float BestPlaceShape { get; set; }
        public float BestTop3Shape { get; set; }
        public float BestTimeAfterWinnerShapeAverage { get; set; }
        public float BestTimeAfterWinnerShapeMedian { get; set; }
        public float BestTimeAfterWinnerLast { get; set; }
        public float BestTimeAfterWinnerMedian { get; set; }
        public float BestTimeAfterWinnerAverage { get; set; }

        #endregion

        #region Driver stats

        public float BestDriverWinPercent { get; set; }
        public float BestDriverTop3Percent { get; set; }
        public float BestDriverPlacePercent { get; set; }
        public float BestDriverMoneyPerStart { get; set; }


        public float BestDriverWinPercentAuto { get; set; }
        public float BestDriverTop3PercentAuto { get; set; }
        public float BestDriverPlacePercentAuto { get; set; }
        public float BestDriverMoneyPerStartAuto { get; set; }


        public float BestDriverWinPercentVolt { get; set; }
        public float BestDriverTop3PercentVolt { get; set; }
        public float BestDriverPlacePercentVolt { get; set; }
        public float BestDriverMoneyPerStartVolt { get; set; }

        #endregion


        #region Profile


        public float BestShortDistanceFactor { get; set; } = 1;
        public float BestMediumDistanceFactor { get; set; } = 1;
        public float BestLongDistanceFactor { get; set; } = 1;
        public float BestAutoStartFactor { get; set; } = 1;
        public float BestVoltStartFactor { get; set; } = 1;
        public float BestTrackConditionFactor { get; set; } = 1;
        public float BestLightTrackConditionFactor { get; set; } = 1;
        public float BestHeavierTrackConditionFactor { get; set; } = 1;
        public float BestHeavyTrackConditionFactor { get; set; } = 1;
        public float BestWinterTrackConditionFactor { get; set; } = 1;

        /*
        public HorseStatsProfile TimeAfterWinnerLastCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerPlaceCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerMinMaxProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerNormMinPlacedCapProfile { get; set; }
        public HorseStatsProfile KmTimeValidProfile { get; set; }
        */

        #endregion

        #endregion

        #region Best in same or better position
        #region Speedfigures

        #region Best
        public float BestInfrontBestShortAuto { get; set; }
        public float BestInfrontBestShortVolt { get; set; }
        public float BestInfrontBestShortTime { get; set; }

        public float BestInfrontBestMediumAuto { get; set; }
        public float BestInfrontBestMediumVolt { get; set; }
        public float BestInfrontBestMediumTime { get; set; }

        public float BestInfrontBestLongAuto { get; set; }
        public float BestInfrontBestLongVolt { get; set; }
        public float BestInfrontBestLongTime { get; set; }


        public float BestInfrontBestAuto { get; set; }
        public float BestInfrontBestVolt { get; set; }
        public float BestInfrontBest { get; set; }


        public float BestInfrontBestLastMonthShort { get; set; }
        public float BestInfrontBestLastMonthMedium { get; set; }
        public float BestInfrontBestLastMonthLong { get; set; }
        public float BestInfrontBestLastMonth { get; set; }
        #endregion

        #region Median
        public float BestInfrontMedianLongAuto { get; set; }
        public float BestInfrontMedianLongVolt { get; set; }
        public float BestInfrontMedianLongTime { get; set; }

        public float BestInfrontMedianShortAuto { get; set; }
        public float BestInfrontMedianShortVolt { get; set; }
        public float BestInfrontMedianShortTime { get; set; }


        public float BestInfrontMedianMediumAuto { get; set; }
        public float BestInfrontMedianMediumVolt { get; set; }
        public float BestInfrontMedianMediumTime { get; set; }

        public float BestInfrontMedianAuto { get; set; }
        public float BestInfrontMedianVolt { get; set; }
        public float BestInfrontMedian { get; set; }


        public float BestInfrontMedianLastMonthShort { get; set; }
        public float BestInfrontMedianLastMonthMedium { get; set; }
        public float BestInfrontMedianLastMonthLong { get; set; }
        public float BestInfrontMedianLastMonth { get; set; }
        #endregion

        #region Average
        public float BestInfrontAverageMediumAuto { get; set; }
        public float BestInfrontAverageMediumVolt { get; set; }
        public float BestInfrontAverageMediumTime { get; set; }

        public float BestInfrontAverageLongAuto { get; set; }
        public float BestInfrontAverageLongVolt { get; set; }
        public float BestInfrontAverageLongTime { get; set; }

        public float BestInfrontAverageShortAuto { get; set; }
        public float BestInfrontAverageShortVolt { get; set; }
        public float BestInfrontAverageShortTime { get; set; }

        public float BestInfrontAverageAuto { get; set; }
        public float BestInfrontAverageVolt { get; set; }
        public float BestInfrontAverage { get; set; }


        public float BestInfrontAverageLastMonthShort { get; set; }
        public float BestInfrontAverageLastMonthMedium { get; set; }
        public float BestInfrontAverageLastMonthLong { get; set; }
        public float BestInfrontAverageLastMonth { get; set; }
        #endregion




        #endregion

        #region Horse stats

        public float BestInfrontWinPercent { get; set; }
        public float BestInfrontTop3Percent { get; set; }
        public float BestInfrontPlacePercent { get; set; }
        public float BestInfrontMoneyPerStartShape { get; set; }
        public float BestInfrontMoneyPerStart { get; set; }
        public float BestInfrontWinShape { get; set; }
        public float BestInfrontPlaceShape { get; set; }
        public float BestInfrontTop3Shape { get; set; }
        public float BestInfrontTimeAfterWinnerShapeAverage { get; set; }
        public float BestInfrontTimeAfterWinnerShapeMedian { get; set; }
        public float BestInfrontTimeAfterWinnerLast { get; set; }
        public float BestInfrontTimeAfterWinnerMedian { get; set; }
        public float BestInfrontTimeAfterWinnerAverage { get; set; }

        #endregion

        #region Driver stats

        public float BestInfrontDriverWinPercent { get; set; }
        public float BestInfrontDriverTop3Percent { get; set; }
        public float BestInfrontDriverPlacePercent { get; set; }
        public float BestInfrontDriverMoneyPerStart { get; set; }


        public float BestInfrontDriverWinPercentAuto { get; set; }
        public float BestInfrontDriverTop3PercentAuto { get; set; }
        public float BestInfrontDriverPlacePercentAuto { get; set; }
        public float BestInfrontDriverMoneyPerStartAuto { get; set; }


        public float BestInfrontDriverWinPercentVolt { get; set; }
        public float BestInfrontDriverTop3PercentVolt { get; set; }
        public float BestInfrontDriverPlacePercentVolt { get; set; }
        public float BestInfrontDriverMoneyPerStartVolt { get; set; }

        #endregion


        #region Profile


        public float BestInfrontShortDistanceFactor { get; set; } = 1;
        public float BestInfrontMediumDistanceFactor { get; set; } = 1;
        public float BestInfrontLongDistanceFactor { get; set; } = 1;
        public float BestInfrontAutoStartFactor { get; set; } = 1;
        public float BestInfrontVoltStartFactor { get; set; } = 1;
        public float BestInfrontTrackConditionFactor { get; set; } = 1;
        public float BestInfrontLightTrackConditionFactor { get; set; } = 1;
        public float BestInfrontHeavierTrackConditionFactor { get; set; } = 1;
        public float BestInfrontHeavyTrackConditionFactor { get; set; } = 1;
        public float BestInfrontWinterTrackConditionFactor { get; set; } = 1;

        /*
        public HorseStatsProfile TimeAfterWinnerLastCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerPlaceCapProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerMinMaxProfile { get; set; }
        public HorseStatsProfile TimeAfterWinnerNormMinPlacedCapProfile { get; set; }
        public HorseStatsProfile KmTimeValidProfile { get; set; }
        */

        #endregion

        #endregion
    }
}
