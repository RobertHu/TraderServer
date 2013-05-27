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
        private Queue<ReceiveData> _Queue = new Queue<ReceiveData>(50);
        private  bool _IsStoped = true;
        private ReceiveData _Current;
        private object _Lock = new object();
        public void Send(ReceiveData data)
        {
            lock (this._Lock)
            {
                this._Queue.Enqueue(data);
                if (this._IsStoped)
                {
                    ProcessData();
                }
            }
        }

        private void ProcessData()
        {
            lock (this._Lock)
            {
                if (this._Queue.Count != 0)
                {
                    this._Current = this._Queue.Dequeue();
                    this._IsStoped = false;
                    ThreadPool.QueueUserWorkItem(this.ProcessCallback);
                }
                else
                {
                    this._IsStoped = true;
                }
            }
                
        }

        private void ProcessCallback(object state)
        {
            SerializedObject request = PacketParser.Parse(this._Current.Data);
            if (request == null)
            {
                return;
            }
            if (request.Session == SessionMapping.INVALID_VALUE)
            {
                request.Session = this._Current.Session;
            }
            else
            {
                request.CurrentSession = this._Current.Session;
            }
            ClientRequestHelper.Process(request);
            ProcessData();
        }
	}
}
