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
    public class ReceiveCenter:IReceiveCenter
    {
        public static readonly ReceiveCenter Default = new ReceiveCenter();
        private ConcurrentQueue<ReceiveData> _Queue = new ConcurrentQueue<ReceiveData>();
        private volatile bool _IsStarted = false;
        private volatile bool _IsStopped = false;
        private AutoResetEvent _Event = new AutoResetEvent(false);
        private ILog _Logger = LogManager.GetLogger(typeof(ReceiveCenter));
        private ReceiveData _Current;
        private ReceiveCenter() { }

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
        }


        public void Send(ReceiveData data)
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
                this._Event.WaitOne();
                while (this._Queue.TryDequeue(out this._Current))
                {
                    var receiveAgent = AgentController.Default.GetReceiver(this._Current.Session);
                    if (receiveAgent != null)
                    {
                        receiveAgent.Send(this._Current);
                    }
                    else
                    {
                        this._Logger.InfoFormat("can't find a receive agent with session {0}",this._Current.Session);
                    }
                }
            }
        }



    }
}
