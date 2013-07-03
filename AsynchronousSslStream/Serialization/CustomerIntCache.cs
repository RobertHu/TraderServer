using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using CommonUtil;
namespace Trader.Server.Serialization
{
    public class CustomerIntCache
    {
        private static ConcurrentDictionary<int, byte[]> _Dict = new ConcurrentDictionary<int, byte[]>();
        public static byte[] Get(int length)
        {
            byte[] result;
            if (_Dict.TryGetValue(length, out result))
            {
                return result;
            }
            result = length.ToCustomerBytes();
            _Dict.TryAdd(length, result);
            return result;
        }
    }
}
