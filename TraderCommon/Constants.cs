using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil;
namespace Trader.Common
{
    public static class Constants
    {
        public const int IsPriceLength = 1;
        public const int SessionHeaderLength = 1;
        public const int ContentHeaderLength = 4;
        public const int IsPriceIndex = 0;
        public const int SessionLengthIndex = 1;
        public const int ContentLengthIndex = 2;
        public const int HeadCount = 6;
        public const int InvokeIDLength = 36;
        public static readonly Encoding SessionEncoding = Encoding.ASCII;
        public static readonly Encoding ContentEncoding = Encoding.UTF8;
        public static readonly Encoding ClientInvokeIDEncoding = Encoding.ASCII;
        public static int GetContentLength(byte[] source, int index)
        {
            Byte[] bytes = new byte[ContentHeaderLength];
            Buffer.BlockCopy(source, index, bytes, 0, ContentHeaderLength);
            return bytes.ToCustomerInt();
        }

        public static int GetPacketLength(byte[] source, int index)
        {
            int sessionLength = source[index + SessionLengthIndex];
            int contentLength = GetContentLength(source, index + ContentLengthIndex);
            return HeadCount + sessionLength + contentLength;
        }

    }
}
