using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil;
namespace Trader.Common
{
    public class MsgParser
    {
        public MsgParser() { }
      
        private byte[] _ReceiveBuf = new byte[40960];
        private byte[] _LeftBuf = new byte[40960];
        private int _LeftLength = 0;
        private int _Index = 0;
        public event Action<MsgParser> Closed = delegate { };
        public event Action<byte[]> DataArrived = delegate { };
        public void Parse(byte[] source)
        {
            try
            {
                Buffer.BlockCopy(source, 0, this._ReceiveBuf, 0, source.Length);
                ProcessData(source.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ProcessData(int count)
        {
            if (count <= 0)
            {
                return;
            }

            bool goOn = true;
            if (this._LeftLength != 0)
            {
                goOn = ParseWhenHasLeft(count);
            }
            if (goOn)
            {
                ExtractMsg(count);
            }
        }

        private void ExtractMsg(int dataCount)
        {
            if (dataCount - this._Index > Constants.HeadCount)
            {
                int packetLength = GetPacketLength(this._ReceiveBuf, this._Index);
                if (dataCount - this._Index >= packetLength)
                {
                    byte[] packet = new byte[packetLength];
                    Buffer.BlockCopy(this._ReceiveBuf, _Index, packet, 0, packetLength);
                    DataArrived(packet);
                    this._Index += packetLength;
                    ExtractMsg(dataCount);
                }
                else
                {
                    SaveWhenPartialPacketLeft(dataCount);
                }
            }
            else
            {
                SaveWhenPartialPacketLeft(dataCount);
            }

        }

        private void SaveWhenPartialPacketLeft(int total)
        {
            this._LeftLength = total - this._Index;
            if (this._LeftLength != 0)
            {
                Buffer.BlockCopy(this._ReceiveBuf, this._Index, this._LeftBuf, 0, this._LeftLength);
            }
            this._Index = 0;
        }


        private bool ParseWhenHasLeft(int count)
        {
            bool result = false;
            bool isCountGreatThanSection = true;
            int diff = Constants.HeadCount - this._LeftLength;
            bool isInnerProcess = false;
            if (diff > 0)
            {
                isCountGreatThanSection = count > diff;
                if (!isCountGreatThanSection)
                {
                    Buffer.BlockCopy(this._ReceiveBuf, 0, this._LeftBuf, this._LeftLength, count);
                    this._LeftLength += count;
                }
                else
                {
                    Buffer.BlockCopy(this._ReceiveBuf, 0, this._LeftBuf, this._LeftLength, diff);
                    this._LeftLength += diff;
                    isInnerProcess = true;
                }
            }
           
            if (!isCountGreatThanSection)
            {
                this._Index = 0;
                result = false;
            }
            else
            {
                int packetLength = GetPacketLength(this._LeftBuf, 0);
                bool isExceed = true;
                if (isInnerProcess)
                {
                    isExceed = packetLength - this._LeftLength - count + diff > 0;
                }
                else
                {
                    isExceed = packetLength - this._LeftLength - count > 0;
                }

                if (isExceed)
                {
                    this._Index = 0;
                    if (isInnerProcess)
                    {
                        Buffer.BlockCopy(this._ReceiveBuf, diff, this._LeftBuf, this._LeftLength, count - diff);
                        this._LeftLength += count - diff;
                    }
                    else
                    {
                        Buffer.BlockCopy(this._ReceiveBuf, 0, this._LeftBuf, this._LeftLength, count);
                        this._LeftLength += count;
                    }
                    result = false;
                }
                else
                {
                    if (isInnerProcess)
                    {
                        Buffer.BlockCopy(this._ReceiveBuf, diff, this._LeftBuf, this._LeftLength, packetLength - this._LeftLength);
                        this._Index = packetLength - this._LeftLength + diff;
                    }
                    else
                    {
                        Buffer.BlockCopy(this._ReceiveBuf, 0, this._LeftBuf, this._LeftLength, packetLength - this._LeftLength);
                        this._Index = packetLength - this._LeftLength;
                    }
                    this._LeftLength = 0;
                    byte[] packet = new byte[packetLength];
                    Buffer.BlockCopy(this._LeftBuf, 0, packet, 0, packetLength);
                    DataArrived(packet);
                    result = true;
                }
            }

            return result;
        }



        private int GetContentLength(byte[] source, int index)
        {
            Byte[] bytes = new byte[Constants.ContentHeaderLength];
            Buffer.BlockCopy(source, index, bytes, 0, Constants.ContentHeaderLength);
            return bytes.ToCustomerInt();
        }

        private int GetPacketLength(byte[] source, int index)
        {
            int sessionLength = source[index + Constants.SessionLengthIndex];
            int contentLength = GetContentLength(source, index + Constants.ContentLengthIndex);
            return Constants.HeadCount + sessionLength + contentLength;

        }

    }
}
