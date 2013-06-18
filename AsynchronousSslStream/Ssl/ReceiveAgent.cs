using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using System.Collections.Concurrent;
using System.Threading;
using Trader.Server.Util;
using Serialization;

namespace Trader.Server.Ssl
{
	public class ReceiveAgent
	{
        private ConcurrentQueue<ReceiveData> _Queue = new ConcurrentQueue<ReceiveData>();
        private volatile  bool _IsStoped = true;

        public void Reset()
        {
            this._IsStoped = true;
            ReceiveData data;
            while (this._Queue.TryDequeue(out data)) { }
        }


        public void Send(ReceiveData data)
        {
            this._Queue.Enqueue(data);
            if (this._IsStoped)
            {
                this._IsStoped = false;
                ProcessData();
            }
        }

        private void ProcessData()
        {
            ReceiveData item;
            if (this._Queue.TryDequeue(out item))
            {
                ThreadPool.QueueUserWorkItem(this.ProcessCallback,item);
            }
            else
            {
                this._IsStoped = true;
            }
        }

        private void ProcessCallback(object state)
        {
            ReceiveData item = (ReceiveData)state;
            SerializedObject request = PacketParser.Parse(item.Data);
            if (request == null)
            {
                return;
            }
            if (request.Session == SessionMapping.INVALID_VALUE)
            {
                request.Session = item.Session;
            }
            else
            {
                request.CurrentSession = item.Session;
            }
            ReceiveDataPool.Default.Push(item);
            ClientRequestHelper.Process(request);
            ProcessData();
        }
	}
}
