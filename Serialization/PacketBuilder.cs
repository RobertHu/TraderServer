using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CommonUtil;
namespace Serialization
{
    public class PacketBuilder
    {

        public static byte[] Build(SerializedObject target)
        {
            byte priceByte = target.IsPrice? (byte)1 : (byte)0;
            byte[] sessionBytes = GetSessionBytes(target.Session);
            byte[] contentBytes;
            if (target.IsPrice)
            {
                contentBytes = target.Price;
            }
            else
            {
                if (target.ContentInByte != null && target.ContentInByte.Length > 0)
                {
                    contentBytes = target.ContentInByte;
                }
                else
                {
                    if (!string.IsNullOrEmpty(target.ClientInvokeID))
                    {
                        AppendClientInvokeIdToContentNode(target.Content, target.ClientInvokeID);
                    }
                    contentBytes = GetContentBytes(target.Content.OuterXml);
                }
                //contentBytes = ZlibHelper.ZibCompress(contentBytes);
            }
            byte[] contentLengthBytes = contentBytes.Length.ToCustomerBytes();
            byte sessionLengthByte = (byte)sessionBytes.Length;
            int packetLength = Constants.HeadCount + sessionBytes.Length + contentBytes.Length;
            byte[] packet = new byte[packetLength];
            AddHeaderToPacket(packet, priceByte, sessionLengthByte, contentLengthBytes);
            AddSessionToPacket(packet, sessionBytes,Constants.HeadCount);
            AddContentToPacket(packet, contentBytes, Constants.HeadCount + sessionBytes.Length);
            return packet;
        }



        private static void AppendClientInvokeIdToContentNode(XmlNode contentNode,string invokeID)
        {
            XmlElement invokeNode = new XmlDocument().CreateElement(RequestConstants.InvokeIdNodeName);
            invokeNode.InnerText = invokeID;
            XmlNode invokeNode2 = contentNode.OwnerDocument.ImportNode(invokeNode, true);
            contentNode.AppendChild(invokeNode2);
        }


        private static void AddSessionToPacket(byte[] packet,byte[] sessionBytes,int index)
        {
            Array.Copy(sessionBytes, 0, packet, index, sessionBytes.Length);
        }

        private static void AddContentToPacket(byte[] packet, byte[] contentBytes,int index)
        {
            Array.Copy(contentBytes, 0, packet, index, contentBytes.Length);
        }

        private static void AddHeaderToPacket(byte[] packet,byte isPrice,byte sessionLength,byte[] contentLengthBytes)
        {
            packet[0] = isPrice;
            packet[1] = sessionLength;
            Array.Copy(contentLengthBytes, 0, packet, 2, contentLengthBytes.Length);
        }

        private static byte[] GetSessionBytes(string sessionID)
        {
            if (sessionID.IsNullOrEmpty())
            {
                return new byte[0];
            }
            byte[] bytes = Constants.SessionEncoding.GetBytes(sessionID);
            if (bytes.Length > 255) throw new ArgumentOutOfRangeException("the length of SessionID");
            return bytes;
        }

        private static byte[] GetContentBytes(string xml)
        {
            byte[] bytes = Constants.ContentEncoding.GetBytes(xml);
            return bytes;
        }
    }
}
