using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using System.Collections.Concurrent;
namespace Trader.Server
{
    public class SessionMonitor
    {
        private ILog _Logger = LogManager.GetLogger(typeof(SessionMonitor));
        private TimeSpan _ExpiredTimeout;
        private long _ClientCount = 0;
        private volatile bool _IsStarted = false;
        private volatile bool _IsStoped = false;
        private ConcurrentDictionary<Guid, DateTime> dict = new ConcurrentDictionary<Guid, DateTime>();
        public SessionMonitor(TimeSpan timeout)
        {
            this._ExpiredTimeout = timeout;
        }

        public void Start()
        {
            try
            {
                if (this._IsStarted)
                {
                    return;
                }
                Thread thread = new Thread(MotitorHandle);
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
            this._IsStoped = true;
        }


        public void Add(Guid session)
        {
            if (this.dict.TryAdd(session, DateTime.Now))
            {
                this._ClientCount++;
                this._Logger.InfoFormat("ClientCount={0}", this._ClientCount);
                //AgentController.Default.AddForLogined(session);
            }
        }

        public void Update(Guid? session)
        {
            if (!session.HasValue)
            {
                return;
            }
            if (this.dict.ContainsKey(session.Value))
            {
                this.dict.AddOrUpdate(session.Value, DateTime.Now, (k, v) => v);
            }
        }

        public void Remove(Guid session)
        {
            RemoveHelper(session);
        }

        public int GetClientCount()
        {
            return this.dict.Count;
        }

        public bool Exist(Guid session)
        {
            return this.dict.ContainsKey(session);
        }

        private void MotitorHandle()
        {
            while (true)
            {
                if (this._IsStoped)
                {
                    break;
                }
                Thread.Sleep(60000);
                var target = this.dict.Where(p => DateTime.Now - p.Value > this._ExpiredTimeout);
                foreach (var item in target)
                {
                    this._Logger.InfoFormat("remove session:{0}", item.Key);
                    RemoveHelper(item.Key);
                }
            }
        }

        private void RemoveHelper(Guid session)
        {
            DateTime result;
            if (this.dict.TryRemove(session, out result))
            {
                ResouceManager.Default.ReleaseResource(session);
                AgentController.Default.EnqueueDisconnectSession(session);
                this._ClientCount--;
                this._Logger.InfoFormat("ClientCount={0}", this._ClientCount);
            }
        }

    }
}
