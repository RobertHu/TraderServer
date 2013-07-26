using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using System.Collections.Concurrent;
using Trader.Common;
namespace Trader.Server
{
    public class ReceiveCenter
    {
        public static readonly ReceiveCenter Default = new ReceiveCenter();
        private ConcurrentQueue<ReceiveData> _Queue = new ConcurrentQueue<ReceiveData>();
        private volatile bool _IsStarted = false;
        private volatile bool _IsStopped = false;
        private AutoResetEvent _Event = new AutoResetEvent(false);
        private AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private AutoResetEvent[] _Events;
        private ILog _Logger = LogManager.GetLogger(typeof(ReceiveCenter));
        private ReceiveData _Current;
        private ReceiveCenter()
        {
            this._Events = new AutoResetEvent[] { this._Event,this._StopEvent};
        }

        public void Start()
        {
            try
            {
                if (this._IsStarted)
                {
                    return;
                }
                Thread thread = new Thread(Process);
                thread.IsBackground = true;
                thread.Start();
                this._IsStarted = true;
            }
            catch (Exception ex)
            {
                this._Logger.Error(ex);
            }
        }

        public void Stop()
        {
            this._IsStopped = true;
            this._StopEvent.Set();
        }


        public void Add(ReceiveData data)
        {
            this._Queue.Enqueue(data);
            this._Event.Set();
        }


        private void Process()
        {
            while (true)
            {
                if (this._IsStopped)
                {
                    break;
                }
                WaitHandle.WaitAny(this._Events);
                while (this._Queue.TryDequeue(out this._Current))
                {
                    var receiveAgent = AgentController.Default.GetReceiver(this._Current.ClientID);
                    if (receiveAgent != null)
                    {
                        receiveAgent.Send(this._Current);
                    }
                    else
                    {
                        this._Logger.InfoFormat("can't find a receive agent with session {0}",this._Current.ClientID);
                    }
                }
            }
        }



    }
}
