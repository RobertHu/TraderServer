using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil;
namespace Serialization
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

        public static readonly Encoding SessionEncoding = Encoding.ASCII;
        public static readonly Encoding ContentEncoding = Encoding.UTF8;
        public static readonly Encoding ClientInvokeIDEncoding = Encoding.ASCII;


        public static int GetContentLength(byte[] source, int index)
        {
            Byte[] bytes = new byte[Constants.ContentHeaderLength];
            Buffer.BlockCopy(source, index, bytes, 0, Constants.ContentHeaderLength);
            return bytes.ToCustomerInt();
        }

        public static int GetPacketLength(byte[] source, int index)
        {
            int sessionLength = source[index + Constants.SessionLengthIndex];
            int contentLength = GetContentLength(source, index + Constants.ContentLengthIndex);
            return Constants.HeadCount + sessionLength + contentLength;

        }

    }


    public static class RequestConstants
    {
        public const string RootNodeName = "Request";
        public const string ArgumentNodeName = "Arguments";
        public const string MethodNodeName = "Method";
        public const string InvokeIdNodeName = "InvokeId";
    }

    public static class ResponseConstants
    {
        public const string RootNodeName = "Result";
        public const string SingleResultContentNodeName = "content_result";
        public const string ErrorResultNodeName = "error";
    }

    public static class KeepAliveConstants
    {
        public const byte IsPricevValue = 0x01;
        public const byte IsKeepAliveMask = 0x02;
        public const byte IsKeepAliveAndSuccessValue = 0x06;
        public const byte IsKeepAliveAndFailedValue = 0x02;
    }
     



}
