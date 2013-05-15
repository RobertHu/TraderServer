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
        private static  Dictionary<string, long> dict = new Dictionary<string, long>();
        private static long _NextSequence = 0;
        private static object _Lock = new object();
        public static long AddSign(string sign)
        {
            lock (_Lock)
            {
                if (!dict.ContainsKey(sign))
                {
                    _NextSequence++;
                    dict.Add(sign, _NextSequence);
                }
                return _NextSequence;
            }
        }

        public static void Remove(string sign)
        {
            lock (_Lock)
            {
                if (dict.ContainsKey(sign))
                {
                    dict.Remove(sign);
                }
            }
        }
    }
}
