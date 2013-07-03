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
using Trader.Server.Serializationn;
namespace Trader.Server
{
    public class SendCenter
    {
        public static readonly SendCenter Default = new SendCenter();
        private ILog _Logger = LogManager.GetLogger(typeof(SendCenter));
        private ConcurrentQueue<SerializedObject> _Queue = new ConcurrentQueue<SerializedObject>();
        private AutoResetEvent _Event = new AutoResetEvent(false);
        private volatile bool _IsStarted=false;
        private volatile bool _IsStopped=false;
        private SendCenter() { }

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

        public void Send(SerializedObject item)
        {
            if (item == null)
            {
                return;
            }
            this._Queue.Enqueue(item);
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
                SerializedObject workItem = null;
                while (this._Queue.TryDequeue(out workItem))
                {
                    UnmanagedMemory packet = SerializeManager.Default.Serialize(workItem);
                    if (packet == null)
                    {
                        continue;
                    }
                    if (workItem.Session == SessionMapping.INVALID_VALUE)
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
