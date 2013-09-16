using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Trader.Common;
using System.Threading;
using System.Collections.Concurrent;
using Trader.Server.ValueObjects;
using Trader.Server.Serialization;
namespace Trader.Server
{
    public class SendCenter
    {
        public static readonly SendCenter Default = new SendCenter();
        private readonly ILog _Logger = LogManager.GetLogger(typeof(SendCenter));
        private readonly ConcurrentQueue<SerializedObject> _Queue = new ConcurrentQueue<SerializedObject>();
        private readonly AutoResetEvent _Event = new AutoResetEvent(false);
        private readonly AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent[] _Events;
        private volatile bool _IsStarted;
        private volatile bool _IsStopped;
        private SendCenter() 
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

        public void Send(SerializedObject item)
        {
            if (item == null) return;
            _Queue.Enqueue(item);
            _Event.Set();
        }

        private void Process()
        {
            while (true)
            {
                if (_IsStopped)
                {
                    break;
                }
                WaitHandle.WaitAny(_Events);
                SerializedObject workItem = null;
                while (_Queue.TryDequeue(out workItem))
                {
                    UnmanagedMemory packet = SerializeManager.Default.Serialize(workItem);
                    if (packet == null)
                    {
                        continue;
                    }
                    if (workItem.Session == Session.InvalidValue)
                    {
                        continue;
                    }
                    if (workItem.Sender != null)
                    {
                        workItem.Sender.Send(new CommandForClient(data:packet));
                    }
                }
            }
        }

    }
}
