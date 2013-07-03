using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Trader.Common;

namespace Trader.Server.Util
{
    public static class XmlRequestCommandHelper
    {
        public static List<string> GetArguments(XElement content)
        {
            var args = content.Descendants(RequestConstants.ArgumentNodeName).SingleOrDefault();
            List<string> argList = new List<string>();
            if (args == null) { return null; }
            else
            {
                foreach (var node in args.Descendants())
                {
                    argList.Add(node.Value);
                }
                return argList;
            }
        }
    }
}
