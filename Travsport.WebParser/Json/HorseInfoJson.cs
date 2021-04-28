using System;
using System.Collections.Generic;
using System.Text;

namespace Travsport.WebParser.Json
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class HorseGender
    {
        public string code { get; set; }
        public string text { get; set; }
    }

    public class HorseBreed
    {
        public string code { get; set; }
        public string text { get; set; }
    }

    public class TrotAdditionalInformation
    {
        public bool mockInlander { get; set; }
        public bool limitedStartPrivileges { get; set; }
        public bool embryo { get; set; }
        public bool offspringStartsExists { get; set; }
        public bool registrationDone { get; set; }
    }

    public class RegistrationStatus
    {
        public bool changeable { get; set; }
        public bool dead { get; set; }
    }



    public class Representative
    {
        public string organisation { get; set; }
        public string sourceOfData { get; set; }
        public int id { get; set; }
        public string personType { get; set; }
        public string name { get; set; }
    }

    public class Messages
    {
        public string trotPremiumChanceMessage { get; set; }
    }

    public class StartMonitoringInformation
    {
        public bool userLoggedIn { get; set; }
        public bool startMonitoringPossible { get; set; }
    }

    public class HorseInfoJson
    {
        public string organisation { get; set; }
        public string sourceOfData { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public HorseGender horseGender { get; set; }
        public HorseBreed horseBreed { get; set; }
        public string color { get; set; }
        public string registrationNumber { get; set; }
        public string uelnNumber { get; set; }
        public string passport { get; set; }
        public string registrationCountryCode { get; set; }
        public string bredCountryCode { get; set; }
        public string birthCountryCode { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string dateOfBirthDisplayValue { get; set; }
        public bool guestHorse { get; set; }
        public OwnerJson owner { get; set; }
        public BreederJson breeder { get; set; }
        public TrainerJson trainer { get; set; }
        public Representative representative { get; set; }
        public Messages messages { get; set; }
        public bool offspringExists { get; set; }
        public bool resultsExists { get; set; }
        public bool historyExists { get; set; }
        public bool breedingEvaluationExists { get; set; }
        public string sportInfoType { get; set; }
    }


}
