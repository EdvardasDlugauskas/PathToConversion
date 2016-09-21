using System.Collections.Generic;
using System.Linq;

namespace ConversionEdvardas
{
    class Program
    {
        static void Main()
        {
            var transactions = Data.ReadJson();

            var allPaths = getConversionPaths(transactions);
            
           Data.PrintPathInfo(allPaths); 
        }


        private static List<ConversionPath> getConversionPaths(List<Transaction> transactions)
        {
            var allPaths = new List<ConversionPath>();

            foreach (var cookie in Data.CookiesWithConversion(transactions))
            {
                var filteredTransactions = transactions.Where(a => a.CookieId == cookie).OrderBy(a => a.LogTime);

                Transaction attributeToTrans = null;

                var stack = new List<Transaction>();

                foreach (var trans in filteredTransactions)
                {
                    switch (trans.TransactionType)
                    {
                        case 1:
                        case 2:
                            attributeToTrans = Data.DecideAttribution(attributeToTrans, trans);
                            break;
                        case 100:
                            trans.AttributeTo(attributeToTrans);
                            break;
                    }
                    stack.Add(trans);
                    if (Data.IsTransLead(trans))
                    {
                        allPaths.Add(new ConversionPath(stack, attributeToTrans));
                    }
                }
            }
            return allPaths;
        }
    }
}