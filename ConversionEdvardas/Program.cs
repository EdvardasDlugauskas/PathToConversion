using System.Collections.Generic;
using System.Linq;

namespace ConversionEdvardas
{
    class Program
    {
        static void Main()
        {
            var transactions = Data.ReadJson();

            var allPaths = GetConversionPaths(transactions);
            
           Data.PrintPathInfo(allPaths); 
        }


        private static List<ConversionPath> GetConversionPaths(List<Transaction> transactions)
        {
            var allPaths = new List<ConversionPath>();

            foreach (var cookie in Data.CookiesWithConversion(transactions))
            {
                var filteredTransactions = transactions.Where(a => a.CookieId == cookie).OrderBy(a => a.LogTime);

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