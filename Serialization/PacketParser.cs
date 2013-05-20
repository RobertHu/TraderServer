using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil;
using System.Xml;
using System.Xml.Linq;
using Trader.Common;
namespace Serialization
{
    public class PacketParser
    {

        public static SerializedObject  Parse(byte[] packet)
        {
            try
            {
                bool isPrice = packet[0] == 1 ? true : false;
                byte sessionLength = packet[1];
                byte[] contentLengthBytes = new byte[Constants.ContentHeaderLength];
                Array.Copy(packet, 2, contentLengthBytes, 0, Constants.ContentHeaderLength);
                int contentLength = contentLengthBytes.ToCustomerInt();
                byte[] contentBytes = new byte[contentLength];
                int contentIndex = Constants.HeadCount + sessionLength;
                Array.Copy(packet, contentIndex, contentBytes, 0, contentLength);
                SerializedObject target = null;
                if (isPrice)
                {
                    target = new SerializedObject(isPrice, contentBytes);
                }
                else
                {
                    bool isKeepAlive = (packet[0] & KeepAliveConstants.IsKeepAliveMask) == KeepAliveConstants.IsKeepAliveMask ? true : false;
                    string session = Constants.SessionEncoding.GetString(packet, Constants.HeadCount, sessionLength);
                    long? sessionGuid = SessionMapping.Get(session);
                    if (!isKeepAlive)
                    {
                        string content = Constants.ContentEncoding.GetString(contentBytes);
                        string processContent = GetRidOfUnprintablesAndUnicode(content);
                        XElement contentNode = XElement.Parse(processContent, LoadOptions.None);
                        XElement clientInvokeNode = contentNode.Descendants().Single(m => m.Name == RequestConstants.InvokeIdNodeName);
                        string clientInvokeId = clientInvokeNode == null ? string.Empty : clientInvokeNode.Value;
                        target = new SerializedObject(contentInBytes: null, isPrice: isPrice, session: sessionGuid, clientInvokeId: clientInvokeId, content: contentNode);
                    }
                    else
                    {
                        target =SerializedObject.Create(sessionGuid, isKeepAlive, packet);
                    }
                }
                return target;
            }
            catch(Exception ex)
            {
                return null;
            }
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
