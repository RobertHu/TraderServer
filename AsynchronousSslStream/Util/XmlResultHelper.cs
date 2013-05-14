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
        public static  XElement ErrorResult
        {
            get
            {
                return NewErrorResult();
            }
        }
        public static XElement CreateRootElement()
        {
            return new XElement(ResponseConstants.RootNodeName);
        }

        public static XElement NewResult(string xml)
        {
            var root = CreateRootElement();
            root.Add(new XElement(ResponseConstants.SingleResultContentNodeName, xml));
            return root;
        }

        public static XElement NewResultWithSigleNode(string xml)
        {
            var root = CreateRootElement();
            root.SetValue(xml);
            return root;
        }

        public static XElement NewResult(string nodeName, string xml)
        {

            var root = CreateRootElement();
            root.Add(new XElement(nodeName, xml));
            return root;
        }


        public static XElement NewErrorResult(string xml = "")
        {
            return NewResult(ResponseConstants.ErrorResultNodeName, xml);
        }

        public static XElement NewResult(Dictionary<string, string> dict)
        {
            var root = CreateRootElement();
            foreach (var p in dict)
            {
                root.Add(new XElement(p.Key, p.Value));
            }
            return root;
        }
    }
}
