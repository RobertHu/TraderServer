using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil;
using System.Xml;
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
                    string session = Constants.SessionEncoding.GetString(packet, Constants.HeadCount, sessionLength);
                    string content = Constants.ContentEncoding.GetString(contentBytes);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(content);
                    XmlNode contentNode = doc.FirstChild;
                    XmlNode clientInvokeIdNode = contentNode.SelectSingleNode(string.Format("//{0}/{1}", contentNode.Name, RequestConstants.InvokeIdNodeName));
                    string clientInvokeId = clientInvokeIdNode == null ? string.Empty : clientInvokeIdNode.InnerText;
                    target = new SerializedObject(contentInBytes: null, isPrice: isPrice, session: session, clientInvokeId: clientInvokeId, content: contentNode);
                }
                return target;
            }
            catch
            {
                return null;
            }
        }

       

    }
}
