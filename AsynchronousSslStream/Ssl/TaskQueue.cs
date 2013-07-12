using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using log4net;

namespace Trader.Server.Ssl
{
    public class TaskQueue
    {
        private TaskQueue() 
        {
            this._Events = new AutoResetEvent[] { this._Event,this._StopEvent};
        }
        private ILog _Logger = LogManager.GetLogger(typeof(TaskQueue));
        public static readonly TaskQueue Default = new TaskQueue();
        private ConcurrentQueue<Task> _Queue = new ConcurrentQueue<Task>();
        private AutoResetEvent _Event = new AutoResetEvent(false);
        private AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private AutoResetEvent[] _Events;
        private volatile bool _IsStarted = false;
        private volatile bool _IsStopped = false;
        public void Enqueue(Task task)
        {
            this._Queue.Enqueue(task);
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
                Task task;
                while (this._Queue.TryDequeue(out task))
                {
                    try
                    {
                        task.Start();
                    }
                    catch (Exception ex)
                    {
                        this._Logger.Error(ex);
                    }
                }
            }
        }

        public void Start()
        {
            try
            {
                if (this._IsStarted)
                {
                    return;
                }
                Thread thread = new Thread(this.Process);
                thread.IsBackground = true;
                thread.Start();
                this._IsStarted = true;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }

        public void Stop()
        {
            this._IsStopped = true;
            this._StopEvent.Set();
        }


    }
}
