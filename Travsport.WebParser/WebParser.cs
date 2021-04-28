using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Travsport.WebParser.Json;
using Travsport.WebParser.Json.StartListJson;

namespace Travsport.WebParser
{
    public class WebParser
    {
        private const string RaceDayUrl = "https://api.travsport.se/webapi/raceinfo/results/organisation/TROT/sourceofdata/SPORT/racedayid/{0}";
        private const string RaceDayOverviewUrl = "https://api.travsport.se/webapi/raceinfo/organisation/TROT/sourceofdata/SPORT?fromracedate={0}&tosubmissiondate={1}&toracedate={1}";
        private const string HorseHistoryUrl = "https://api.travsport.se/webapi/horses/results/organisation/TROT/sourceofdata/SPORT/horseid/{0}";
        private const string HorseInfoUrlr = "https://api.travsport.se/webapi/horses/basicinformation/organisation/TROT/sourceofdata/SPORT/horseid/{0}";
        private const string RaceDayStartListUrl = "https://api.travsport.se/webapi/raceinfo/startlists/organisation/TROT/sourceofdata/SPORT/racedayid/{0}";

        private const string DateFormat = "yyyy-MM-dd";
        private static DateTime LastCall = DateTime.MinValue;
        private static TimeSpan MinCallDiff = TimeSpan.FromMilliseconds(100);
        private static TimeSpan Randomize = TimeSpan.FromMilliseconds(25);
        private static bool SaveToDisk = true;
        private const string SavePath = @"G:\Travsport\";
        private const string HorsePath = @"G:\Travsport\Horses\";
        private const string DataPath = @"G:\Travsport\Data\";
        private const string ImportedPath = @"G:\Travsport\Imported\";
        private const string ImportedPath2 = @"G:\Travsport\Imported\ReallyImported\";
        private static Random random = new Random();
        private static string GetJson(string url)
        {
            var r = random.NextDouble();

            var diff = DateTime.Now - LastCall;
            if (diff < MinCallDiff.Add(Randomize*r))
            {
                var msToWait = MinCallDiff.Add(Randomize * r) - (diff);
                if (msToWait > TimeSpan.Zero)
                {
                    Thread.Sleep(msToWait);
                }
            }
            LastCall = DateTime.Now;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.DownloadString(url);
                }
                catch(WebException webEx)
                {
                    Console.WriteLine(DateTime.Now+": WEBEX Status: " + webEx.Status + " Message: " + webEx.Message+" getting "+url);
                    
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " when getting url " + url);
                    return null;
                }
            }
        }
        public static StartListRootJson GetStartList(long id)
        {
            var url = string.Format(RaceDayStartListUrl, id);
            var json = GetJson(url);
            if (json == null)
                return null;
            StartListRootJson root = JsonConvert.DeserializeObject<StartListRootJson>(json);
            return root;
        }
        public static HorseInfoJson GetHorseInfo(long id)
        {
            if (File.Exists(HorsePath + id + ".txt"))
            {
                var fileJson = File.ReadAllText(HorsePath + id + ".txt");
                var fileInfo = JsonConvert.DeserializeObject<HorseInfoJson>(fileJson);
                return fileInfo;
            }
            var url = string.Format(HorseInfoUrlr, id);
            var json = GetJson(url);
            if (json == null)
                return null;
            HorseInfoJson info = JsonConvert.DeserializeObject<HorseInfoJson>(json);
            if (SaveToDisk)
            {
                File.WriteAllText(HorsePath + id + ".txt", json);
            }
            return info;
        }
        public static List<RaceHistoryJson> GetHorseHistory(long id)
        {
            var url = string.Format(HorseHistoryUrl, id);
            var json = GetJson(url);
            if (json == null)
                return new List<RaceHistoryJson>();
            List<RaceHistoryJson> history = JsonConvert.DeserializeObject< List<RaceHistoryJson>>(json);
            return history;
        }
        public static List<RaceDayJson> GetRaceDays(DateTime from, DateTime to)
        {
            if (false && File.Exists(DataPath + "history2.txt"))
            {
                var fileJson = File.ReadAllText(DataPath + "history2.txt");
                List<RaceDayJson> fileList = JsonConvert.DeserializeObject<List<RaceDayJson>>(fileJson);
                return fileList;
            }
            var url = string.Format(RaceDayOverviewUrl, from.ToString(DateFormat), to.ToString(DateFormat));
            var json = GetJson(url);
            if (json == null)
            {
                return new List<RaceDayJson>();
            }
            List<RaceDayJson> list = JsonConvert.DeserializeObject<List<RaceDayJson>>(json);
            File.WriteAllText(SavePath + "history.txt", json);
            return list;
        }

        public static List<RaceDayResultJson> LoadResultsFromDiskReallyImported(IEnumerable<long> racedayIds)
        {
            var files = Directory.EnumerateFiles(ImportedPath2);
            List<RaceDayResultJson> results = new List<RaceDayResultJson>();
            foreach (var id in racedayIds)
            {
                var filename = @"\" + id + ".txt";
                var file = files.SingleOrDefault(f => f.EndsWith(filename));
                if (file == null)
                    continue;
                var fileJson = File.ReadAllText(file);
                var fileResult = JsonConvert.DeserializeObject<RaceDayResultJson>(fileJson);
                fileResult.FileName = file;
                results.Add(fileResult);
            }
            return results;
        }
        public static List<RaceDayResultJson> LoadResultsFromDisk(IEnumerable<long> racedayIds)
        {
            var files = Directory.EnumerateFiles(ImportedPath);
            List<RaceDayResultJson> results = new List<RaceDayResultJson>();
            foreach(var id in racedayIds)
            {
                var filename = @"\"+id + ".txt";
                var file = files.SingleOrDefault(f => f.EndsWith(filename));
                if (file == null)
                    continue;
                var fileJson = File.ReadAllText(file);
                var fileResult = JsonConvert.DeserializeObject<RaceDayResultJson>(fileJson);
                fileResult.FileName = file;
                results.Add(fileResult);
            }
            return results;
        }
        public static RaceDayResultJson GetResults(long id, string trackName)
        {
            var filename = SavePath + id + ".txt";
            var importFilename = ImportedPath + id + ".txt";
            if (File.Exists(filename))
            {
                var fileJson = File.ReadAllText(filename);
                var fileResult = JsonConvert.DeserializeObject<RaceDayResultJson>(fileJson);
                return fileResult;
            }
            if (File.Exists(importFilename))
            {
                return null;
                var fileJson = File.ReadAllText(filename);
                var fileResult = JsonConvert.DeserializeObject<RaceDayResultJson>(fileJson);
                return fileResult;
            }
            var url = string.Format(RaceDayUrl, id);
            var json = GetJson(url);
            if (json == null)
            {
                return null;
            }
            var results = JsonConvert.DeserializeObject<RaceDayResultJson>(json);
            results.CreatedTrackName = trackName;
            if (SaveToDisk)
            {
                File.WriteAllText(SavePath + id+".txt", json);
            }
            return results;
        }

        public static List<RaceDayResultJson> LoadFromDisk(int numToTake)
        {
            List<RaceDayResultJson> results = new List<RaceDayResultJson>();
            var files = Directory.EnumerateFiles(SavePath);
            foreach(var file in files.Take(numToTake))
            {
                var json = File.ReadAllText(file);
                var r = JsonConvert.DeserializeObject<RaceDayResultJson>(json);
                r.FileName = file;
                results.Add(r);
            }
            return results;
        }
        public static int MoveFiles(IEnumerable<string> filenames)
        {
            int counter = 0;
            Console.WriteLine("Moving " + filenames.Count() + " files");
            foreach (var file in filenames)
            {
                var filePartName = Path.GetFileName(file);
                try
                {
                    File.Move(file, ImportedPath + filePartName);
                    counter++;
                }
                catch(Exception)
                {

                }
            }
            Console.WriteLine("Moved " + counter + " files");
            return counter;
        }

        public static int MoveFiles2(IEnumerable<string> filenames)
        {
            int counter = 0;
            Console.WriteLine("Moving " + filenames.Count() + " files");
            foreach (var file in filenames)
            {
                var filePartName = Path.GetFileName(file);
                try
                {
                    File.Move(file, ImportedPath2 + filePartName);
                    counter++;
                }
                catch (Exception)
                {

                }
            }
            Console.WriteLine("Moved " + counter + " files");
            return counter;
        }
    }
}
