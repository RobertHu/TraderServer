using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncSslServer.TypeExtension
{
    public static class BoolExtension
    {
        public static string ToXmlResult(this bool source)
        {
            return source ? "1" : "0";
        }
    }
}
