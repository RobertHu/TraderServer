using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Serialization;

namespace Trader.Server.Util
{
    public static class XmlRequestCommandHelper
    {
        public static List<string> GetArguments(XmlNode content)
        {
            XmlNode args = content.SelectSingleNode(string.Format("//{0}/{1}", content.Name, RequestConstants.ArgumentNodeName));
            List<string> argList = new List<string>();
            if (args == null) { return null; }
            else
            {
                foreach (XmlNode node in args.ChildNodes)
                {
                    argList.Add(node.InnerText);
                }
                return argList;
            }
        }
    }
}
