using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server.Serialization
{
    public static class PriceEncoding
    {
        private static Dictionary<char, byte> mapping;
        private static byte INVALID_VALUE = 10;
        static PriceEncoding()
        {
            mapping = new Dictionary<char, byte>()
           {
               {'0',0},
               {'1',1},
               {'2',2},
               {'3',3},
               {'4',4},
               {'5',5},
               {'6',6},
               {'7',7},
               {'8',8},
               {'9',9}
           };
        }

        public static void Map(byte[] packet, int index, string input)
        {
            int i = 0;
            for (; i < input.Length; i++)
            {
                packet[index + i] = mapping[input[i]];
            }
            while (i < 4)
            {
                packet[index + i] = INVALID_VALUE;
                i++;
            }
        }
    }
}
