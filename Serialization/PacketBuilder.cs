using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CommonUtil;
using System.Xml.Linq;
using Trader.Common;
namespace Serialization
{
    public class PacketBuilder
    {

        public static byte[] Build(SerializedObject target)
        {
            byte priceByte = 0;
            if (target.IsKeepAlive)
            {
                target.KeepAlivePacket[0] = target.IsKeepAliveSuccess ? KeepAliveConstants.IsKeepAliveAndSuccessValue : KeepAliveConstants.IsKeepAliveAndFailedValue;
                return target.KeepAlivePacket;
            }
            if (!string.IsNullOrEmpty(target.ClientInvokeID))
            {
                AppendClientInvokeIdToContentNode(target.Content, target.ClientInvokeID);
            }
            byte[] contentBytes = GetContentBytes(target.Content.ToString());
            byte[] sessionBytes = GetSessionBytes(target.Session.ToString());
            byte sessionLengthByte = (byte)sessionBytes.Length;
            byte[] contentLengthBytes = contentBytes.Length.ToCustomerBytes();
            int packetLength = Constants.HeadCount + sessionLengthByte + contentBytes.Length;
            byte[] packet = new byte[packetLength];
            AddHeaderToPacket(packet, priceByte, sessionLengthByte, contentLengthBytes);
            AddSessionToPacket(packet, sessionBytes, Constants.HeadCount);
            AddContentToPacket(packet, contentBytes, Constants.HeadCount + sessionLengthByte);
            return packet;
        }

        public static byte[] BuildPrice(byte[] price)
        {
            return BuildForContentBytesCommon(price, true);
        }

        public static byte[] BuildForContentInBytesCommand(byte[] content)
        {
            return BuildForContentBytesCommon(content, false);
        }


        private static byte[] BuildForContentBytesCommon(byte[] content, bool isPrice)
        {
            byte[] packet = new byte[Constants.HeadCount + content.Length];
            byte[] contentLengthBytes = CustomerIntCache.Get(content.Length);
            byte priceByte = isPrice ? KeepAliveConstants.IsPricevValue : (byte)0; 
            AddHeaderToPacket(packet, priceByte, 0, contentLengthBytes);
            AddContentToPacket(packet, content, Constants.HeadCount);
            return packet;
        }

        private static void AppendClientInvokeIdToContentNode(XElement contentNode,string invokeID)
        {
            contentNode.Add(new XElement(RequestConstants.InvokeIdNodeName, invokeID));
        }


        private static void AddSessionToPacket(byte[] packet,byte[] sessionBytes,int index)
        {
            if (sessionBytes != null)
            {
                Buffer.BlockCopy(sessionBytes, 0, packet, index, sessionBytes.Length);
            }
        }

        private static void AddContentToPacket(byte[] packet, byte[] contentBytes,int index)
        {
            Buffer.BlockCopy(contentBytes, 0, packet, index, contentBytes.Length);
        }

        private static void AddHeaderToPacket(byte[] packet,byte isPrice,byte sessionLength,byte[] contentLengthBytes)
        {
            packet[0] = isPrice;
            packet[1] = sessionLength;
            Buffer.BlockCopy(contentLengthBytes, 0, packet, 2, contentLengthBytes.Length);
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
