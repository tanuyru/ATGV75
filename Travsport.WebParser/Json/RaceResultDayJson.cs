using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATG.Shared.Enums;
using Newtonsoft.Json;
namespace Travsport.WebParser.Json
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class RacesWithBasicInfoAndResultStatu
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("numberDisplay")]
        public string NumberDisplay { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("resultReady")]
        public bool ResultReady { get; set; }
    }

    public class GeneralInfo
    {
        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("raceNumber")]
        public int RaceNumber { get; set; }

        [JsonProperty("heading")]
        public string Heading { get; set; }

        [JsonProperty("trackConditions")]
        public string TrackConditions { get; set; }

        [JsonProperty("victoryMargin")]
        public string VictoryMargin { get; set; }

        [JsonProperty("tempoText")]
        public string TempoText { get; set; }

        [JsonProperty("infoTextRows")]
        public List<string> InfoTextRows { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }
    }

    public class HorseJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class DriverJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("licenseId")]
        public long LicenseId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class TrainerJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("licenseId")]
        public int LicenseId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class ShoeInfo
    {
        [JsonProperty("front")]
        public bool Front { get; set; }

        [JsonProperty("back")]
        public bool Back { get; set; }

        [JsonProperty("sortValue")]
        public int SortValue { get; set; }
    }

    public class ShoeOption
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class SulkyOption
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class EquipmentSelection
    {
        [JsonProperty("shoeOption")]
        public ShoeOption ShoeOption { get; set; }

        [JsonProperty("sulkyOption")]
        public SulkyOption SulkyOption { get; set; }
    }

    public class CanceledDriver
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class RaceResultRow
    {
        [JsonIgnore]
        public int RaceDistance { get; set; }
        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("placementDisplay")]
        public string PlacementDisplay { get; set; }

        public int ParsedProgramNumber()
        {
            if (string.IsNullOrEmpty(ProgramNumber))
                return 0;
            if (int.TryParse(ProgramNumber, out int pn))
                return pn;
            return 0;
        }
        [JsonProperty("programNumber")]
        public string ProgramNumber { get; set; }

        [JsonProperty("horse")]
        public HorseJson Horse { get; set; }

        [JsonProperty("driver")]
        public DriverJson Driver { get; set; }

        [JsonProperty("driverChanged")]
        public bool DriverChanged { get; set; }

        [JsonProperty("trainer")]
        public TrainerJson Trainer { get; set; }

        [JsonProperty("shoeInfo")]
        public ShoeInfo ShoeInfo { get; set; }

        public int DistanceParsed()
        {
            if (string.IsNullOrEmpty(StartPositionAndDistance))
                return 0;
            var parts = StartPositionAndDistance.Split('/');
            if (parts.Length != 2)
                return 0;
            if (int.TryParse(parts[1], out var dist))
                return dist;
            return 0;
            
        }
        public int PositionParsed()
        {
            if (string.IsNullOrEmpty(StartPositionAndDistance))
                return 0;
            var parts = StartPositionAndDistance.Split('/');
            if (parts.Length != 2)
                return 0;
            if (int.TryParse(parts[0], out var pos))
                return pos;
            return 0;

        }
        [JsonProperty("startPositionAndDistance")]
        public string StartPositionAndDistance { get; set; }

        [JsonIgnore]
        public double FinishTime { get; set; }
        [JsonIgnore]
        public long KmTimeMilliseconds { get; set; }
        public double SecondsTime()
        {
            if (string.IsNullOrEmpty(Time))
                return 0;
            var trimmed = Time.Trim('g').Trim('a');
            if (double.TryParse(trimmed, out var dTime))
                return dTime;
            return 0;
        }
        [JsonProperty("time")]
        public string Time { get; set; }

        public double DoubleOdds()
        {
            if (string.IsNullOrEmpty(Odds))
                return 0;
            var str = Odds;
            if (str.StartsWith("("))
            {
                str = str.Trim('(').Trim(')');
                if (!double.TryParse(str, out var number))
                {
                    return 0;
                }
                return number / 10.0;
            }
            if (double.TryParse(Odds, out var d))
                return d;
            return 0;
        }
        public double DoublePlatsOdds()
        {
            if (string.IsNullOrEmpty(PlatsOdds))
                return 0;
            var str = PlatsOdds;
            if (str.StartsWith("("))
            {
                str = str.Trim('(').Trim(')');
                if (!double.TryParse(str, out var number))
                {
                    return 0;
                }
                return number / 10.0;
            }
            if (double.TryParse(PlatsOdds, out var d))
                return d;
            return 0;
        }
        [JsonProperty("odds")]
        public string Odds { get; set; }

        [JsonProperty("platsOdds")]
        public string PlatsOdds { get; set; }
        [JsonProperty("placementNumber")]
        public int PlacementNumber { get; set; }

        [JsonProperty("equipmentSelection")]
        public EquipmentSelection EquipmentSelection { get; set; }

        [JsonProperty("prizeMoney")]
        public int PrizeMoney { get; set; }

        [JsonProperty("canceledDriver")]
        public CanceledDriver CanceledDriver { get; set; }
    }

    public class WithdrawnHors
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("programNumber")]
        public string ProgramNumber { get; set; }
        public int ParsedProgramNumber()
        {
            if (string.IsNullOrEmpty(ProgramNumber))
                return 0;
            if (int.TryParse(ProgramNumber, out int pn))
                return pn;
            return 0;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cause")]
        public string Cause { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class PropositionDetailRow
    {
        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class OwnerJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class BreederJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class Father
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class Mother
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class MothersFather
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class WinnerHors
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("colorAndGenderCode")]
        public string ColorAndGenderCode { get; set; }

        [JsonProperty("owner")]
        public OwnerJson Owner { get; set; }

        [JsonProperty("breeder")]
        public BreederJson Breeder { get; set; }

        [JsonProperty("father")]
        public Father Father { get; set; }

        [JsonProperty("mother")]
        public Mother Mother { get; set; }

        [JsonProperty("mothersFather")]
        public MothersFather MothersFather { get; set; }
    }

    public class Penalty
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("licenseId")]
        public long LicenseId { get; set; }

        [JsonProperty("licenseName")]
        public string LicenseName { get; set; }

        [JsonProperty("horseId")]
        public long HorseId { get; set; }

        [JsonProperty("cause")]
        public string Cause { get; set; }

        [JsonProperty("linkableDriver")]
        public bool LinkableDriver { get; set; }

        [JsonProperty("linkableTrainer")]
        public bool LinkableTrainer { get; set; }
    }

    public class DopingTestedHors
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class ProhibitedHors
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("programNumber")]
        public string ProgramNumber { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cause")]
        public string Cause { get; set; }

        [JsonProperty("linkable")]
        public bool Linkable { get; set; }
    }

    public class RacesWithReadyResult
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceId")]
        public long RaceId { get; set; }

        [JsonProperty("generalInfo")]
        public GeneralInfo GeneralInfo { get; set; }

        [JsonProperty("raceResultRows")]
        public List<RaceResultRow> RaceResultRows { get; set; }

        [JsonProperty("withdrawnHorses")]
        public List<WithdrawnHors> WithdrawnHorses { get; set; }

        public StartTypeEnum StartType()
        {
            if (PropositionDetailRows == null)
                return StartTypeEnum.Unknown;
            var tRow = PropositionDetailRows.SingleOrDefault(dr => dr.Type == "T");
            if (tRow == null)
            {
                return StartTypeEnum.Unknown;
            }
            if (tRow.Text.Contains("Autostart"))
            {
                return StartTypeEnum.Auto;
            }
            return StartTypeEnum.Volt;
        }
        [JsonProperty("propositionDetailRows")]
        public List<PropositionDetailRow> PropositionDetailRows { get; set; }

        [JsonProperty("winnerHorses")]
        public List<WinnerHors> WinnerHorses { get; set; }

        [JsonProperty("penalties")]
        public List<Penalty> Penalties { get; set; }

        [JsonProperty("dopingTestedHorses")]
        public List<DopingTestedHors> DopingTestedHorses { get; set; }

        [JsonProperty("prohibitedHorses")]
        public List<ProhibitedHors> ProhibitedHorses { get; set; }
    }

    public class RaceDayResultJson
    {
        [JsonProperty("createdTrackName")]

        public string CreatedTrackName { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("raceDayId")]
        public long RaceDayId { get; set; }

        [JsonProperty("heading")]
        public string Heading { get; set; }

        [JsonProperty("raceDayInfo")]
        public string RaceDayInfo { get; set; }

        [JsonProperty("attendance")]
        public string Attendance { get; set; }

        [JsonProperty("prizeMoney")]
        public string PrizeMoney { get; set; }

    
        [JsonProperty("nextRaceDayId")]
        public long NextRaceDayId { get; set; }

        [JsonProperty("nextRaceDayText")]
        public string NextRaceDayText { get; set; }

        [JsonProperty("previousRaceDayId")]
        public long PreviousRaceDayId { get; set; }

        [JsonProperty("previousRaceDayText")]
        public string PreviousRaceDayText { get; set; }

        [JsonProperty("racesWithBasicInfoAndResultStatus")]
        public List<RacesWithBasicInfoAndResultStatu> RacesWithBasicInfoAndResultStatus { get; set; }

        [JsonProperty("racesWithReadyResult")]
        public List<RacesWithReadyResult> RacesWithReadyResult { get; set; }
    }


}
