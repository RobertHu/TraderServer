using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server.SessionNamespace;
using Trader.Common;
using System.Xml;

namespace Trader.Server._4BitCompress
{
    public static class CommandTranslator
    {
        private static object _Lock = new object();
        public static byte[] GetDataForCommand(Token token, TraderState state, Command command)
        {
            if (string.IsNullOrEmpty(state.QuotationFilterSign))
            {
                return null;
            }
            return GetDataBytesInUtf8Format(token, state, command);
        }


        private static byte[] GetDataBytesInUtf8Format(Token token, TraderState state, Command command)
        {
            var node = ConvertCommand(token, state, command);
            if (node == null)
            {
                return null;
            }
            string xml = node.OuterXml;
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            return Constants.ContentEncoding.GetBytes(xml);
        }

        private static XmlNode ConvertCommand(Token token, State state, Command command)
        {
            if (token != null && token.AppType == AppType.Mobile)
            {
                return iExchange3Promotion.Mobile.Manager.ConvertCommand(token, command);
            }
            else
            {
                XmlNode commandNode = command.ToXmlNode(token, state);
                XmlElement commandElement = commandNode as XmlElement;
                if (commandElement == null)
                {
                    return null;
                }
                lock (_Lock)
                {
                    if (!commandElement.HasAttribute(ResponseConstants.CommandSequence))
                    {
                        commandElement.SetAttribute(ResponseConstants.CommandSequence, command.Sequence.ToString());
                    }
                }
                return commandElement;
            }
        }
    }
}
