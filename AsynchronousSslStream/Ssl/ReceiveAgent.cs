using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using System.Collections.Concurrent;
using System.Threading;
using Trader.Server.Util;
using Trader.Server.Serialization;

namespace Trader.Server.Ssl
{
	public class ReceiveAgent
	{
        private ConcurrentQueue<ReceiveData> _Queue = new ConcurrentQueue<ReceiveData>();
        private volatile  bool _IsStoped = true;
        private ReceiveData _Current;

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
            if (this._Queue.TryDequeue(out this._Current))
            {
                ThreadPool.QueueUserWorkItem(this.ProcessCallback,null);
            }
            else
            {
                this._IsStoped = true;
            }
        }

        private void ProcessCallback(object state)
        {
            SerializedObject request = PacketParser.Parse(this._Current.Data);
            if (request != null)
            {
                if (request.Session == Session.INVALID_VALUE)
                {
                    request.Session = this._Current.ClientID;
                }
                else
                {
                    request.ClientID = this._Current.ClientID;
                }
                request.Sender = AgentController.Default.GetSender(this._Current.ClientID);
                ClientRequestHelper.Process(request);
            }
            ProcessData();
        }
	}
}
