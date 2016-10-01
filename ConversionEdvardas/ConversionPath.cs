using System.Collections.Generic;
using System.Linq;

namespace ConversionEdvardas
{
    public class ConversionPath
    {
        private readonly List<Transaction> _path;
        public List<Transaction> GetPath => _path;

        private readonly Transaction _attributedTo;
        private bool _hasRecentAdInteraction;
        public int cookie;

        public ConversionPath(List<Transaction> path)
        {
            _path = path.OrderBy(a=>a.LogTime).ToList();
            _attributedTo = Data.GetAttributedTransOfPath(_path, GetFirsLogPoint());

            cookie = _path[0].CookieId;

            InheritFromAttributedTransaction();
            SetRecentAdInteraction();
        }

        private void SetRecentAdInteraction()
        {
            var logPointTime = GetFirsLogPoint().LogTime;
            if (_path.Any(a => logPointTime - a.LogTime < Data.RecentAdInteractionSpan)) _hasRecentAdInteraction = true;
            else _hasRecentAdInteraction = false;
        }

        private void InheritFromAttributedTransaction()
        {
            foreach (Transaction trans in _path)
            {
                if (trans.TransactionType == 100)
                {
                    trans.AttributeTo(Data.GetAttributedTransOfPath(_path, trans));
                }
            }
        }


        public string GetAggregatedPath()
        {
            var fullPath = AggregateMedia();
            if (_attributedTo != null)
                fullPath.Add($"[Lead|{InteractionType}|Campaign]");
            else
                fullPath.Add($"[Lead|Non-Campaign|{Referrer}]");

            return string.Join(" -> ", fullPath); // → no unicode in console ;(
        }


        private string Referrer => Data.GetReferrerType(GetFirsLogPoint());


        private Transaction GetFirsLogPoint()
        {
            var firstLeadInChain = _path.Last();
            foreach (var trans in Enumerable.Reverse(_path))
            {
                if (trans.TransactionType != Data.TrackingPoint) continue;

                if (firstLeadInChain.LogTime - trans.LogTime < Data.SessionTimeoutSpan)
                    firstLeadInChain = trans;
                else return firstLeadInChain;
            }
            return firstLeadInChain;
        }


        private string InteractionType
        {
            get
            {
                switch (_attributedTo.TransactionType)
                {
                    case 1:
                        return "Post-Impression";
                    case 2:
                        return "Post-Click";
                    default:
                        return "*Attributed to unspecified transaction type*";
                }
            }
            
        }


        private List<string> AggregateMedia()
        {
            var result = new List<string>();

            string lastMedia = null;
            var mediaCount = 1;
            foreach (var trans in _path)
            {
                if (trans.TransactionType == Data.TrackingPoint) 
                    continue;

                var newMedia = trans.Media;

                if (newMedia == lastMedia)
                {
                    mediaCount++;
                }
                else
                {
                    if (lastMedia != null)
                        result.Add(mediaCount > 1 ? $"[{lastMedia} x{mediaCount}]" : $"[{lastMedia}]");
                    lastMedia = newMedia;
                    mediaCount = 1;
                }
            }

            if (lastMedia != null)
                result.Add(mediaCount > 1 ? $"[{lastMedia} x{mediaCount}]" : $"[{lastMedia}]");

            return result;
        }

        public void Print()
        {
            foreach (var trans in _path)
            {
                trans.Print();
            }
        }
    }
}
