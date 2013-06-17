using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Trader.Common;
namespace Trader.Server.Util
{
    public static class XmlSerializeHelper
    {
        public static string ToXml(Type targetType, object target)
        {
            XmlSerializer serializer = new XmlSerializer(targetType);
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, target);
                return Constants.ContentEncoding.GetString(ms.ToArray());
            }

        }
        
    }
}
