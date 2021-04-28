using ATG.ML.MLModels;
using ATG.ML.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class PredictSorter : ISorter<TimeAfterWinnerModel>
    {
        private PredictionEngine<TimeAfterWinnerModel, TimeResult> _predictor;
        public PredictSorter(MLContext mlContext, ITransformer model)
        {
            _predictor = mlContext.Model.CreatePredictionEngine<TimeAfterWinnerModel, TimeResult>(model);
        }
        public IOrderedEnumerable<TimeAfterWinnerModel> Sort(IEnumerable<TimeAfterWinnerModel> entries, RaceModel race)
        {
            return entries.OrderBy(time => _predictor.Predict(time).TimeAfterWinner);
        }
    }
}
