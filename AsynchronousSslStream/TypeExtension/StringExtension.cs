using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using System.Xml;

namespace AsyncSslServer.TypeExtension
{
    public static class StringExtension
    {
        public static string ToJoinString(this string[] source)
        {
            return source.Aggregate(string.Empty, (acc, m) => string.Format("{0}{1}{2}", acc, StringConstants.ArrayItemSeparator, m), acc => acc.Substring(1));
        }

        public static Guid[] ToGuidArray(this string source)
        {
           return source.Split(StringConstants.ArrayItemSeparator).Select(s => Guid.Parse(s)).ToArray();
        }
        public static XmlNode ToXmlNode(this string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.FirstChild;
        }
    }
}
