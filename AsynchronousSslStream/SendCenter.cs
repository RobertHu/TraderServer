using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Trader.Common;
using System.Threading;
using Trader.Helper;
using System.Collections.Concurrent;
namespace Trader.Server
{
    public class SendCenter
    {
        public static readonly SendCenter Default = new SendCenter();
        private ILog _Logger = LogManager.GetLogger(typeof(SendCenter));
        private ConcurrentQueue<JobItem> _Queue = new ConcurrentQueue<JobItem>();
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

        public void Send(JobItem item)
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
                while (this._Queue.Count != 0)
                {
                    if (this._IsStopped)
                    {
                        break;
                    }
                    JobItem workItem = null;
                    if (!this._Queue.TryDequeue(out workItem))
                    {
                        continue;
                    }
                    byte[] packet = SendManager.SerializeMsg(workItem);
                    if (packet == null)
                    {
                        continue;
                    }
                    if (!workItem.SessionID.HasValue)
                    {
                        continue;
                    }
                    var client = AgentController.Default.GetSender(workItem.SessionID.Value);
                    if (client != null)
                    {
                        client.Send(packet);
                    }
                }
            }
        }
    }
}
