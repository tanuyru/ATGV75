using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travsport.WebParser.Json.StartListJson
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RaceType
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Driver
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("licenseId")]
        public int LicenseId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hasStatistics")]
        public bool HasStatistics { get; set; }
    }

    public class Trainer
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("licenseId")]
        public int LicenseId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hasStatistics")]
        public bool HasStatistics { get; set; }

        [JsonProperty("hasTrainingList")]
        public bool HasTrainingList { get; set; }
    }

    public class HorseGender
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class SulkyOption
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class ShoeOption
    {
        [JsonProperty("code")]
        public int Code { get; set; }
    }

    public class PreviousShoeOption
    {
        [JsonProperty("code")]
        public int Code { get; set; }
    }

    public class PreviousSulkyOption
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class Hors
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("age")]
        public string Age { get; set; }

        [JsonProperty("priceSum")]
        public int PriceSum { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("driver")]
        public DriverJson Driver { get; set; }

        [JsonProperty("trainer")]
        public TrainerJson Trainer { get; set; }

        [JsonProperty("breeder")]
        public BreederJson Breeder { get; set; }

        [JsonProperty("owner")]
        public OwnerJson Owner { get; set; }

        [JsonProperty("startPosition")]
        public int StartPosition { get; set; }

        [JsonProperty("programNumber")]
        public int ProgramNumber { get; set; }

        [JsonProperty("programNumberDisplay")]
        public string ProgramNumberDisplay { get; set; }

        [JsonProperty("horseGender")]
        public HorseGender HorseGender { get; set; }

        [JsonProperty("actualDistance")]
        public int ActualDistance { get; set; }

        [JsonProperty("sulkyOption")]
        public SulkyOption SulkyOption { get; set; }

        [JsonProperty("shoeOption")]
        public ShoeOption ShoeOption { get; set; }

        [JsonProperty("previousShoeOption")]
        public PreviousShoeOption PreviousShoeOption { get; set; }

        [JsonProperty("horseWithdrawn")]
        public bool HorseWithdrawn { get; set; }

        [JsonProperty("driverChanged")]
        public bool DriverChanged { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }

        [JsonProperty("previousSulkyOption")]
        public PreviousSulkyOption PreviousSulkyOption { get; set; }

        [JsonProperty("reasonForWithdrawn")]
        public string ReasonForWithdrawn { get; set; }
    }

    public class PropText
    {
        [JsonProperty("propositionId")]
        public int PropositionId { get; set; }

        [JsonProperty("propositionNumber")]
        public string PropositionNumber { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("typ")]
        public string Typ { get; set; }
    }

    public class RaceList
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceNumber")]
        public int RaceNumber { get; set; }

        [JsonProperty("raceId")]
        public int RaceId { get; set; }

        [JsonProperty("propositionId")]
        public int PropositionId { get; set; }

        [JsonProperty("distance")]
        public int Distance { get; set; }

        [JsonProperty("trackConditions")]
        public string TrackConditions { get; set; }

        [JsonProperty("raceType")]
        public RaceType RaceType { get; set; }

        [JsonProperty("horses")]
        public List<Hors> Horses { get; set; }

        [JsonProperty("propTexts")]
        public List<PropText> PropTexts { get; set; }

        [JsonProperty("startDateTime")]
        public DateTime StartDateTime { get; set; }

        [JsonProperty("resultsReady")]
        public bool ResultsReady { get; set; }
    }

    public class StartListRootJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceDayId")]
        public int RaceDayId { get; set; }

        [JsonProperty("firstStart")]
        public DateTime FirstStart { get; set; }

        [JsonProperty("raceList")]
        public List<RaceList> RaceList { get; set; }

        [JsonProperty("prevRaceDayId")]
        public int PrevRaceDayId { get; set; }

        [JsonProperty("nextRaceDayId")]
        public int NextRaceDayId { get; set; }

        [JsonProperty("trackName")]
        public string TrackName { get; set; }

        [JsonProperty("raceStandard")]
        public string RaceStandard { get; set; }

        [JsonProperty("raceDayDate")]
        public string RaceDayDate { get; set; }

        [JsonProperty("hasPdf")]
        public bool HasPdf { get; set; }
    }


}
