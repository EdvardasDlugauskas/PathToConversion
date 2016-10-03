using System.Collections.Generic;
using System.Linq;
using Colorful;

namespace ConversionEdvardas
{
    class Program
    {
        static void Main()
        {
            var transactions = Data.ReadJson("bigdata.txt");

            var allPaths = GetConversionPaths(transactions);
            
            Data.PrintPathInfo(allPaths);

            Console.ReadLine();
        }


        private static List<ConversionPath> GetConversionPaths(List<Transaction> transactions)
        {
            var allPaths = new List<ConversionPath>();

            var clientAndCookies = Data.CookiesWithConversion(transactions);

            foreach (var client in clientAndCookies)
            {
                var filteredByClient = transactions.Where(a => a.ClientSite == client.Key).ToList();
                var cookies = client.Value;
                foreach (var cookie in cookies)
                {
                    var stack = new List<Transaction>();
                    foreach (var trans in filteredByClient.Where(a => a.CookieId == cookie).OrderBy(a=>a.LogTime))
                    {
                        stack.Add(trans);
                        if (Data.IsTransLead(trans))
                            allPaths.Add(new ConversionPath(stack));
                    }

                }
            }
            return allPaths;
        }

    }
}