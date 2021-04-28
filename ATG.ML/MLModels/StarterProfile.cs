using ATG.DB.Entities;
using ATG.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TS = Travsport.DB.Entities;
namespace ATG.ML.MLModels
{
    public class StarterProfile
    {
        public static TimeSpan ShapeLength = TimeSpan.FromDays(30);
        public static TimeSpan TrainerShapeLength = TimeSpan.FromDays(30);
        public static int GetDistanceBucket(int distance)
        {
            if (distance < 1800)
            {
                return 1;
            }
            else if (distance >= 1800 && distance <= 2300)
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
        public TS.RaceResult TSResult;
        public RaceResult Result { get; set; }
        public int DistanceHandicapBucket
        {
            get
            {
                if (Result != null)
                    return Result.DistanceHandicap;
                if (TSResult != null)
                    return TSResult.DistanceHandicap;
                throw new Exception();
            }
        }
        public int PositionInDistance
        {
            get
            {
                if (Result != null)
                    return Result.Position;
                if (TSResult != null)
                    return TSResult.PositionForDistance;
                throw new Exception();
            }
        }
        public bool IsValid { get; set; }
        public StarterProfile()
        { }
        public StarterProfile(IEnumerable<StartHistory> history, IEnumerable<StartHistory> trainerHistory, IEnumerable<StartHistory> driverHistory, TS.Race race, TS.RaceResult result)
        {
            Distribution = (float)result.Distribution;
            HorseId = result.HorseId;
            TSResult = result;
            var shapeCutoffDate = race.StartTime.Add(-ShapeLength);
            float total = (float)history.Count();
            if (total == 0)
            {
                WinOdds =(float) result.WinOdds;
                IsValid = false;
                return;
            }
            int won = history.Count(s => s.PlacePosition == 1);
            int place = history.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4);

            float totMoney = history.Sum(s => s.PrizeMoneyWon);
            var shapeHistory = history.Where(s => s.RaceDate >= shapeCutoffDate);
            float totShape = (float)shapeHistory.Count();
            int wonInShape = shapeHistory.Count(s => s.PlacePosition == 1);
            int placeInShape = shapeHistory.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4);
            
            float moneyWonInShape = shapeHistory.Sum(s => s.PrizeMoneyWon);
            int wonOnStart = history.Count(s => s.PlacePosition == 1 && s.StartType == race.StartType);
            int placeOnStart = history.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4 && s.StartType == race.StartType);
            float totOnStart = (float)history.Count(s => s.StartType == race.StartType);

            int distanceBucket = GetDistanceBucket(race.Distance);
            bool missingNumber = false;
          

            WinRate = won / (total);
            PlaceRate = place / (float)total;
            MoneyPerRace = totMoney / total;
            if (totShape > 0)
            {
                WinShape = wonInShape / totShape;
                PlaceShape = placeInShape / totShape;
                MoneyShape = moneyWonInShape / totShape;
                TimeAfterWinnerShape = shapeHistory.Average(sh => sh.TimeAfterWinner);
            }
            else
            {
                missingNumber = true;
            }
            var validHistory = history.Where(s => s.KmTimeMilliseconds > 0 && !s.DQ && !s.Galloped && GetDistanceBucket(s.Distance) == distanceBucket);
            if (!validHistory.Any())
            {
                validHistory = history.Where(s => s.KmTimeMilliseconds > 0 && !s.DQ && !s.Galloped);
            }

            if (validHistory.Any())
            {
                var bestTime = validHistory.Min(s => s.KmTimeMilliseconds);
                var medianTime = (float)validHistory.Select(s => s.KmTimeMilliseconds).GetMedian();
                BestTimeOnDistance = bestTime;
                MedianTimeOnDistance = medianTime;
            }
            else
            {
                missingNumber = true;
            }
            var historyOnDistance = history.Where(s => GetDistanceBucket(s.Distance) == distanceBucket);
            float totOnDistance = (float)historyOnDistance.Count();
            int wonOnDistance = historyOnDistance.Count(s => s.PlacePosition == 1);
            int placeOnDistance = historyOnDistance.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4);

            if (totOnDistance > 0)
            {
                DistanceWinRate = wonOnDistance / totOnDistance;
                DistancePlaceRate = placeOnDistance / totOnDistance;
            }
            else
            {
                //missingNumber = true;
            }
            if (totOnStart > 0)
            {
                StartTypeWinRate = wonOnStart / totOnStart;
                StartTypePlaceRate = placeOnStart / totOnStart;
            }
            else
            {
                // missingNumber = true;
            }
            /*
            var handicapBucket = GetDistanceHandicapBucket(result.DistanceHandicap);
            var validOnHandicap = history.Where(s => GetDistanceHandicapBucket(s.Handicap) == handicapBucket);
            if (validOnHandicap.Any())
            {

            }*/
            if (result.FinishTimeMilliseconds > 0)
            {
                TimeAfterWinner = (float)(result.FinishTimeMilliseconds - race.WinnerFinishTime);
            }
            else
            {
                missingNumber = true;
            }
            DateTime lastYearStart = new DateTime(race.StartTime.Year - 1, 1, 1);
            DateTime thisYearStart = new DateTime(race.StartTime.Year, 1, 1);

            

          
                var lastYearDriverHistory = driverHistory.Where(sh => sh.RaceDate >= lastYearStart && sh.RaceDate < thisYearStart);
                int driverWinsLastYear = lastYearDriverHistory.Count(sh => sh.PlacePosition == 1);
                int driverPlaceLastYear = lastYearDriverHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                float totDriverLastYear = lastYearDriverHistory.Count();
                if (totDriverLastYear > 0)
                {
                    DriverPlaceRateLastYear = driverPlaceLastYear / totDriverLastYear;
                    DriverWinRateLastYear = driverWinsLastYear / totDriverLastYear;
                }
            
         
                var thisYearDriverHistory = driverHistory.Where(sh => sh.RaceDate >= thisYearStart && sh.RaceDate < race.StartTime);
                driverWinsLastYear = thisYearDriverHistory.Count(sh => sh.PlacePosition == 1);
                driverPlaceLastYear = thisYearDriverHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                totDriverLastYear = thisYearDriverHistory.Count();
                if (totDriverLastYear > 0)
                {
                    DriverPlaceRateThisYear = driverPlaceLastYear / totDriverLastYear;
                    DriverWinRateThisYear = driverWinsLastYear / totDriverLastYear;
                }
            
         
                var lastYearTrainerHistory = trainerHistory.Where(sh => sh.RaceDate >= lastYearStart && sh.RaceDate < thisYearStart);
                 driverWinsLastYear = lastYearTrainerHistory.Count(sh => sh.PlacePosition == 1);
                 driverPlaceLastYear = lastYearTrainerHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                 totDriverLastYear = lastYearTrainerHistory.Count();
                if (totDriverLastYear > 0)
                {
                    TrainerPlaceRateLastYear = driverPlaceLastYear / totDriverLastYear;
                    TrainerWinRateLastYear = driverWinsLastYear / totDriverLastYear;
                }
            
         
                var thisYearTrainerHistory = trainerHistory.Where(sh => sh.RaceDate >= thisYearStart && sh.RaceDate < race.StartTime);
                driverWinsLastYear = thisYearTrainerHistory.Count(sh => sh.PlacePosition == 1);
                driverPlaceLastYear = thisYearTrainerHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                totDriverLastYear = thisYearTrainerHistory.Count();
                if (totDriverLastYear > 0)
                {
                    TrainerPlaceRateThisYear = driverPlaceLastYear / totDriverLastYear;
                    TrainerWinRateThisYear = driverWinsLastYear / totDriverLastYear;
                }
           
            var trainerShapeDate = race.StartTime.Add(-TrainerShapeLength);
            var trainerShape = trainerHistory.Where(sh => sh.RaceDate > trainerShapeDate);

            int totTrainerShape = trainerShape.Count();
            if (totTrainerShape > 0)
            {
                int totTrainerShapeWin = trainerShape.Count(sh => sh.PlacePosition == 1);
                int totTrainerShapePlace = trainerShape.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);

                TrainerPlaceShape = totTrainerShapePlace / (float)totTrainerShape;
                TrainerWinShape = totTrainerShapeWin / (float)totTrainerShape;
            }

            WinOdds = (float)result.WinOdds;
            FinishPosition = result.FinishPosition;
            IsValid = !missingNumber;

            RaceNumber = race.RaceOrder;
        }

        public StarterProfile(IEnumerable<StartHistory> history, IEnumerable<StartHistory> trainerHistory, IEnumerable<StartHistory> driverHistory, Race race, RaceResult result)
        {
            Distribution = (float) result.Distribution;
            HorseId = result.HorseId;
            Result = result;
            var shapeCutoffDate = race.StartTime.Add(-ShapeLength);
            float total = (float)history.Count();
            if (total == 0)
            {
                throw new Exception("No history, cant create profile");
            }
            int won = history.Count(s => s.PlacePosition == 1);
            int place = history.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4);

            float totMoney = history.Sum(s => s.PrizeMoneyWon);
            var shapeHistory = history.Where(s => s.RaceDate >= shapeCutoffDate);
            float totShape = (float)shapeHistory.Count();
            int wonInShape = shapeHistory.Count(s => s.PlacePosition == 1);
            int placeInShape = shapeHistory.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4);
            float moneyWonInShape = shapeHistory.Sum(s => s.PrizeMoneyWon);
            int wonOnStart = history.Count(s => s.PlacePosition == 1 && s.StartType == race.StartType);
            int placeOnStart = history.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4 && s.StartType == race.StartType);
            float totOnStart = (float)history.Count(s => s.StartType == race.StartType);

            int distanceBucket = GetDistanceBucket(race.Distance);
            bool missingNumber = false;
            WinRate = won / (total);
            PlaceRate = place / (float)total;
            MoneyPerRace = totMoney / total;
            if (totShape > 0)
            {
                WinShape = wonInShape / totShape;
                PlaceShape = placeInShape / totShape;
                MoneyShape = moneyWonInShape / totShape;
            }
            var validHistory = history.Where(s => s.KmTimeMilliseconds > 0 && !s.DQ && !s.Galloped && GetDistanceBucket(s.Distance) == distanceBucket);
            if (!validHistory.Any())
            {
                validHistory = history.Where(s => s.KmTimeMilliseconds > 0 && !s.DQ && !s.Galloped);
            }
            
            if (validHistory.Any())
            {
                var bestTime = validHistory.Min(s => s.KmTimeMilliseconds);
                var medianTime = (float)validHistory.Select(s => s.KmTimeMilliseconds).GetMedian();
                BestTimeOnDistance = bestTime;
                MedianTimeOnDistance = medianTime;
            }
            else
            {
                missingNumber = true;
            }
            var historyOnDistance = history.Where(s => GetDistanceBucket(s.Distance) == distanceBucket);
            float totOnDistance = (float)historyOnDistance.Count();
            int wonOnDistance = historyOnDistance.Count(s => s.PlacePosition == 1);
            int placeOnDistance = historyOnDistance.Count(s => s.PlacePosition > 0 && s.PlacePosition < 4);

            if (totOnDistance > 0)
            {
                DistanceWinRate = wonOnDistance / totOnDistance;
                DistancePlaceRate = placeOnDistance / totOnDistance;
            }
            else
            {
                //missingNumber = true;
            }
            if (totOnStart > 0)
            {
                StartTypeWinRate = wonOnStart / totOnStart;
                StartTypePlaceRate = placeOnStart / totOnStart;
            }
            else
            {
               // missingNumber = true;
            }
            /*
            var handicapBucket = GetDistanceHandicapBucket(result.DistanceHandicap);
            var validOnHandicap = history.Where(s => GetDistanceHandicapBucket(s.Handicap) == handicapBucket);
            if (validOnHandicap.Any())
            {

            }*/
            if (result.FinishTimeMilliseconds > 0)
            {
                TimeAfterWinner = (float)(result.FinishTimeMilliseconds - race.WinnerFinishTime);
            }
            else
            {
                missingNumber = true;
            }
            DateTime lastYearStart = new DateTime(race.StartTime.Year - 1, 1, 1);
            DateTime thisYearStart = new DateTime(race.StartTime.Year, 1, 1);
            DriverWinRateLastYear = (float)result.LastYearDriverWinPercent/10000.0f;
            DriverWinRateThisYear = (float)result.DriverWinPercent / 10000.0f;
           

            if (result.LastYearDriverStarts > 0)
            {
                DriverPlaceRateLastYear = (float)((result.LastYearDriverThirds + result.LastYearDriverSeconds + result.LastYearDriverWins) / (float)result.LastYearDriverStarts);
            }
            else
            {
                var lastYearDriverHistory = driverHistory.Where(sh => sh.RaceDate >= lastYearStart && sh.RaceDate < thisYearStart);
                int driverWinsLastYear = lastYearDriverHistory.Count(sh => sh.PlacePosition == 1);
                int driverPlaceLastYear = lastYearDriverHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                float totDriverLastYear = lastYearDriverHistory.Count();
                if (totDriverLastYear > 0)
                {
                    DriverPlaceRateLastYear = driverPlaceLastYear / totDriverLastYear;
                    DriverWinRateLastYear = driverWinsLastYear / totDriverLastYear;
                }
            }
            if (result.DriverStarts > 0)
            {
                DriverPlaceRateThisYear = (float)((result.DriverWins + result.DriverSeconds + result.DriverThirds) / (float)result.DriverStarts);
            }
            else
            {
                var thisYearDriverHistory = driverHistory.Where(sh => sh.RaceDate >= thisYearStart && sh.RaceDate < race.StartTime);
                int driverWinsLastYear = thisYearDriverHistory.Count(sh => sh.PlacePosition == 1);
                int driverPlaceLastYear = thisYearDriverHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                float totDriverLastYear = thisYearDriverHistory.Count();
                if (totDriverLastYear > 0)
                {
                    DriverPlaceRateThisYear = driverPlaceLastYear / totDriverLastYear;
                    DriverWinRateThisYear = driverWinsLastYear / totDriverLastYear;
                }
            }
            TrainerWinRateLastYear = (float)result.LastYearTrainerWinPercent / 10000.0f;
            TrainerWinRateThisYear = (float)result.TrainerWinPercent / 10000.0f;
            if (result.LastYearTrainerStarts > 0)
            {
                TrainerPlaceRateLastYear = (float)((result.LastYearTrainerThirds + result.LastYearTrainerSeconds + result.LastYearTrainerWins) / (float)result.LastYearTrainerStarts);
            }
            else
            {
                var lastYearTrainerHistory = trainerHistory.Where(sh => sh.RaceDate >= lastYearStart && sh.RaceDate < thisYearStart);
                int driverWinsLastYear = lastYearTrainerHistory.Count(sh => sh.PlacePosition == 1);
                int driverPlaceLastYear = lastYearTrainerHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                float totDriverLastYear = lastYearTrainerHistory.Count();
                if (totDriverLastYear > 0)
                {
                    TrainerPlaceRateLastYear = driverPlaceLastYear / totDriverLastYear;
                    TrainerWinRateLastYear = driverWinsLastYear / totDriverLastYear;
                }
            }
            if (result.TrainerStarts > 0)
            {
                TrainerPlaceRateThisYear = (float)((result.TrainerWins + result.TrainerSeconds + result.TrainerThirds) / (float)result.TrainerStarts);
            }
            else
            {
                var thisYearTrainerHistory = trainerHistory.Where(sh =>  sh.RaceDate >= thisYearStart && sh.RaceDate < race.StartTime);
                int driverWinsLastYear = thisYearTrainerHistory.Count(sh => sh.PlacePosition == 1);
                int driverPlaceLastYear = thisYearTrainerHistory.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);
                float totDriverLastYear = thisYearTrainerHistory.Count();
                if (totDriverLastYear > 0)
                {
                    TrainerPlaceRateThisYear = driverPlaceLastYear / totDriverLastYear;
                    TrainerWinRateThisYear = driverWinsLastYear / totDriverLastYear;
                }
            }
            var trainerShapeDate = race.StartTime.Add(-TrainerShapeLength);
            var trainerShape = trainerHistory.Where(sh => sh.RaceDate > trainerShapeDate);

            int totTrainerShape = trainerShape.Count();
            if (totTrainerShape > 0)
            {
                int totTrainerShapeWin = trainerShape.Count(sh => sh.PlacePosition == 1);
                int totTrainerShapePlace = trainerShape.Count(sh => sh.PlacePosition > 0 && sh.PlacePosition < 4);

                TrainerPlaceShape = totTrainerShapePlace / (float)totTrainerShape;
                TrainerWinShape = totTrainerShapeWin / (float)totTrainerShape;
            }

            WinOdds = (float) result.WinOdds;
            FinishPosition = result.FinishPosition;
            IsValid = !missingNumber;
        }

        public float TimeAfterWinnerShape { get; set; }

        public int RaceNumber { get; set; }
        public float Distribution { get; set; }
        public int FinishPosition { get; set; }
        public float WinOdds { get; set; }
        public long HorseId { get; set; }
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

        public float FinishTimeTotal { get; set; }

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

        public float TimeAfterWinner { get; set; }
    }
}
