using ATG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATG.WebParser
{
    public static class OddsHelper
    {
        public static double FromAmerican(double american)
        {
            if (american > 0)
            {
                return 1 + (american / 100);
            }
            else
            {
                return 1 + (100 / -american);
            }
        }

        public static GameTypeEnum ParseGameType(string gt)
        {
            if (!Enum.TryParse<GameTypeEnum>(gt, out var gtEnum))
            {
                throw new Exception();
            }
            return gtEnum;
        }
    }
}
