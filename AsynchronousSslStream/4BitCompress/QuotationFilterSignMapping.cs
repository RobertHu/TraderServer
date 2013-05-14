using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Trader.Server._4BitCompress
{
    public class QuotationFilterSignMapping
    {
        private static ConcurrentDictionary<string, long> dict = new ConcurrentDictionary<string, long>();
        private static long _NextSequence = 0;
        public static long AddSign(string sign)
        {
            long sequence;
            if (!dict.TryGetValue(sign, out sequence))
            {
                sequence = Interlocked.Increment(ref _NextSequence);
                dict.TryAdd(sign, sequence);
            }
            return sequence;
        }
    }
}
