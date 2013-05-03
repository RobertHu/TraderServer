using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using Trader.Server.TypeExtension;
using Serialization;
namespace Trader.Server.Util
{
    public static class XmlResultHelper
    {
        public static readonly XmlNode ErrorResult= NewErrorResult();
        public static XElement CreateRootElement()
        {
            return new XElement(ResponseConstants.RootNodeName);
        }

        public static XmlNode NewResult(string xml)
        {
            var root = CreateRootElement();
            root.Add(new XElement(ResponseConstants.SingleResultContentNodeName, xml));
            return root.ToXmlNode();
        }

        public static XmlNode NewResultWithSigleNode(string xml)
        {
            var root = CreateRootElement();
            root.SetValue(xml);
            return root.ToXmlNode();
        }

        public static XmlNode NewResult(string nodeName, string xml)
        {

            var root = CreateRootElement();
            root.Add(new XElement(nodeName, xml));
            return root.ToXmlNode();
        }


        public static XmlNode NewErrorResult(string xml = "")
        {
            return NewResult(ResponseConstants.ErrorResultNodeName, xml);
        }

        public static XmlNode NewResult(Dictionary<string, string> dict)
        {
            var root = CreateRootElement();
            foreach (var p in dict)
            {
                root.Add(new XElement(p.Key, p.Value));
            }
            return root.ToXmlNode();
        }
    }
}
