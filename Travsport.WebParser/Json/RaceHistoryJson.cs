using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travsport.WebParser.Json
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RaceInformation
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("displayDate")]
        public string DisplayDate { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("raceDayId")]
        public long RaceDayId { get; set; }

        [JsonProperty("raceNumber")]
        public int RaceNumber { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }

        [JsonProperty("hasStartList")]
        public bool HasStartList { get; set; }
    }

    public class RaceType
    {
        [JsonProperty("sortValue")]
        public string SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class StartPosition
    {
        [JsonProperty("sortValue")]
        public int SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class Distance
    {
        [JsonProperty("sortValue")]
        public int SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class Placement
    {
        [JsonProperty("sortValue")]
        public int SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class KilometerTime
    {
        [JsonProperty("sortValue")]
        public int SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class Odds
    {
        [JsonProperty("sortValue")]
        public int SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class PrizeMoney
    {
        [JsonProperty("sortValue")]
        public int SortValue { get; set; }

        [JsonProperty("displayValue")]
        public string DisplayValue { get; set; }
    }

    public class ShoeOptions
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class EquipmentOptions
    {
        [JsonProperty("shoeOptions")]
        public ShoeOptions ShoeOptions { get; set; }
    }

    public class RaceHistoryJson
    {
        [JsonProperty("trackCode")]
        public string TrackCode { get; set; }

        [JsonProperty("raceInformation")]
        public RaceInformation RaceInformation { get; set; }

        [JsonProperty("raceType")]
        public RaceType RaceType { get; set; }

        [JsonProperty("startPosition")]
        public StartPosition StartPosition { get; set; }

        [JsonProperty("distance")]
        public Distance Distance { get; set; }

        [JsonProperty("trackCondition")]
        public string TrackCondition { get; set; }

        [JsonProperty("placement")]
        public Placement Placement { get; set; }

        [JsonProperty("kilometerTime")]
        public KilometerTime KilometerTime { get; set; }

        [JsonProperty("startMethod")]
        public string StartMethod { get; set; }

        [JsonProperty("odds")]
        public Odds Odds { get; set; }

        [JsonProperty("shoeInfo")]
        public ShoeInfo ShoeInfo { get; set; }

        [JsonProperty("driver")]
        public DriverJson Driver { get; set; }

        [JsonProperty("trainer")]
        public TrainerJson Trainer { get; set; }

        [JsonProperty("prizeMoney")]
        public PrizeMoney PrizeMoney { get; set; }

        [JsonProperty("equipmentOptions")]
        public EquipmentOptions EquipmentOptions { get; set; }

        [JsonProperty("withdrawn")]
        public bool Withdrawn { get; set; }
    }


}
