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
    public static class PacketBuilder
    {
        public  static UnmanagedMemory Build(SerializedObject response)
        {
            if (response.IsKeepAlive)
            {
                return BuildForKeepAlive(response);
            }
            if (response.ContentInPointer != null)
            {
                return BuildForPointer(response.ContentInPointer, response.ClientInvokeID); 
            }
            return BuildForGeneral(response);
        }

        private static UnmanagedMemory BuildForGeneral(SerializedObject response)
        {
            if (!string.IsNullOrEmpty(response.ClientInvokeID))
            {
                AppendClientInvokeIdToContentNode(response.Content, response.ClientInvokeID);
            }
            byte[] contentBytes = GetContentBytes(response.Content.ToString());
            byte[] sessionBytes = GetSessionBytes(response.Session.ToString());
            byte sessionLengthByte = (byte)sessionBytes.Length;
            byte[] contentLengthBytes = contentBytes.Length.ToCustomerBytes();
            int packetLength = Constants.HeadCount + sessionLengthByte + contentBytes.Length;
            UnmanagedMemory packet = new UnmanagedMemory(packetLength);
            byte priceByte = 0;
            AddHeaderToPacket(packet, priceByte, sessionLengthByte, contentLengthBytes);
            AddSessionToPacket(packet, sessionBytes, Constants.HeadCount);
            AddContentToPacket(packet, contentBytes, Constants.HeadCount + sessionLengthByte);
            return packet;
        }


        private static UnmanagedMemory BuildForKeepAlive(SerializedObject response)
        {
            response.KeepAlivePacket[0] = response.IsKeepAliveSuccess ? FirstHeadByteBitConstants.IsKeepAliveAndSuccessValue : FirstHeadByteBitConstants.IsKeepAliveAndFailedValue;
            UnmanagedMemory packet = new UnmanagedMemory(response.KeepAlivePacket);
            return packet;
        }


        private unsafe static UnmanagedMemory BuildForPointer(UnmanagedMemory source, string invokeID)
        {
            UnmanagedMemory content = ZlibHelper.ZibCompress(source.ToArray());
            source.Dispose();
            int contentLength = Constants.INVOKE_ID_LENGTH + content.Length;
            int packetLength=Constants.HeadCount + contentLength;
            UnmanagedMemory packet = new UnmanagedMemory(packetLength);
            byte[] contentLengthBytes = contentLength.ToCustomerBytes();
            packet.Handle[0] = FirstHeadByteBitConstants.IsPlainString;
            packet.Handle[1] = 0;
            byte[] invokeIDBytes = Constants.ClientInvokeIDEncoding.GetBytes(invokeID);
            for (int i = 0; i < contentLengthBytes.Length; i++)
            {
                packet.Handle[Constants.ContentLengthIndex + i] = contentLengthBytes[i];
            }
            int index = Constants.HeadCount;
            for (int i = 0; i < invokeIDBytes.Length; i++)
            {
                packet.Handle[index + i] = invokeIDBytes[i];
            }
            index += invokeIDBytes.Length;
            for (int i = 0; i < content.Length; i++)
            {
                packet.Handle[index + i] = content.Handle[i];
            }
            content.Dispose();
            return packet;
        }



        public static UnmanagedMemory BuildPrice(byte[] price)
        {
            return BuildForCommandCommon(price, true);
          
        }

        public static UnmanagedMemory BuildForContentInBytesCommand(byte[] content)
        {
            return BuildForCommandCommon(content, false);
        }

        private unsafe static UnmanagedMemory BuildForCommandCommon(byte[] data, bool isPrice)
        {
            int packetLength = Constants.HeadCount + data.Length;
            UnmanagedMemory packet =new UnmanagedMemory(packetLength);
            byte[] contentLengthBytes = CustomerIntCache.Get(data.Length);
            packet.Handle[0] = 0;
            if (isPrice)
            {
                byte priceByte = FirstHeadByteBitConstants.IsPricevValue;
                packet.Handle[0] = priceByte;
            }
            packet.Handle[1] = 0;
            for (int i = 0; i < contentLengthBytes.Length; i++)
            {
                packet.Handle[Constants.ContentLengthIndex + i] = contentLengthBytes[i];
            }
            for (int i = 0; i < data.Length; i++)
            {
                packet.Handle[Constants.HeadCount + i] = data[i];
            }
            return packet;
        }


        private static void AppendClientInvokeIdToContentNode(XElement contentNode,string invokeID)
        {
            contentNode.Add(new XElement(RequestConstants.InvokeIdNodeName, invokeID));
        }


        private unsafe static void AddSessionToPacket(UnmanagedMemory packet,byte[] sessionBytes,int index)
        {
            if (sessionBytes != null)
            {
                for (int i = 0; i < sessionBytes.Length; i++)
                {
                    packet.Handle[index + i] = sessionBytes[i];
                }
            }
        }

        private unsafe static void AddContentToPacket(UnmanagedMemory packet, byte[] contentBytes,int index)
        {
            for (int i = 0; i < contentBytes.Length; i++)
            {
                packet.Handle[index + i] = contentBytes[i];
            }
        }

        private unsafe static void AddHeaderToPacket(UnmanagedMemory packet, byte isPrice, byte sessionLength, byte[] contentLengthBytes)
        {
            packet.Handle[0] = isPrice;
            packet.Handle[1] = sessionLength;
            int startIndex = Constants.ContentLengthIndex;
            for (int i = 0; i < contentLengthBytes.Length; i++)
            {
                packet.Handle[startIndex + i] = contentLengthBytes[i];
            }
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
