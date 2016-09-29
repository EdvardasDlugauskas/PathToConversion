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

        public ConversionPath(List<Transaction> path)
        {
            _path = path;
            _attributedTo = Data.GetAttributedTransOfPath(_path, GetFirsLogPoint());

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
            if (_attributedTo == null) return;

            for (var i = _path.IndexOf(_attributedTo); i < _path.Count; i++)
            {
                if (_path[i].TransactionType == Data.TrackingPoint)
                    _path[i].AttributeTo(_attributedTo);
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
            if (_path.Count == 1) return new List<string> {_path[0].Media};

            var result = new List<string>();

            var lastMedia = _path[0].Media;
            var mediaCount = 1;
            foreach (var trans in _path.Skip(1))
            {
                var newMedia = trans.Media;
                if (newMedia == lastMedia)
                {
                    mediaCount++;
                    continue;
                }
                if (mediaCount > 1)
                    result.Add($"[{lastMedia} x{mediaCount}]");
                else
                    result.Add($"[{lastMedia}]");
                lastMedia = newMedia;
                mediaCount = 1;
            }

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
