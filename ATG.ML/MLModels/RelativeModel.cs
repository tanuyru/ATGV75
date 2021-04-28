using ATG.ML.Models;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.MLModels
{
    public class RelativeResult
    {
        [ColumnName("Score")]
        public float TimeAfterWinner { get; set; }
    }
    public class RelativeModel
    {
        public enum CompareMethod
        {
            Ratio,
            Diff,
        }
        public enum RelMethod
        {
            Average,
            Median
        }
        public enum MissingValueMethod
        {
            None,
            Average,
            Median,
        }
        public static RelMethod Rel = RelMethod.Median;
        public static CompareMethod Compare = CompareMethod.Diff;
        public RelativeModel()
        { }

        public RelativeModel(IEnumerable<StarterProfile> profiles, StarterProfile horse, float positionWinRate)
        {
            //Distribution = horse.Distribution;
            Func<Func<StarterProfile, float>, IEnumerable<StarterProfile>, MissingValueMethod, float> funcer = null;
            if (Rel == RelMethod.Average)
            {
                funcer = GetAverage;
            }
            else if (Rel == RelMethod.Median)
            {
                funcer = GetMedian;
            }
            WinRate = funcer((s) => s.WinRate, profiles, MissingValueMethod.None);
            PlaceRate = funcer((s) => s.PlaceRate, profiles, MissingValueMethod.None);
            MoneyPerRace = funcer((s) => s.MoneyPerRace, profiles, MissingValueMethod.None);
            WinShape = funcer((s) => s.WinShape, profiles, MissingValueMethod.None);
            PlaceShape = funcer((s) => s.PlaceShape, profiles, MissingValueMethod.None);
            MoneyShape = funcer((s) => s.MoneyShape, profiles, MissingValueMethod.None);
            BestTimeOnDistance = funcer((s) => s.BestTimeOnDistance, profiles, MissingValueMethod.Median);
            MedianTimeOnDistance = funcer((s) => s.MedianTimeOnDistance, profiles, MissingValueMethod.Median);
            DistanceWinRate = funcer((s) => s.DistanceWinRate, profiles, MissingValueMethod.None);
            DistancePlaceRate = funcer((s) => s.DistancePlaceRate, profiles, MissingValueMethod.None);
            StartTypeWinRate = funcer((s) => s.StartTypeWinRate, profiles, MissingValueMethod.Median);
            StartTypePlaceRate = funcer((s) => s.StartTypePlaceRate, profiles, MissingValueMethod.Median);
            TimeAfterWinnerShape = funcer((s) => s.TimeAfterWinnerShape, profiles, MissingValueMethod.Median);

            DriverWinRateThisYear = funcer((s) => s.DriverWinRateThisYear, profiles, MissingValueMethod.None);
            DriverPlaceRateThisYear = funcer((s) => s.DriverPlaceRateThisYear, profiles, MissingValueMethod.None);
            DriverWinRateLastYear = funcer((s) => s.DriverWinRateLastYear, profiles, MissingValueMethod.Median);
            DriverPlaceRateLastYear = funcer((s) => s.DriverPlaceRateLastYear, profiles, MissingValueMethod.Median);
            TrainerWinRateThisYear = funcer((s) => s.TrainerWinRateThisYear, profiles, MissingValueMethod.None);
            TrainerPlaceRateThisYear = funcer((s) => s.TrainerPlaceRateThisYear, profiles, MissingValueMethod.None);
            TrainerWinRateLastYear = funcer((s) => s.TrainerWinRateLastYear, profiles, MissingValueMethod.None);
            TrainerPlaceRateLastYear = funcer((s) => s.TrainerPlaceRateLastYear, profiles, MissingValueMethod.Median);
            TrainerWinShape = funcer((s) => s.TrainerWinShape, profiles, MissingValueMethod.None);
            TrainerPlaceShape = funcer((s) => s.TrainerPlaceShape, profiles, MissingValueMethod.None);


            if (Compare == CompareMethod.Diff)
            {
                WinRate = horse.WinRate - WinRate;
                PlaceRate = horse.PlaceRate - PlaceRate;
                MoneyPerRace = horse.MoneyPerRace - MoneyPerRace;
                WinShape = horse.WinShape - WinShape;
                PlaceShape = horse.PlaceShape - PlaceShape;
                MoneyShape = horse.MoneyShape - MoneyShape;
                BestTimeOnDistance = horse.BestTimeOnDistance - BestTimeOnDistance;
                MedianTimeOnDistance = horse.MedianTimeOnDistance - MedianTimeOnDistance;
                DistanceWinRate = horse.DistanceWinRate - DistanceWinRate;
                DistancePlaceRate = horse.DistancePlaceRate - DistancePlaceRate;
                StartTypeWinRate = horse.StartTypeWinRate - StartTypeWinRate;
                StartTypePlaceRate = horse.StartTypePlaceRate - StartTypePlaceRate;

                DriverWinRateThisYear = horse.DriverWinRateThisYear - DriverWinRateThisYear;
                DriverPlaceRateThisYear = horse.DriverPlaceRateThisYear - DriverPlaceRateThisYear;
                DriverWinRateLastYear = horse.DriverWinRateLastYear - DriverWinRateLastYear;
                DriverPlaceRateLastYear = horse.DriverPlaceRateLastYear - DriverPlaceRateLastYear;
                TrainerWinRateThisYear = horse.TrainerWinRateThisYear - TrainerWinRateThisYear;
                TrainerPlaceRateThisYear = horse.TrainerPlaceRateThisYear - TrainerPlaceRateThisYear;
                TrainerWinRateLastYear = horse.TrainerWinRateLastYear - TrainerWinRateLastYear;
                TrainerPlaceRateLastYear = horse.TrainerPlaceRateLastYear - TrainerPlaceRateLastYear;
                TrainerWinShape = horse.TrainerWinShape - TrainerWinShape;
                TrainerPlaceShape = horse.TrainerPlaceShape - TrainerPlaceShape;
                TimeAfterWinnerShape = horse.TimeAfterWinnerShape - TimeAfterWinnerShape;
            }
            else if (Compare == CompareMethod.Ratio)
            {
                if (WinRate > 0)
                    WinRate = horse.WinRate/WinRate;
                if (PlaceRate > 0)
                    PlaceRate = horse.PlaceRate / PlaceRate;
                if (MoneyPerRace > 0)
                    MoneyPerRace = horse.MoneyPerRace / MoneyPerRace;
                if (WinShape > 0)
                    WinShape = horse.WinShape / WinShape;
                if (PlaceShape > 0)
                    PlaceShape = horse.PlaceShape / PlaceShape;
                if (MoneyShape > 0)
                    MoneyShape = horse.MoneyShape / MoneyShape;
                if (BestTimeOnDistance > 0)
                    BestTimeOnDistance = horse.BestTimeOnDistance / BestTimeOnDistance;
                if (MedianTimeOnDistance > 0)
                    MedianTimeOnDistance = horse.MedianTimeOnDistance / MedianTimeOnDistance;
                if (DistanceWinRate > 0)
                    DistanceWinRate = horse.DistanceWinRate / DistanceWinRate;
                if (DistancePlaceRate > 0)
                    DistancePlaceRate = horse.DistancePlaceRate / DistancePlaceRate;
                if (StartTypeWinRate > 0)
                    StartTypeWinRate = horse.StartTypeWinRate / StartTypeWinRate;
                if (StartTypePlaceRate > 0)
                    StartTypePlaceRate = horse.StartTypePlaceRate / StartTypePlaceRate;

                if (DriverWinRateThisYear > 0)
                    DriverWinRateThisYear = horse.DriverWinRateThisYear / DriverWinRateThisYear;
                if (DriverPlaceRateThisYear > 0)
                    DriverPlaceRateThisYear = horse.DriverPlaceRateThisYear  / DriverPlaceRateThisYear;
                if (DriverWinRateLastYear > 0)
                    DriverWinRateLastYear = horse.DriverWinRateLastYear / DriverWinRateLastYear;
                if (DriverPlaceRateLastYear > 0)
                    DriverPlaceRateLastYear = horse.DriverPlaceRateLastYear / DriverPlaceRateLastYear;
                if (TrainerWinRateThisYear > 0)
                    TrainerWinRateThisYear = horse.TrainerWinRateThisYear / TrainerWinRateThisYear;
                if (TrainerPlaceRateThisYear > 0)
                    TrainerPlaceRateThisYear = horse.TrainerPlaceRateThisYear / TrainerPlaceRateThisYear;
                if (TrainerWinRateLastYear > 0)
                    TrainerWinRateLastYear = horse.TrainerWinRateLastYear / TrainerWinRateLastYear;
                if (TrainerPlaceRateLastYear > 0)
                    TrainerPlaceRateLastYear = horse.TrainerPlaceRateLastYear / TrainerPlaceRateLastYear;
                if (TrainerWinShape > 0)
                    TrainerWinShape = horse.TrainerWinShape / TrainerWinShape;
                if (TrainerPlaceShape > 0)
                    TrainerPlaceShape = horse.TrainerPlaceShape / TrainerPlaceShape;
                if (TimeAfterWinnerShape > 0)
                    TimeAfterWinnerShape = horse.TimeAfterWinnerShape / TimeAfterWinnerShape;


            }
            AvgWinRateFromStartPosition = positionWinRate;
            TimeAfterWinner = horse.TimeAfterWinner;
            if (TimeAfterWinner < 0)
                TimeAfterWinner = 0;


            var platsProfiles = profiles.Where(sp => sp.FinishPosition > 0);
            if (platsProfiles.Any())
            {
                var maxForPlats = platsProfiles.Max(p => p.TimeAfterWinner);
                //TimeAfterWinnerPlatsMax = Math.Min(maxForPlats, TimeAfterWinner);
            }
            NumHorses = profiles.Count();

        }
        private float GetAverage(Func<StarterProfile, float> propSel, IEnumerable<StarterProfile> profiles, MissingValueMethod miss)
        {
            int numMissing = profiles.Count(p => propSel(p) == 0);

            if (miss == MissingValueMethod.None || numMissing == 0)
            {
                return profiles.Select(p => propSel(p)).Average();
            }
            var valids = profiles.Where(p => propSel(p) != 0).Select(p => propSel(p));

            if (!valids.Any())
                return 0;
            float valToAdd = 0;
            if (miss == MissingValueMethod.Average)
            {
                valToAdd = valids.Average();
            }
            else if (miss == MissingValueMethod.Median)
            {
                valToAdd = valids.GetMedian();
            }
            var list = valids.ToList();
            for (int i = 0; i < numMissing; i++)
            {
                list.Add(valToAdd);
            }
            return list.Average();
        }
        private float GetMedian(Func<StarterProfile,  float> propSel, IEnumerable<StarterProfile> profiles, MissingValueMethod miss)
        {
            int numMissing = profiles.Count(p => propSel(p) == 0);

            if (miss == MissingValueMethod.None || numMissing == 0)
            {
                return profiles.Select(p => propSel(p)).GetMedian();
            }
            var valids = profiles.Where(p => propSel(p) != 0).Select(p => propSel(p));
            if (!valids.Any())
                return 0;
            float valToAdd = 0;
            if (miss == MissingValueMethod.Average)
            {
                valToAdd = valids.Average();
            }
            else if (miss == MissingValueMethod.Median)
            {
                valToAdd = valids.GetMedian();
            }
            var list = valids.ToList();
            for(int i = 0; i < numMissing;i++)
            {
                list.Add(valToAdd);
            }
            return list.GetMedian();
        }
       
        public float Distribution { get; set; }
        public float WinRate { get; set; }
        public float PlaceRate { get; set; }
        public float MoneyPerRace { get; set; }

        public float WinShape { get; set; }
        public float PlaceShape { get; set; }
        public float MoneyShape { get; set; }

        public float BestTimeOnDistance { get; set; }
        public float MedianTimeOnDistance { get; set; }



        public float DistanceWinRate { get; set; }

        public float DistancePlaceRate { get; set; }

        public float StartTypeWinRate { get; set; }
        public float StartTypePlaceRate { get; set; }

        public float AvgWinRateFromStartPosition { get; set; }

        public float DriverWinRateThisYear { get; set; }
        public float DriverPlaceRateThisYear { get; set; }

        public float DriverWinRateLastYear { get; set; }
        public float DriverPlaceRateLastYear { get; set; }

        public float TrainerWinRateThisYear { get; set; }
        public float TrainerPlaceRateThisYear { get; set; }

        public float TrainerWinRateLastYear { get; set; }
        public float TrainerPlaceRateLastYear { get; set; }

        public float TrainerWinShape { get; set; }
        public float TrainerPlaceShape { get; set; }

        public float NumHorses { get; set; }

        public float TimeAfterWinnerShape { get; set; }
        public float TimeAfterWinner { get; set; }

        // public float TimeAfterWinnerPlatsMax { get; set; }
    }
}
