using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Colorful;
using Newtonsoft.Json;

namespace ConversionEdvardas
{
    public class Data
    {
        public static List<string> SearchEngines = new List<string> { "google" };
        public static List<string> SocialMediaSites = new List<string> { "facebook" };
        //public static List<string> ReferringSites = new List<string> { "orai.lt" };


        public static Dictionary<int, string> TransTypes = new Dictionary<int, string>
        {
            {1, "Impression"},
            {2, "Click"},
            {21, "Event"},
            {4, "Unload"},
            {100, "Tracking point"}
        };

        public static List<int> LeadIds = new List<int> { 1001, 3240 };


        public static List<Transaction> ReadJson(string filename = "transactions.txt")
        {
            string json;

            using (var r = new StreamReader(filename))
            {
                json = r.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<List<Transaction>>(json);
        }


        public static void PrintPathInfo(List<ConversionPath> allPaths)
        {
            foreach (var conversionPath in allPaths)
            {
                Console.WriteLine(
                       "---------------------------------------------------------------------------------------------------------");
                Console.WriteLine(conversionPath.GetAggregatedPath(), Color.WhiteSmoke);
                Console.WriteLine();
                conversionPath.Print();
            }
        }


        public static string GetReferrerType(Transaction trans)
        {
            var url = trans.UrLfrom;
            if (url == null) return "Direct link";
            if (SearchEngines.Any(s => url.Contains(s))) return "Natural search";
            if (SocialMediaSites.Any(s => url.Contains(s))) return "Social media";
            return "Referring site";
        }


        public static bool IsTransLead(Transaction trans)
        {
            return trans.TransactionType == 100 && trans.LogPointName.ToLower().Contains("thank you");
        }


        public static Transaction DecideAttribution(Transaction first, Transaction second)
        {
            if (first == null) return second;
            // Assume only transaction types 1 and 2 supplied
            if (first.TransactionType == 1 || second.TransactionType == 2)
                return second;
            return first;
        }


        public static HashSet<int> CookiesWithConversion(List<Transaction> transactions)
        {
            var cookies = new HashSet<int>();
            foreach (var trans in transactions)
            {
                if (trans.TransactionType == 100 && trans.LogPointName.ToLower().Contains("thank you"))
                    cookies.Add(trans.CookieId);
            }
            return cookies;
        }
    }
}