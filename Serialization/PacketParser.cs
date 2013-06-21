using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil;
using System.Xml;
using System.Xml.Linq;
using Trader.Common;
using log4net;
namespace Serialization
{
    public static class PacketParser
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(PacketParser));

        public static SerializedObject  Parse(byte[] packet)
        {
            try
            {
                bool isKeepAlive = (packet[0] & FirstHeadByteBitConstants.IsKeepAliveMask) == FirstHeadByteBitConstants.IsKeepAliveMask;
                if (isKeepAlive)
                {
                    return ParseForKeepAlive(packet);
                }
                return ParseForNormal(packet);

            }
            catch (Exception ex)
            {
                _Logger.Error("parse packet", ex);
                return null;
            }
        }

        private static SerializedObject ParseForNormal(byte[] packet)
        {
            byte sessionLength = packet[1];
            byte[] contentLengthBytes = new byte[Constants.ContentHeaderLength];
            Array.Copy(packet, 2, contentLengthBytes, 0, Constants.ContentHeaderLength);
            int contentLength = contentLengthBytes.ToCustomerInt();
            byte[] contentBytes = new byte[contentLength];
            int contentIndex = Constants.HeadCount + sessionLength;
            Array.Copy(packet, contentIndex, contentBytes, 0, contentLength);
            string sessionStr = Constants.SessionEncoding.GetString(packet, Constants.HeadCount, sessionLength);
            long session = SessionMapping.Get(sessionStr);
            string content = Constants.ContentEncoding.GetString(contentBytes);
            string processContent = GetRidOfUnprintablesAndUnicode(content);
            XElement contentNode = XElement.Parse(processContent, LoadOptions.None);
            XElement clientInvokeNode = FetchClientInvokeNode(contentNode);
            string clientInvokeId = clientInvokeNode == null ? string.Empty : clientInvokeNode.Value;
            return new SerializedObject(session, clientInvokeId, contentNode);
        }


        private static SerializedObject ParseForKeepAlive(byte[] packet)
        {
            byte sessionLength = packet[1];
            string sessionStr = Constants.SessionEncoding.GetString(packet, Constants.HeadCount, sessionLength);
            long session = SessionMapping.Get(sessionStr);
            return SerializedObject.Create(session, true, packet);
        }

        private static XElement FetchClientInvokeNode(XElement parent)
        {
            XElement result = null;
            foreach (XElement ele in parent.Descendants())
            {
                if (ele.Name == RequestConstants.InvokeIdNodeName)
                {
                    result = ele;
                    break;
                }
            }
            return result;
        }



        private static string GetRidOfUnprintablesAndUnicode(string inpString)
        {
            string outputs = String.Empty;
            for (int jj = 0; jj < inpString.Length; jj++)
            {
                char ch = inpString[jj];
                if (((int)(byte)ch) >= 32 & ((int)(byte)ch) <= 128)
                {
                    outputs += ch;
                }
            }
            return outputs;
        } 

       

    }
}
