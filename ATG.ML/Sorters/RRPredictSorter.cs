using ATG.ML.MLModels;
using ATG.ML.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATG.ML.Sorters
{
    public class RRPredictSorter : ISorter<RaceResultModel>
    {
        private PredictionEngine<RaceResultModel, RaceResultPrediction> _predictor;
        public RRPredictSorter(MLContext mlContext, ITransformer model)
        {
            _predictor = mlContext.Model.CreatePredictionEngine<RaceResultModel, RaceResultPrediction>(model);
        }
        public IOrderedEnumerable<RaceResultModel> Sort(IEnumerable<RaceResultModel> entries, RaceModel race)
        {
            return entries.OrderBy(r => _predictor.Predict(r).TimeAfterWinner);
        }
    }
}
