using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
namespace Trader.Server.TypeExtension
{
    public static class XElementExtension
    {
        public static XmlNode ToXmlNode(this XElement xelement)
        {
            string xml = xelement.ToString();
            return xml.ToXmlNode();
        }
    }
}
