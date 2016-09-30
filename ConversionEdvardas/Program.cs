using System.Collections.Generic;
using System.Linq;
using Colorful;

namespace ConversionEdvardas
{
    class Program
    {
        static void Main()
        {
            var transactions = Data.ReadJson("test.txt");

            var allPaths = GetConversionPaths(transactions);
            
            Data.PrintPathInfo(allPaths);

            Console.ReadLine();
        }


        private static List<ConversionPath> GetConversionPaths(List<Transaction> transactions)
        {
            var allPaths = new List<ConversionPath>();

            foreach (var cookie in Data.CookiesWithConversion(transactions))
                    // <cookieId, client>
            {
                var filteredTransactions = transactions.Where(a => a.CookieId == cookie.Key && a.ClientSite == cookie.Value).OrderBy(a => a.LogTime);

                var stack = new List<Transaction>();

                foreach (var trans in filteredTransactions)
                {
                    stack.Add(trans);
                    if (Data.IsTransLead(trans))
                    {
                        allPaths.Add(new ConversionPath(stack));
                    }
                }
            }
            return allPaths;
        }

    }
}