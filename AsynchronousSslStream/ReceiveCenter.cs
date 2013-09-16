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
        private readonly ConcurrentQueue<ReceiveData> _Queue = new ConcurrentQueue<ReceiveData>();
        private volatile bool _IsStarted;
        private volatile bool _IsStopped;
        private readonly AutoResetEvent _Event = new AutoResetEvent(false);
        private readonly AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent[] _Events;
        private readonly ILog _Logger = LogManager.GetLogger(typeof(ReceiveCenter));
        private ReceiveData _Current;
        private ReceiveCenter()
        {
            _Events = new[] { _Event,_StopEvent};
        }

        public void Start()
        {
            try
            {
                if (_IsStarted) return;
                Thread thread = new Thread(Process) {IsBackground = true};
                thread.Start();
                _IsStarted = true;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }

        public void Stop()
        {
            _IsStopped = true;
            _StopEvent.Set();
        }


        public void Add(ReceiveData data)
        {
            _Queue.Enqueue(data);
            _Event.Set();
        }

        private void Process()
        {
            while (true)
            {
                if (_IsStopped) break;
                WaitHandle.WaitAny(waitHandles: _Events);
                while (_Queue.TryDequeue(out _Current))
                {
                    var receiveAgent = AgentController.Default.GetReceiver(_Current.ClientID);
                    if (receiveAgent != null)
                    {
                        receiveAgent.Send(_Current);
                    }
                    else
                    {
                        _Logger.InfoFormat("can't find a receive agent with session {0}",_Current.ClientID);
                    }
                }
            }
        }
    }
}
