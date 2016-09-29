using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Console = Colorful.Console;

namespace ConversionEdvardas
{
    public class Data
    {
        public static List<string> SearchEngines = new List<string> { "google" };
        public static List<string> SocialMediaSites = new List<string> { "facebook" };
        public static List<string> ReferringSites = new List<string> { "orai.lt" };

        public const int Impression = 1;
        public const int Click = 2;
        public const int Unload = 4;
        public const int Event = 21;
        public const int TrackingPoint = 100;

        public static readonly TimeSpan ImpressionLifeSpan = TimeSpan.FromDays(7);
        public static readonly TimeSpan ClickLifeSpan = TimeSpan.FromDays(30);
        public static readonly TimeSpan RecentAdInteractionSpan = TimeSpan.FromSeconds(30);

        public static Dictionary<int, string> TransTypeName = new Dictionary<int, string>
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
            if (ReferringSites.Any(s => url.Contains(s))) return "Referring site";
            return "Referrer unknown";
        }


        public static bool IsTransLead(Transaction trans)
        {
            return trans.TransactionType == TrackingPoint && trans.LogPointName.ToLower().Contains("thank you");
        }


        public static Transaction DecideAttribution(Transaction oldTrans, Transaction newTrans)
        {
            if (oldTrans == null) return newTrans;

            if (oldTrans.TransactionType == Impression || newTrans.TransactionType == Click)
                return newTrans;
            return oldTrans;
        }


        public static HashSet<int> CookiesWithConversion(List<Transaction> transactions)
        {
            var cookies = new HashSet<int>();
            foreach (var trans in transactions)
            {
                if (trans.TransactionType == TrackingPoint && trans.LogPointName.ToLower().Contains("thank you"))
                    cookies.Add(trans.CookieId);
            }
            return cookies;
        }

        public static Transaction GetAttributedTransOfPath(List<Transaction> transactions, Transaction logPoint)
        {
            var targetTime = logPoint.LogTime;
            var filtered = transactions.Where(a =>
                (a.TransactionType == Impression && targetTime - a.LogTime <  ImpressionLifeSpan)
                ||
                (a.TransactionType == Click && targetTime - a.LogTime < ClickLifeSpan));

            Transaction bestTransaction = null;
            foreach (var transaction in filtered)
            {
                bestTransaction = DecideAttribution(bestTransaction, transaction);
            }
             return bestTransaction;
        }
    }
}