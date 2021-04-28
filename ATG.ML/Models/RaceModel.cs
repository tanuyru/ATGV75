using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.ML.Models
{
    public class RaceModel
    {
        public StartTypeEnum StartType { get; set; }
        public int Distance { get; set; }
        public string Arena { get; set; }
    }
}
