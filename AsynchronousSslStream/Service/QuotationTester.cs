using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;

namespace Trader.Server.Service
{
    public class QuotationTester
    {
        private NetworkStream _Stream;
        private QuotationTester() 
        {
            TcpClient client = new TcpClient("127.0.0.1", 6666);
            this._Stream = client.GetStream();
            Thread thread = new Thread(this.Handle);
            thread.IsBackground = true;
            thread.Start();
        }
        private ConcurrentQueue<int> _Queue = new ConcurrentQueue<int>();
        private AutoResetEvent _Event = new AutoResetEvent(false);
        public static readonly QuotationTester Default = new QuotationTester();

        public void Add()
        {
            this._Queue.Enqueue(1);
            this._Event.Set();
        }


        private void Handle()
        {
            while (true)
            {
                this._Event.WaitOne();
                int item;
                while (this._Queue.TryDequeue(out item))
                {
                    this._Stream.WriteByte(1);
                }
            }
        }

    }
}
