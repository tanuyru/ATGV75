using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travsport.WebParser.Json
{

    public class RaceDayJson
    {
        [JsonProperty("organisation")]
        public string Organisation { get; set; }

        [JsonProperty("sourceOfData")]
        public string SourceOfData { get; set; }

        [JsonProperty("trackName")]
        public string TrackName { get; set; }

        [JsonProperty("raceDayDate")]
        public string RaceDayDate { get; set; }

        [JsonProperty("raceDayId")]
        public long RaceDayId { get; set; }

        [JsonProperty("hasPropositions")]
        public bool HasPropositions { get; set; }

        [JsonProperty("hasCorrections")]
        public bool HasCorrections { get; set; }

        [JsonProperty("hasSubmissionsList")]
        public bool HasSubmissionsList { get; set; }

        [JsonProperty("hasOldStartList")]
        public bool HasOldStartList { get; set; }

        [JsonProperty("startListLinkToATG")]
        public string StartListLinkToATG { get; set; }

        [JsonProperty("hasNewStartList")]
        public bool HasNewStartList { get; set; }

        [JsonProperty("numberOfResults")]
        public int NumberOfResults { get; set; }

        [JsonProperty("resultsReady")]
        public bool ResultsReady { get; set; }

        [JsonProperty("driverCorrectionOpen")]
        public bool DriverCorrectionOpen { get; set; }

        [JsonProperty("driverCorrectionOpened")]
        public bool DriverCorrectionOpened { get; set; }

        [JsonProperty("openClosed")]
        public string OpenClosed { get; set; }

        [JsonProperty("mountingAvailable")]
        public bool MountingAvailable { get; set; }


        [JsonProperty("trackProgramExists")]
        public bool TrackProgramExists { get; set; }

        [JsonProperty("shortBetType")]
        public string ShortBetType { get; set; }

        [JsonProperty("betType")]
        public string BetType { get; set; }
    }



}
