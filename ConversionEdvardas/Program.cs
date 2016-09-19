using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Drawing;
using Console = Colorful.Console;

namespace ConversionEdvardas
{
    public class Transaction
    {
        // http://json2csharp.com/
        public DateTime LogTime { get; set; }
        public int TransactionType { get; set; }
        public int CookieID { get; set; }
        public bool CookiesEnabled { get; set; }
        public string Campaign { get; set; }
        public string Media { get; set; }
        public string Banner { get; set; }
        public string TrackingSetup { get; set; }
        public int? ID_LogPoints { get; set; }
        public string LogPointName { get; set; }
        public string URLfrom { get; set; }
        public string URLto { get; set; }
        public string ClientSite { get; set; }

        public void inheritProperties(Transaction other)
        {
            Banner = other.Banner;
            Campaign = other.Campaign;
            Media = other.Media;
        }

        public override string ToString()
        {
            return $"|{LogTime, 22}|{TransTypes[TransactionType], -15}|{Campaign, -15}|" +
                $"{Media, -10}|{Banner, -18}|{LogPointName, -14}|";
        }

        public void Print()
        {
            var color = Color.White;
            switch (TransactionType)
            {
                case 1:
                    color = Color.Aquamarine;
                    break;
                case 2:
                    color = Color.Aqua;
                    break;
                case 100:
                    color = Color.Yellow;
                    break;
            }
            if (LogPointName == "Thank You") color = Color.LawnGreen;

            Console.WriteLine(ToString(), color);
        }

        public static Dictionary<int, string> TransTypes = new Dictionary<int, string>
        {
            {1, "Impression"},
            {2, "Click"},
            {21, "Event"},
            {4, "Unload"},
            {100, "Tracking point" }
        };
    }

    class Program
    {
        static void Main()
        {
            /* // For reading json from file:
                using (StreamReader r = new StreamReader("transactions.txt"))
                {
                    string json = r.ReadToEnd();
                }
             */
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);

            // Holds data about last important transaction (ex. Click or Impression)
            Dictionary<int, Transaction> attributeToDict = new Dictionary<int, Transaction>();

            Dictionary<int, List<Transaction>> allStacks = new Dictionary<int, List<Transaction>>();

            Dictionary<int, List<List<Transaction>>> completeConversions = new Dictionary<int, List<List<Transaction>>>();

            foreach (var transaction in transactions)
            {
                var currentCookieId = transaction.CookieID;
                List<Transaction> currentStack;
                try
                {
                    currentStack = allStacks[currentCookieId];
                }
                catch (KeyNotFoundException)
                {
                    allStacks[currentCookieId] = new List<Transaction>();
                    currentStack = allStacks[currentCookieId];
                }
                if (!attributeToDict.ContainsKey(currentCookieId) || attributeToDict[currentCookieId] == null)
                {
                    attributeToDict[currentCookieId] = transaction;
                }
                else if (transaction.TransactionType == 2) 
                {
                    attributeToDict[currentCookieId] = transaction;
                }
                else if (transaction.TransactionType == 1 && attributeToDict[currentCookieId].TransactionType != 2)
                {
                    attributeToDict[currentCookieId] = transaction;
                }
                currentStack.Add(transaction);
                if (transaction.TransactionType == 100) 
                {
                    try
                    {
                        transaction.inheritProperties(attributeToDict[currentCookieId]);
                    }
                    catch (KeyNotFoundException){ };

                    if (transaction.LogPointName == "Thank You")
                    {
                        try
                        {
                            completeConversions[currentCookieId].Add(currentStack);
                        }
                        catch (KeyNotFoundException)
                        {
                            completeConversions[currentCookieId] = new List<List<Transaction>> {currentStack};
                        }
                        allStacks[currentCookieId] = new List<Transaction>();
                        attributeToDict[currentCookieId] = null;
                    }
                } 
            }

            foreach (var paths in completeConversions)
            {
                Console.WriteLine($"\r\nConversion paths for cookie with id `{paths.Key}`:", Color.IndianRed);
                foreach (var path in paths.Value)
                {
                    foreach (var trans in path)
                        trans.Print();

                    Console.WriteLine("------------------------------------------------------------------------------------------------------");
                }
            }
        }

        private static string json =
            // Impression -> Impression -> Click -> Log -> Log -> Log -> Conversion
@"[{
	""LogTime"": ""2016-09-14 20:15:18"",
	""TransactionType"": 1,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": ""Summer Campaign"",
	""Media"": ""Delfi.lt"",
	""Banner"": ""Expanding 200x300"",
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": null,
	""LogPointName"": null,
	""URLfrom"": null,
	""URLto"": null,
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-14 21:16:20"",
	""TransactionType"": 1,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": ""Winter Campaign"",
	""Media"": ""15min.lt"",
	""Banner"": ""Expanding 250x300"",
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": null,
	""LogPointName"": null,
	""URLfrom"": null,
	""URLto"": null,
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-15 12:01:20"",
	""TransactionType"": 2,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": ""Summer Campaign"",
	""Media"": ""alfa.lt"",
	""Banner"": ""Standard banner"",
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": null,
	""LogPointName"": null,
	""URLfrom"": null,
	""URLto"": null,
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:16:20"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3245,
	""LogPointName"": ""Titulinis"",
	""URLfrom"": ""www.alfa.lt/sportas"",
	""URLto"": ""www.cocacola.lt"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:20:21"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3246,
	""LogPointName"": ""Produktai"",
	""URLfrom"": ""www.cocacola.lt"",
	""URLto"": ""www.cocacola.lt/products"",
	""ClientSite"": ""Coca-Cola"",
},"
+    // New Cookie in the middle
@"
{
	""LogTime"": ""2016-09-16 21:20:25"",
	""TransactionType"": 2,
	""CookieID"": ""254257100"",
	""CookiesEnabled"": 1,
	""Campaign"": ""Summer Campaign"",
	""Media"": ""alfa.lt"",
	""Banner"": ""Standard banner"",
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": null,
	""LogPointName"": null,
	""URLfrom"": null,
	""URLto"": null,
	""ClientSite"": ""Coca-Cola"",
},"
+   // First path continued
@"
{
	""LogTime"": ""2016-09-16 21:23:21"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3241,
	""LogPointName"": ""Prefill order"",
	""URLfrom"": ""www.cocacola.lt/products"",
	""URLto"": ""www.cocacola.lt/orders"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:26:01"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3240,
	""LogPointName"": ""Thank You"",
	""URLfrom"": ""www.cocacola.lt/orders"",
	""URLto"": ""www.cocacola.lt/thankyoupage"",
	""ClientSite"": ""Coca-Cola"",
},"
+   // New path without clicks. First cookie.
    // Impression -> Impression -> Impression -> Log -> Log -> Log -> Conversion
@"
{
	""LogTime"": ""2016-09-14 20:15:18"",
	""TransactionType"": 1,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": ""Summer Campaign"",
	""Media"": ""Delfi.lt"",
	""Banner"": ""Expanding 200x300"",
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": null,
	""LogPointName"": null,
	""URLfrom"": null,
	""URLto"": null,
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-14 21:16:20"",
	""TransactionType"": 1,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": ""Summer Campaign"",
	""Media"": ""15min.lt"",
	""Banner"": ""Expanding 250x300"",
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": null,
	""LogPointName"": null,
	""URLfrom"": null,
	""URLto"": null,
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:16:20"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3245,
	""LogPointName"": ""Titulinis"",
	""URLfrom"": ""www.alfa.lt/sportas"",
	""URLto"": ""www.cocacola.lt"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:20:21"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3246,
	""LogPointName"": ""Produktai"",
	""URLfrom"": ""www.cocacola.lt"",
	""URLto"": ""www.cocacola.lt/products"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:23:21"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3241,
	""LogPointName"": ""Prefill order"",
	""URLfrom"": ""www.cocacola.lt/products"",
	""URLto"": ""www.cocacola.lt/orders"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:26:01"",
	""TransactionType"": 100,
	""CookieID"": ""254257132"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3240,
	""LogPointName"": ""Thank You"",
	""URLfrom"": ""www.cocacola.lt/orders"",
	""URLto"": ""www.cocacola.lt/thankyoupage"",
	""ClientSite"": ""Coca-Cola"",
},"
+ // Path of second cookie continued. (Click -> ) Log -> Log -> Conversion
@"
{
	""LogTime"": ""2016-09-16 21:22:21"",
	""TransactionType"": 100,
	""CookieID"": ""254257100"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3246,
	""LogPointName"": ""Produktai"",
	""URLfrom"": ""www.cocacola.lt"",
	""URLto"": ""www.cocacola.lt/products"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:23:21"",
	""TransactionType"": 100,
	""CookieID"": ""254257100"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3241,
	""LogPointName"": ""Prefill order"",
	""URLfrom"": ""www.cocacola.lt/products"",
	""URLto"": ""www.cocacola.lt/orders"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:26:01"",
	""TransactionType"": 100,
	""CookieID"": ""254257100"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3240,
	""LogPointName"": ""Thank You"",
	""URLfrom"": ""www.cocacola.lt/orders"",
	""URLto"": ""www.cocacola.lt/thankyoupage"",
	""ClientSite"": ""Coca-Cola"",
},"
+   //  Third cookie. No clicks or impressions. Log -> Log -> Conversion
@"
{
	""LogTime"": ""2016-09-16 21:20:21"",
	""TransactionType"": 100,
	""CookieID"": ""123"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3246,
	""LogPointName"": ""Produktai"",
	""URLfrom"": ""www.cocacola.lt"",
	""URLto"": ""www.cocacola.lt/products"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:23:21"",
	""TransactionType"": 100,
	""CookieID"": ""123"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3241,
	""LogPointName"": ""Prefill order"",
	""URLfrom"": ""www.cocacola.lt/products"",
	""URLto"": ""www.cocacola.lt/orders"",
	""ClientSite"": ""Coca-Cola"",
},
{
	""LogTime"": ""2016-09-16 21:26:01"",
	""TransactionType"": 100,
	""CookieID"": ""123"",
	""CookiesEnabled"": 1,
	""Campaign"": null,
	""Media"": null,
	""Banner"": null,
	""TrackingSetup"": ""Coca-Cola - Tracking"",
	""ID_LogPoints"": 3240,
	""LogPointName"": ""Thank You"",
	""URLfrom"": ""www.cocacola.lt/orders"",
	""URLto"": ""www.cocacola.lt/thankyoupage"",
	""ClientSite"": ""Coca-Cola"",
}
]";
    }


}
