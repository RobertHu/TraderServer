using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using System.Xml;
using System.Diagnostics;

namespace Trader.Server.TypeExtension
{
    public static class StringExtension
    {
        private const int Capacity = 100;
        public static string ToJoinString(this string[] source)
        {
            StringBuilder sb = new StringBuilder(Capacity);
            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i]);
                if (i == source.Length - 1)
                {
                    continue;
                }
                sb.Append(StringConstants.ArrayItemSeparator);
            }
            return sb.ToString();
        }

        public static Guid[] ToGuidArray(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new Guid[]{};
            }
            string[] stringArray = ToStringArray(source, StringConstants.ArrayItemSeparator);
            Guid[] result = new Guid[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                result[i] = Guid.Parse(stringArray[i]);
            }
            return result;
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
  
        public static string[][] To2DArray(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            string[] outter = ToStringArray(source, StringConstants.Array2DItemSeparator);
          
            string[][] result = new string[outter.Length][];
            for (int i = 0; i < outter.Length; i++)
            {
                result[i] = ToStringArray(outter[i], StringConstants.ArrayItemSeparator);
            }
            return result;
        }

        public static XmlNode ToXmlNode(this string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.FirstChild;
        }


        private static string[] ToStringArray(string source, char separator)
        {
            int lastIndex = 0;
            int currentIndex = -1;
            List<String> list = new List<String>();
            while ((currentIndex = source.IndexOf(separator, lastIndex)) != -1)
            {
                list.Add(ParseString(source, lastIndex, currentIndex));
                lastIndex = currentIndex + 1;
            }
            list.Add(ParseString(source, lastIndex, source.Length));
            return list.ToArray();
        }

        private static Guid ParseGuid(string source, int startIndex, int endIndex)
        {
            String item = ParseString(source, startIndex, endIndex);
            return Guid.Parse(item);
        }

        private unsafe static String ParseString(string source, int startIndex, int endIndex)
        {
            int len = endIndex - startIndex;
            char* newChars = stackalloc char[len];
            char* currentChar = newChars;
            for (int i = startIndex; i < endIndex; i++)
            {
                *currentChar++ = source[i];
            }
            return new string(newChars, 0, len);
        }
    }
}
