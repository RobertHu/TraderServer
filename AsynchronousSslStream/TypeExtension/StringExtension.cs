using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using System.Xml;

namespace Trader.Server.TypeExtension
{
    public static class StringExtension
    {
        public static string ToJoinString(this string[] source)
        {
            return source.Aggregate(string.Empty, (acc, m) => string.Format("{0}{1}{2}", acc, StringConstants.ArrayItemSeparator, m), acc => acc.Substring(1));
        }

        public static Guid[] ToGuidArray(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new Guid[]{};
            }
           return source.Split(StringConstants.ArrayItemSeparator).Select(s => Guid.Parse(s)).ToArray();
        }

        public static Guid ToGuid(this string source)
        {
            Guid result;
            Guid.TryParse(source,out result);
            return result;
        }

        public static int ToInt(this string source)
        {
            return int.Parse(source);
        }
  
    

        public static string[][] To2DArray(this string souce)
        {
            return souce.Split(StringConstants.Array2DItemSeparator)
                        .Select(m => m.Split(StringConstants.ArrayItemSeparator))
                        .ToArray();
        }

        public static XmlNode ToXmlNode(this string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.FirstChild;
        }
    }
}
