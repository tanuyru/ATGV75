using ATG.DB.Entities;
using ATG.ML.Models;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.MLModels
{
    public class RaceResultPrediction
    {
        [ColumnName("Score")]
        public float TimeAfterWinner { get; set; }
    }
    public class RaceResultModel
    {
        public RaceResultModel()
        { }

        public RaceResultModel(
            Race race, 
            RaceResult rr, 
            IEnumerable<RecentHorseStart> horseRecent,
            IEnumerable<RecentHorseStart> allRecent,
            double distribution,
            bool gallopedLast,
            float winPercentOnTrackDistance)
        {
            IEnumerable<float> kmTimes = null;
            var validTimes = horseRecent.Where(rs => !rs.DQ && rs.KmTimeMilliseconds > 0);
            var validAlls = allRecent.Where(rs => !rs.DQ && rs.KmTimeMilliseconds > 0);
            if (validTimes.Any())
            {
                kmTimes = validTimes.Select(rs => (float)rs.KmTimeMilliseconds);
            }
            else if (validAlls.Any())
            {
                kmTimes = validAlls.Select(rs => (float)rs.KmTimeMilliseconds);
            }
            
            if (kmTimes.Any())
            {
                MedianKmTime = kmTimes.GetMedian();
                BestKmTime = kmTimes.Average();
                AvgKmTime = kmTimes.Average();
            }
    
            FinishPosition = rr.FinishPosition;
            HorseId = rr.HorseId;
            //TrainerId = rr.TrainerId;
            RaceId = race.Id;
            DriverId = rr.DriverId;
            Track = (float)rr.Track;
            Position = rr.Position;
            KmTimeMilliSeconds = rr.KmTimeMilliSeconds;
            WinOdds = (float)rr.WinOdds;
            PrizeMoney = rr.PrizeMoney;
            DQ = rr.DQ;
            Galopp = rr.Galopp;
            if (rr.FrontShoes.HasValue)
                FrontShoes = rr.FrontShoes.Value;
            if (rr.BackShoes.HasValue)
                BackShoes = rr.BackShoes.Value;
            if (rr.FrontChange.HasValue)
                FrontChange = rr.FrontChange.Value;
            if (rr.BackChange.HasValue)
                BackChange = rr.BackChange.Value;
            Scratched = rr.Scratched;

            HorseMoneyTotal = (float)rr.HorseMoneyTotal;
            HorseMoneyLastYear = (float)rr.HorseMoneyLastYear;
            LastYearMoneyPerStart = (float)rr.LastYearMoneyPerStart;
            HorseMoneyThisYear = (float)rr.HorseMoneyThisYear;


            HorseTotalMoney = (float)rr.HorseTotalMoney;
            HorseTotalWinPercent = (float)rr.HorseTotalWinPercent;
            HorseTotalPlacepercent = (float)rr.HorseTotalPlacepercent;
            StartsThisYear = rr.Starts;
            WinThisYears = rr.Wins;
            SecondsThisYear = rr.Seconds;
            ThirdsThisYear = rr.Thirds;

            MoneyPerStart = (float)rr.MoneyPerStart;
            LastYearStarts = (float)rr.LastYearStarts;
            LastYearSeconds = (float)rr.LastYearSeconds;
            LastYearThirds = (float)rr.LastYearThirds;

            TrainerWinPercent = (float)rr.TrainerWinPercent;
            DriverWinPercent = (float)rr.DriverWinPercent;
            TrainerMoneyThisYear = (float)rr.TrainerMoneyThisYear;
            TrainerMoneyLastYear = (float)rr.TrainerMoneyLastYear;

            TrainerStarts = (float)rr.TrainerStarts;
            TrainerWins = (float)rr.TrainerWins;
            TrainerSeconds = (float)rr.TrainerSeconds;
            TrainerThirds = (float)rr.TrainerThirds;

            LastYearTrainerStarts = (float)rr.LastYearTrainerStarts;
            LastYearTrainerWins = (float)rr.LastYearTrainerWins;
            LastYearTrainerSeconds = (float)rr.LastYearTrainerSeconds;
            LastYearTrainerThirds = (float)rr.LastYearTrainerThirds;
            LastYearTrainerWinPercent = (float)rr.LastYearTrainerWinPercent;

            DriverMoneyLastYear = (float)rr.DriverMoneyLastYear;
            DriverMoney = (float)rr.DriverMoney;
            DriverStarts = (float)rr.DriverStarts;
            DriverWins = (float)rr.DriverWins;
            DriverSeconds = (float)rr.DriverSeconds;
            DriverThirds = (float)rr.DriverThirds;

            LastYearDriverStarts = (float)rr.LastYearDriverStarts;
            LastYearDriverWins = (float)rr.LastYearDriverWins;
            LastYearDriverSeconds = (float)rr.LastYearDriverSeconds;
            LastYearDriverThirds = (float)rr.LastYearDriverThirds;
            LastYearDriverWinPercent = (float)rr.LastYearDriverWinPercent;

            DistanceBucket = GetDistanceBucket(race.Distance);
            StartType = (int)race.StartType;
            Distribution = (float)distribution;
            GallopedLast = gallopedLast;
            WinPercentFromTrack = winPercentOnTrackDistance;

            HorseWinPercentThisYear = WinThisYears / StartsThisYear;
            if (DriverStarts > 0)
            {
                DriverWinPercentThisYear = DriverWins / DriverStarts;
                DriverSecondPercentThisYear = DriverSeconds / DriverStarts;
                DriverThirdPercentThisYear = DriverThirds / DriverStarts;
                DriverPlacePercentThisYear = (DriverWins + DriverSeconds + DriverThirds) / DriverStarts;
            }

            if (TrainerStarts > 0)
            {
                TrainerWinPercentThisYear = TrainerWins / TrainerStarts;
                TrainerSecondPercentThisYear = TrainerSeconds / TrainerStarts;
                TrainerThirdPercentThisYear = TrainerThirds / TrainerStarts;
                TrainerPlacePercentThisYear = (TrainerWins + TrainerSeconds + TrainerThirds) / TrainerStarts;
            }
            TimeBehindWinner = rr.TimeBehindWinner;
            TimeAfterWinner = (float)(rr.FinishTimeMilliseconds - race.WinnerFinishTime);
            if (TimeAfterWinner < 0)
            {
                TimeAfterWinner = 0;
                NumFasterThanWinner++;
            }

            InvertedWinOdds = 1.0f / WinOdds;
            DistanceHandicapBucket = GetDistanceHandicapBucket(rr.DistanceHandicap);
        }

        public static int NumFasterThanWinner = 0;
        public static void NormalizeRelative(IEnumerable<RaceResultModel> models)
        {
            var best = models.Min(m => m.BestKmTime);
            var avg = models.Min(m => m.AvgKmTime);
            var median = models.Min(m => m.MedianKmTime);
            foreach(var m in models)
            {
                m.BestSpeedAfterFastest = m.BestKmTime - best;
                m.AvgSpeedAfterFastest = m.AvgKmTime - avg;
                m.MedianSpeedAfterFastest = m.MedianSpeedAfterFastest - avg;
            }
            best = models.Min(m => m.BestSpeedAfterFastest);
            avg = models.Min(m => m.AvgSpeedAfterFastest);
            median = models.Min(m => m.MedianSpeedAfterFastest);

            var bestMax = models.Max(m => m.BestSpeedAfterFastest);
            var avgMax = models.Max(m => m.AvgSpeedAfterFastest);
            var medianMax = models.Max(m => m.MedianSpeedAfterFastest);

            var bestDiff = bestMax - best;
            var avgDiff = avgMax - avg;
            var medianDiff = medianMax - avg;

            foreach (var m in models)
            {
                m.BestSpeedAfterFastest = m.BestSpeedAfterFastest.Normalize(best, bestDiff);
                m.AvgSpeedAfterFastest = m.AvgSpeedAfterFastest.Normalize(avg, avgDiff);
                m.MedianSpeedAfterFastest = m.MedianSpeedAfterFastest.Normalize(median, medianDiff);
            }
            NormalizeProperty(models, (m) => m.TrainerMoneyThisYear, (m, f) => m.TrainerMoneyThisYear = f, (m, f) => m.TrainerMoneyThisYear = f);
            NormalizeProperty(models, (m) => m.TrainerMoneyLastYear, (m, f) => m.TrainerMoneyLastYear = f, (m, f) => m.TrainerMoneyLastYear = f);

            NormalizeProperty(models, (m) => m.TrainerWinPercent, (m, f) => m.TrainerWinPercent = f, (m, f) => m.RelTrainerWinPercent = f);
            NormalizeProperty(models, (m) => m.LastYearTrainerWinPercent, (m, f) => m.LastYearTrainerWinPercent = f, (m, f) => m.RelLastYearTrainerWinPercent = f);
            NormalizeProperty(models, (m) => m.TrainerSecondPercentThisYear, (m, f) => m.TrainerSecondPercentThisYear = f, (m, f) => m.RelTrainerSecondPercentThisYear = f);
            NormalizeProperty(models, (m) => m.TrainerThirdPercentThisYear, (m, f) => m.TrainerThirdPercentThisYear = f, (m, f) => m.RelTrainerThirdPercentThisYear = f);
            NormalizeProperty(models, (m) => m.TrainerPlacePercentThisYear, (m, f) => m.TrainerPlacePercentThisYear = f, (m, f) => m.RelTrainerPlacePercentThisYear = f);

            NormalizeProperty(models, (m) => m.DriverWinPercentThisYear, (m, f) => m.DriverWinPercentThisYear = f, (m, f) => m.RelDriverWinPercentThisYear = f);
            NormalizeProperty(models, (m) => m.LastYearDriverWinPercent, (m, f) => m.LastYearDriverWinPercent = f, (m, f) => m.RelLastYearDriverWinPercent = f);
            NormalizeProperty(models, (m) => m.DriverSecondPercentThisYear, (m, f) => m.DriverSecondPercentThisYear = f, (m, f) => m.RelDriverSecondPercentThisYear = f);
            NormalizeProperty(models, (m) => m.DriverThirdPercentThisYear, (m, f) => m.DriverThirdPercentThisYear = f, (m, f) => m.RelDriverThirdPercentThisYear = f);
            NormalizeProperty(models, (m) => m.DriverPlacePercentThisYear, (m, f) => m.DriverPlacePercentThisYear = f, (m, f) => m.RelDriverPlacePercentThisYear = f);
            NormalizeProperty(models, (m) => m.TrainerPlacePercentThisYear, (m, f) => m.TrainerPlacePercentThisYear = f, (m, f) => m.RelTrainerPlacePercentThisYear = f);

            NormalizeProperty(models, (m) => m.HorseWinPercentThisYear, (m, f) => m.HorseWinPercentThisYear = f, (m, f) => m.RelHorseWinPercentThisYear = f);
            NormalizeProperty(models, (m) => m.HorseSecondPercentThisYear, (m, f) => m.HorseSecondPercentThisYear = f, (m, f) => m.RelHorseSecondPercentThisYear = f);
            NormalizeProperty(models, (m) => m.HorseThirdPercentThisYear, (m, f) => m.HorseThirdPercentThisYear = f, (m, f) => m.RelHorseThirdPercentThisYear = f);
            NormalizeProperty(models, (m) => m.HorsePlacePercentThisYear, (m, f) => m.HorsePlacePercentThisYear = f, (m, f) => m.RelHorsePlacePercentThisYear = f);

            NormalizeProperty(models, (m) => m.MedianKmTime, (m, f) => m.MedianKmTime = f, (m, f) => m.RelMedianKmTime = f);
            NormalizeProperty(models, (m) => m.AvgKmTime, (m, f) => m.AvgKmTime = f, (m, f) => m.RelAvgKmTime = f);
            NormalizeProperty(models, (m) => m.BestKmTime, (m, f) => m.BestKmTime = f, (m, f) => m.RelBestKmTime = f);


        }
        public static void NormalizeProperty(IEnumerable<RaceResultModel> models, Func<RaceResultModel, float> propSelector, Action<RaceResultModel, float> propSetter, Action<RaceResultModel, float> propSetterRel, bool ignoreZeros = false)
        {
            var validModels = models.Where(m => !ignoreZeros || propSelector(m) != 0);
            if (!validModels.Any())
            {
                return;
            }
            var min = models.Min(rrm => propSelector(rrm));
            var max = models.Max(rrm => propSelector(rrm));
            var diff = max - min;
          
            var avg = models.Average(rrm => propSelector(rrm));
            if (avg != 0)
            {
                foreach (var m in models)
                {
                    propSetterRel(m, propSelector(m) / avg);
                }
            }
            else
            {
                foreach (var m in models)
                {
                    propSetterRel(m, 0);
                }
            }
            /*
            if (diff > 0)
            {
                foreach (var m in models)
                {
                    propSetter(m, propSelector(m).Normalize(min, diff));
                }
            }
            else
            {
                foreach (var m in models)
                {
                    propSetter(m, 0.5f);
                }
            }
            
            var avg = models.Average(rrm => propSelector(rrm));
            if (avg != 0)
            {
                foreach (var m in models)
                {
                    propSetterRel(m, propSelector(m) / avg);
                }
            }
            else
            {
                foreach (var m in models)
                {
                    propSetterRel(m, 0);
                }
            }

            min = models.Min(rrm => propSelector(rrm));
            max = models.Max(rrm => propSelector(rrm));
            diff = max - min;
            if (diff > 0)
            {
                foreach (var m in models)
                {
                    propSetter(m, propSelector(m).Normalize(min, diff));
                }
            }
            else
            {
                foreach (var m in models)
                {
                    propSetter(m, 0.5f);
                }
            }
            */
        }
        public static int GetDistanceBucket(int distance)
        {
            if (distance < 1800)
            {
                return 1;
            }
            else if (distance >= 1800 && distance <= 2200)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        public static int GetDistanceHandicapBucket(int handicap)
        {
            if (handicap == 20)
            {
                return 1;
            }
            else if (handicap > 20)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }
        public float TimeBehindWinner { get; set; }
        public float BestSpeedAfterFastest { get; set; }
        public float AvgSpeedAfterFastest { get; set; }
        public float MedianSpeedAfterFastest { get; set; }
        public int DistanceHandicapBucket { get; set; }
        public float InvertedWinOdds { get; set; }
        public float TimeAfterWinner { get; set; }

        public float MedianKmTime { get; set; }
        public float RelMedianKmTime { get; set; }

        public float BestKmTime { get; set; }
        public float RelBestKmTime { get; set; }
        public float AvgKmTime { get; set; }
        public float RelAvgKmTime { get; set; }

        public float RelTrainerWinPercent { get; set; }
        public float RelLastYearTrainerWinPercent { get; set; }
        public float RelLastYearDriverWinPercent { get; set; }
        public float RelHorseWinPercentThisYear { get; set; }
        public float RelHorseSecondPercentThisYear { get; set; }
        public float RelHorseThirdPercentThisYear { get; set; }

        public float RelHorsePlacePercentThisYear { get; set; }

        public float RelDriverWinPercentThisYear { get; set; }
        public float RelDriverSecondPercentThisYear { get; set; }
        public float RelDriverThirdPercentThisYear { get; set; }

        public float RelDriverPlacePercentThisYear { get; set; }

        public float RelTrainerWinPercentThisYear { get; set; }
        public float RelTrainerSecondPercentThisYear { get; set; }
        public float RelTrainerThirdPercentThisYear { get; set; }

        public float RelTrainerPlacePercentThisYear { get; set; }


        public float HorsePlacePercentLastFive { get; set; }
        public float HorseWinPercentLastFive { get; set; }

        public float HorseWinPercentThisYear { get; set; }
        public float HorseSecondPercentThisYear { get; set; }
        public float HorseThirdPercentThisYear { get; set; }

        public float HorsePlacePercentThisYear { get; set; }

        public float DriverWinPercentThisYear { get; set; }
        public float DriverSecondPercentThisYear { get; set; }
        public float DriverThirdPercentThisYear { get; set; }

        public float DriverPlacePercentThisYear { get; set; }

        public float TrainerWinPercentThisYear { get; set; }
        public float TrainerSecondPercentThisYear { get; set; }
        public float TrainerThirdPercentThisYear { get; set; }

        public float TrainerPlacePercentThisYear { get; set; }

        #region Raw features
        public int DistanceBucket { get; set; }
        public int StartType { get; set; }
        public float WinPercentFromTrack { get; set; }
        public bool GallopedLast { get; set; }
        public float Distribution { get; set; }

        public int FinishPosition { get; set; }
        public long HorseId { get; set; }
        //public long? TrainerId { get; set; }
        public long RaceId { get; set; }
        public long DriverId { get; set; }
        public float Track { get; set; }
        public float Position { get; set; }
        public float KmTimeMilliSeconds { get; set; }
        public float WinOdds { get; set; }
        public float PrizeMoney { get; set; }
        public bool DQ { get; set; }
        public bool Galopp { get; set; }
        public bool FrontShoes { get; set; }
        public bool BackShoes { get; set; }
        public bool FrontChange { get; set; }
        public bool BackChange { get; set; }
        public bool Scratched { get; set; }


        public float HorseMoneyTotal { get; set; }
        public float HorseMoneyLastYear { get; set; }
        public float LastYearMoneyPerStart { get; set; }
        public float HorseMoneyThisYear { get; set; }

        public float HorseTotalMoney { get; set; }
        public float HorseTotalWinPercent { get; set; }
        public float HorseTotalPlacepercent { get; set; }

        // This years
        public float StartsThisYear { get; set; }
        public float WinThisYears { get; set; }
        public float SecondsThisYear { get; set; }
        public float ThirdsThisYear { get; set; }

        public float MoneyPerStart { get; set; }
        public float LastYearStarts { get; set; }
        public float LastYearWins { get; set; }
        public float LastYearSeconds { get; set; }
        public float LastYearThirds { get; set; }

        public float TrainerWinPercent { get; set; }
        public float DriverWinPercent { get; set; }
        public float TrainerMoneyThisYear { get; set; }
        public float TrainerMoneyLastYear { get; set; }


        public float TrainerStarts { get; set; }
        public float TrainerWins { get; set; }
        public float TrainerSeconds { get; set; }
        public float TrainerThirds { get; set; }

        public float LastYearTrainerStarts { get; set; }
        public float LastYearTrainerWins { get; set; }
        public float LastYearTrainerSeconds { get; set; }
        public float LastYearTrainerThirds { get; set; }

        public float LastYearTrainerWinPercent { get; set; }

        public float DriverMoneyLastYear { get; set; }
        public float DriverMoney { get; set; }
        public float DriverStarts { get; set; }
        public float DriverWins { get; set; }
        public float DriverSeconds { get; set; }
        public float DriverThirds { get; set; }

        public float LastYearDriverStarts { get; set; }
        public float LastYearDriverWins { get; set; }
        public float LastYearDriverSeconds { get; set; }
        public float LastYearDriverThirds { get; set; }
        public float LastYearDriverWinPercent { get; set; }
        #endregion
    }
}
