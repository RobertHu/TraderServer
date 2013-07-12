using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server.TypeExtension
{
    public static class BoolExtension
    {
        public static string ToPlainBitString(this bool source)
        {
            return source ? "1" : "0";
        }
    }
}
