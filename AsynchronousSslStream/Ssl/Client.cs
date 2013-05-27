using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using  Trader.Common;
using System.Threading;
using System.Collections.Concurrent;
using Trader.Helper;
using System.Threading.Tasks;
using log4net;
namespace Trader.Server.Ssl
{
    public class Client
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(Client));
        private SslStream _Stream;
        private volatile bool _IsClosed = false;
        private ConcurrentQueue<byte[]> _Queue = new ConcurrentQueue<byte[]>();
        private volatile bool _IsSendingData = false;
        private byte[] _HeadBuf = new byte[Constants.HeadCount];
        private byte[] _Buf = new byte[512];
        private int _ReadedHeadCount = 0;
        private int _ReadedContentCount = 0;
        private int _ContentLength = 0;
        private long _Session;
        public Client(SslStream stream, long session)
        {
            this._Stream = stream;
            this._Session = session;
            this.BeginReadHeader();
        }

        public void Send(byte[] packet)
        {
            if (this._IsClosed)
            {
                return;
            }
            this._Queue.Enqueue(packet);
            if (!_IsSendingData)
            {
                this._IsSendingData = true;
                BeginWrite();
            }
        }

        private void BeginReadHeader()
        {
            if (this._IsClosed)
            {
                return;
            }
            try
            {
                this._Stream.BeginRead(this._HeadBuf, this._ReadedHeadCount,Constants.HeadCount - this._ReadedHeadCount, this.EndReadHeader, null);
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }


        private void BeginReadContent()
        {
            if (this._IsClosed)
            {
                return;
            }
            try
            {
                this._Stream.BeginRead(this._Buf, this._ReadedContentCount,this._ContentLength - this._ReadedContentCount,this.EndReadContent ,null);
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }


        private void EndReadContent(IAsyncResult ar)
        {
            try
            {
                int len = this._Stream.EndRead(ar);
                if (len > 0)
                {
                    this._ReadedContentCount = +len;
                    if (this._ReadedContentCount != this._ContentLength)
                    {
                        BeginReadContent();
                    }
                    else
                    {
                        byte[] packet = new byte[Constants.HeadCount + this._ContentLength];
                        System.Buffer.BlockCopy(this._HeadBuf, 0, packet, 0, Constants.HeadCount);
                        System.Buffer.BlockCopy(this._Buf, 0, packet, Constants.HeadCount, this._ContentLength);
                        ReceiveCenter.Default.Send(new Common.ReceiveData(this._Session, packet));
                        this.Reset();
                        BeginReadHeader();
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                this.Close();
            }

        }

        private void EndReadHeader(IAsyncResult ar)
        {
            try
            {
                int len = this._Stream.EndRead(ar);
                if (len > 0)
                {
                    this._ReadedHeadCount += len;
                    if (this._ReadedHeadCount != Constants.HeadCount)
                    {
                        BeginReadHeader();
                    }
                    else
                    {
                        int packetLength = Constants.GetPacketLength(this._HeadBuf, 0);
                        this._ContentLength = packetLength - Constants.HeadCount;
                        if (this._Buf.Length < this._ContentLength)
                        {
                            this._Buf = new byte[this._ContentLength];
                        }
                        BeginReadContent();
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }

        public void UpdateSession(long session)
        {
            this._Session = session;
        }

        private void Reset()
        {
            this._ReadedContentCount = 0;
            this._ReadedHeadCount = 0;
            this._ContentLength = 0;
        }

        private void BeginWrite()
        {
            try
            {
                byte[] packet;
                if (this._Queue.TryDequeue(out packet))
                {
                    this._Stream.BeginWrite(packet, 0, packet.Length, this.EndWrite, null);
                }
                else
                {
                    this._IsSendingData = false;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                this.Close();
            }
        }

        private void EndWrite(IAsyncResult ar)
        {
            try
            {
                this._Stream.EndWrite(ar);
                BeginWrite();
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                this.Close();
            }
           
        }

        private void Close()
        {
            if (this._IsClosed )
            {
                return;
            }
            this._Stream.Close();
            this._IsClosed = true;
            AgentController.Default.EnqueueDisconnectSession(this._Session);
        }


    }
}
