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
        private ConcurrentQueue<byte[]> _Queue = new ConcurrentQueue<byte[]>();
        private AutoResetEvent _Event = new AutoResetEvent(false);
        public Client(SslStream stream, long session,IReceiveCenter receiveCenter)
        {
            this._Stream = stream;
            this._Reader = new Reader.ClientReader(stream, session, receiveCenter);
            this._Reader.Start();
            ThreadPool.QueueUserWorkItem( x=>BeginWrite(),null);
        }

        public void Send(byte[] packet)
        {
            if (this._IsClosed || this._Reader.IsClosed)
            {
                return;
            }
            this._Queue.Enqueue(packet);
            this._Event.Set();
        }

        public void UpdateSession(long session)
        {
            this._Reader.UpdateSession(session);
        }
        private void BeginWrite()
        {
            try
            {
                byte[] packet;
                if (_Queue.TryDequeue(out packet))
                {
                    Task task = Task.Factory.FromAsync(this._Stream.BeginWrite, this._Stream.EndWrite, packet, 0, packet.Length, null);
                    task.ContinueWith(t =>
                        {
                            BeginWrite();
                        });
                }
                else
                {
                    this._Event.WaitOne();
                    BeginWrite();
                }
            }
            catch (Exception ex)
            {
                this.Close();
            }

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
