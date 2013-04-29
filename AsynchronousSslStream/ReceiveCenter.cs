using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
namespace AsyncSslServer
{
    public class ReceiveCenter
    {
        public static readonly ReceiveCenter Default = new ReceiveCenter();
        private Queue<Tuple<Guid, byte[]>> _Queue = new Queue<Tuple<Guid, byte[]>>(1024);
        private volatile bool _IsStarted = false;
        private volatile bool _IsStopped = false;
        private object _Lock = new object();
        private AutoResetEvent _Event = new AutoResetEvent(false);
        private ILog _Logger = LogManager.GetLogger(typeof(ReceiveCenter));
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

        public void DataArrivedHandler(object sender, Trader.Helper.Common.DataArrivedEventArgs args)
        {
            this.Send(args.Session, args.Data);
        }


        public void Send(Guid session, byte[] packet)
        {
            lock (this._Lock)
            {
                this._Queue.Enqueue(Tuple.Create(session, packet));
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
                    Tuple<Guid, Byte[]> workItem = null;
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
                    Guid session = workItem.Item1;
                    byte[] packet = workItem.Item2;
                    var receiveAgent = AgentController.Default.GetReceiver(session);
                    if (receiveAgent != null)
                    {
                        receiveAgent.Send(session, packet);
                    }
                    else
                    {
                        this._Logger.Info("can't find a receive agent");
                    }
                }
            }
        }



    }
}
