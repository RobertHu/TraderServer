using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Trader.Common;
using System.Threading;
using Trader.Helper;
namespace AsyncSslServer
{
    public class SendCenter
    {
        public static readonly SendCenter Default = new SendCenter();
        private ILog _Logger = LogManager.GetLogger(typeof(SendCenter));
        private Queue<JobItem> _Queue = new Queue<JobItem>(1024);
        private object _Lock = new object();
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

        public void ResponseSentHandle(object sender, Trader.Helper.Common.ResponseEventArgs e)
        {
            this.Send(e.Job);
        }


        public void Send(JobItem item)
        {
            lock (this._Lock)
            {
                this._Queue.Enqueue(item);
                this._Event.Set();
            }
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
                while (true)
                {
                    if (this._IsStopped)
                    {
                        break;
                    }
                    JobItem workItem = null;
                    lock (this._Lock)
                    {
                        if (this._Queue.Count != 0)
                        {
                            workItem = this._Queue.Dequeue();
                        }
                    }
                    if (workItem == null)
                    {
                        break;
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
