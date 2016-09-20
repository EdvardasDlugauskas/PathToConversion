using System.Collections.Generic;
using System.Linq;

namespace ConversionEdvardas
{
    class Program
    {
        static void Main()
        {
            // !Get Json objects
            var transactions = Data.ReadJson(Data.DefaultFileName);

            var allPaths = new List<ConversionPath>();

            var cookies = CookiesWithConversion(transactions);

            // Filter out only needed cookies (+by time?)
            foreach (var cookie in cookies)
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
                            attributeToTrans = DecideAttribution(attributeToTrans, trans);
                            break;
                        case 100:
                            trans.AttributeTo(attributeToTrans);
                            break;
                    }
                    stack.Add(trans);
                    if (IsTransLead(trans))
                    {
                        allPaths.Add(new ConversionPath(stack, attributeToTrans));
                    }
                }
            }
            
           Data.PrintPaths(allPaths); 
        }

        private static bool IsTransLead(Transaction trans)
        {
            return trans.TransactionType == 100 && trans.LogPointName.ToLower().Contains("thank you");
        }

        private static Transaction DecideAttribution(Transaction first, Transaction second)
        {
            if (first == null) return second;
            // Assume only transaction types 1 and 2 supplied
            if (first.TransactionType == 1 || second.TransactionType == 2)
                return second;
            return first;
        }

        private static HashSet<int> CookiesWithConversion(List<Transaction> transactions)
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