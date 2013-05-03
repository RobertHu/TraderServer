using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server.TypeExtension
{
   public static class GuidExtension
    {
       public static string ToJoinString(this Guid[] source)
       {
           if (source == null || source.Length == 0)
           {
               return string.Empty;
           }
           return source.Select(m => m.ToString()).ToArray().ToJoinString();
       }
    }
}
