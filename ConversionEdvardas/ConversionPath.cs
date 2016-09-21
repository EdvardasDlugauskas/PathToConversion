using System.Collections.Generic;
using System.Linq;

namespace ConversionEdvardas
{
    public class ConversionPath
    {
        private List<Transaction> _path;
        public List<Transaction> GetPath => _path;

        private Transaction _attributedTo;


        public ConversionPath(List<Transaction> path, Transaction attributedTo)
        {
            _path = path;
            _attributedTo = attributedTo;
        }


        public string GetAggregatedPath()
        {
            var fullPath = AggregateMedia();
            if (_attributedTo != null)
                fullPath.Add($"[Lead|Campaign|{GetInteraction()}]");
            else
                fullPath.Add($"[Lead|Non-Campaign|{GetReferrer()}]");

            return string.Join(" -> ", fullPath); // → no unicode in console ;(
        }


        private string GetReferrer()
        {
            return Data.GetReferrerType(GetFirsLogPoint());
        }


        private Transaction GetFirsLogPoint()
        {
            var firstInLeadChain = _path.Last();
            foreach (var trans in Enumerable.Reverse(_path))
            {
                if (trans.TransactionType != 100) return firstInLeadChain;
                firstInLeadChain = trans;
            }
            return firstInLeadChain;
        }


        private string GetInteraction()
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
