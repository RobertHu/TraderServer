using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Trader.Server._4BitCompress
{
    struct AutoValue
    {
        public int RefCount;
        public long Sequence;
        public AutoValue(int refCount, long sequence)
        {
            this.RefCount = refCount;
            this.Sequence = sequence;
        }
    }


    public class QuotationFilterSignMapping
    {
        private static readonly Dictionary<string, AutoValue> _Dict = new Dictionary<string, AutoValue>();
        private static long _NextSequence;
        private static readonly object _Lock = new object();
        public static long AddSign(string sign)
        {
            lock (_Lock)
            {
                AutoValue val;
                if (!_Dict.TryGetValue(sign, out val))
                {
                    ++_NextSequence;
                    _Dict.Add(sign, new AutoValue(1, _NextSequence));
                    return _NextSequence;
                }
                _Dict[sign] = new AutoValue(++(val.RefCount), val.Sequence);
                return val.Sequence;
            }
        }

        public static bool Remove(string sign)
        {
            lock (_Lock)
            {
                AutoValue val;
                if (!_Dict.TryGetValue(sign, out val)) return false;
                if (--(val.RefCount) == 0)
                {
                    _Dict.Remove(sign);
                    return true;
                }
                _Dict[sign] = new AutoValue(val.RefCount, val.Sequence);
                return false;
            }
        }
    }
}
