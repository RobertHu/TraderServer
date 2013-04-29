using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
namespace AsyncSslServer.TypeExtension
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
