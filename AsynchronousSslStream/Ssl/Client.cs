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
namespace Trader.Server.Ssl
{
    public class Client
    {
        private SslStream _Stream;
        private Reader.ClientReader _Reader;
        private volatile bool _IsClosed = false;
        private Queue<byte[]> _Queue = new Queue<byte[]>();
        private volatile bool _IsEndWrote = true;
        private volatile bool _IsStoped = true;
        private object _Lock = new object();
        public Client(SslStream stream, long session,IReceiveCenter receiveCenter)
        {
            this._Stream = stream;
            this._Reader = new Reader.ClientReader(stream, session, receiveCenter);
            this._Reader.Start();
        }

        public void Send(byte[] packet)
        {
            lock (this._Lock)
            {
                if (this._IsClosed || this._Reader.IsClosed)
                {
                    return;
                }
                this._Queue.Enqueue(packet);
                if (this._IsStoped && this._IsEndWrote)
                {
                    BeginWrite();
                }
            }
        }

        public void UpdateSession(long session)
        {
            this._Reader.UpdateSession(session);
        }
        private void BeginWrite()
        {
            lock (this._Lock)
            {
                try
                {
                    if (!this._IsEndWrote)
                    {
                        return;
                    }
                    if (this._Queue.Count != 0)
                    {
                        byte[] packet = this._Queue.Dequeue();
                        if (this._IsStoped)
                        {
                            this._IsStoped = false;
                        }
                        this._IsEndWrote = false;
                        this._Stream.BeginWrite(packet, 0, packet.Length, this.EndWrite, null);
                    }
                    else
                    {
                        this._IsStoped = true;
                    }
                }
                catch (Exception ex)
                {
                    this.Close();
                }
            }

        }

        private void EndWrite(IAsyncResult ar)
        {
            this._Stream.EndWrite(ar);
            this._IsEndWrote = true;
            BeginWrite();
        }

        private void Close()
        {
            if (this._Reader.IsClosed && (!this._IsClosed))
            {
                this._IsClosed = true;
                return;
            }
            if (this._IsClosed || this._Reader.IsClosed)
            {
                return;
            }
            this._Stream.Close();
            this._IsClosed = true;
            this._Reader.IsClosed = true;
            long session = this._Reader.GetSession();
            AgentController.Default.EnqueueDisconnectSession(session);
        }


      


    }
}
