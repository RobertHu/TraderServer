using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CommonUtil;
using System.Xml.Linq;
using Trader.Common;
using System.Runtime.InteropServices;
namespace Trader.Server.Serialization
{
    public static class PacketBuilder
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
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
            response.KeepAlivePacket[Constants.IsPriceIndex] = response.IsKeepAliveSuccess ? FirstHeadByteBitConstants.IsKeepAliveAndSuccessValue : FirstHeadByteBitConstants.IsKeepAliveAndFailedValue;
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
            packet.Handle[Constants.IsPriceIndex] = FirstHeadByteBitConstants.IsPlainString;
            packet.Handle[Constants.SessionLengthIndex] = 0;
            int offset = Constants.ContentLengthIndex;
            Marshal.Copy(contentLengthBytes, 0, (IntPtr)(packet.Handle + offset), contentLengthBytes.Length);
            offset = Constants.HeadCount;
            byte[] invokeIDBytes = Constants.ClientInvokeIDEncoding.GetBytes(invokeID);
            Marshal.Copy(invokeIDBytes, 0, (IntPtr)(packet.Handle + offset), invokeIDBytes.Length);
            offset += invokeIDBytes.Length;
            CopyMemory((IntPtr)(packet.Handle + offset),(IntPtr)content.Handle, (uint)content.Length);
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
            packet.Handle[Constants.IsPriceIndex] = 0;
            if (isPrice)
            {
                byte priceByte = FirstHeadByteBitConstants.IsPricevValue;
                packet.Handle[Constants.IsPriceIndex] = priceByte;
            }
            packet.Handle[Constants.SessionLengthIndex] = 0;
            int offset = Constants.ContentLengthIndex;
            Marshal.Copy(contentLengthBytes, 0, (IntPtr)(packet.Handle + offset), contentLengthBytes.Length);
            offset = Constants.HeadCount;
            Marshal.Copy(data, 0, (IntPtr)(packet.Handle + offset), data.Length);
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
                Marshal.Copy(sessionBytes, 0, (IntPtr)(packet.Handle + index), sessionBytes.Length);
            }
        }

        private unsafe static void AddContentToPacket(UnmanagedMemory packet, byte[] contentBytes,int index)
        {
            Marshal.Copy(contentBytes, 0, (IntPtr)(packet.Handle + index), contentBytes.Length);
        }

        private unsafe static void AddHeaderToPacket(UnmanagedMemory packet, byte isPrice, byte sessionLength, byte[] contentLengthBytes)
        {
            packet.Handle[Constants.IsPriceIndex] = isPrice;
            packet.Handle[Constants.SessionLengthIndex] = sessionLength;
            int startIndex = Constants.ContentLengthIndex;
            Marshal.Copy(contentLengthBytes, 0, (IntPtr)(packet.Handle + startIndex), contentLengthBytes.Length);
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
