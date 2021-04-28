using ATG.Shared.Enums;
using ATG.WebParser.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace ATG.WebParser
{
    public static class Parser
    {
        private static string NoRaceUrl = "https://www.rikstoto.no/api/results/raceDays/{0}/{1}/completeresults";
        private static string RaceExtendedFormat = "https://www.atg.se/services/racinginfo/v1/api/races/{0}/extended";
        private static string UrlFormat = "https://www.atg.se/services/racinginfo/v1/api/games/{0}_{1}";
        private static string UrlTotalFormat = "https://www.atg.se/services/racinginfo/v1/api/games/{0}";
        private static string HorseFormat = "https://www.atg.se/services/racinginfo/v1/api/races/{0}/start/{1}";
        private static string GameDayFormat = "https://www.atg.se/services/racinginfo/v1/api/calendar/day/{0}";
        private static string DateFormat = "yyyy-MM-dd";

        private static DateTime LastCall = DateTime.MinValue;
        private static TimeSpan MinCallDiff = TimeSpan.FromSeconds(0.1f);

        private static Dictionary<string, string> _noArenaToName = new Dictionary<string, string>();
        static Parser()
        {
            _noArenaToName.Add("Bjerke", "BJ_NR");
            _noArenaToName.Add("Forus", "FO_NR");
            _noArenaToName.Add("Sörlandet", "ST_NR");
            _noArenaToName.Add("Biri", "BI_NR");
            _noArenaToName.Add("Övrevoll", "OV_NR");
            _noArenaToName.Add("Momarken", "MO_NR");
            _noArenaToName.Add("Leangen", "LE_NR");
            _noArenaToName.Add("Jarlsberg", "JA_NR");

            _noArenaToName.Add("Harstad", "HA_NR");
            _noArenaToName.Add("Bergen", "BT_NR");
            _noArenaToName.Add("Klosterskogen", "KL_NR");
            _noArenaToName.Add("Bodö", "BD_NR");
        }
        private static string GetJson(string url)
        {
            if (DateTime.Now - LastCall < MinCallDiff)
            {
                var msToWait = MinCallDiff - (DateTime.Now - LastCall);
                Thread.Sleep(msToWait);
            }
            LastCall = DateTime.Now;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.DownloadString(url);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message + " when getting url " + url);
                    return null;
                }
            }
        }

        public static GameJson GetGame(GameTypeEnum gameType, string raceId)
        {
            GameJson game = null;

            var url = string.Format(UrlFormat, gameType.ToString(), raceId);
            var json = GetJson(url);
            try
            {
                game = JsonConvert.DeserializeObject<GameJson>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at deserializing: " + ex.Message + ": " + json);
                return null;
            }

            return game;
        }
        public static NoRaceJson GetNoRaceJson(string arena, DateTime raceDate, int raceNumber)
        {
            if (!_noArenaToName.TryGetValue(arena, out var arenaCode))
            {
                arenaCode = arena.Substring(0, 2) + "_NR";
                Console.WriteLine("Guessed arena code " + arenaCode + " from arena " + arena);
                _noArenaToName.Add(arena, arenaCode);
            }
            var url = string.Format(NoRaceUrl, _noArenaToName[arena]+"_"+raceDate.ToString("yyyy-MM-dd"), raceNumber);
            NoRaceJson raceJson = null;
            var json = GetJson(url);
            try
            {
                raceJson = JsonConvert.DeserializeObject<NoRaceJson>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at deserializing: " + ex.Message + ": " + json);
                return null;
            }
            return raceJson;
        }
        public static RaceJson GetRace(string raceId)
        {
            RaceJson game = null;

            var url = string.Format(RaceExtendedFormat, raceId);
            var json = GetJson(url);
            try
            {
                game = JsonConvert.DeserializeObject<RaceJson>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at deserializing: " + ex.Message + ": " + json);
                return null;
            }

            return game;
        }
        public static GameJson GetGame(string gameId)
        {
            GameJson game = null;
            
                var url = string.Format(UrlTotalFormat, gameId);
                var json = GetJson(url);
                try
                {
                    game = JsonConvert.DeserializeObject<GameJson>(json);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Failed at deserializing: " + ex.Message + ": " + json);
                    return null;
                }
       
            return game;
        }

        public static HorseInfoJson GetHorseRecords(string gameId, int number)
        {
            HorseInfoJson horse;
                var url = string.Format(HorseFormat, gameId, number);
                var json = GetJson(url);
                try
                {
                    horse = JsonConvert.DeserializeObject<HorseInfoJson>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed at deserializing: " + ex.Message + ": " + json);
                    return null;
                }

            
            return horse;
        }


        public static GameDayJson GetGameDay(DateTime date)
        {
            GameDayJson gameDay = null;
            var dateString = date.ToString(DateFormat);
            var url = string.Format(GameDayFormat, dateString);
            var json = GetJson(url);
            try
            {
                gameDay = JsonConvert.DeserializeObject<GameDayJson>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at deserializing: " + ex.Message + ": " + json);
                return null;
            }


            return gameDay;
        }
    }
}
