using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using System.Collections.Concurrent;
using Trader.Server.SessionNamespace;
using Trader.Common;
namespace Trader.Server
{
    public class SessionMonitor
    {
        private ILog _Logger = LogManager.GetLogger(typeof(SessionMonitor));
        private TimeSpan _ExpiredTimeout;
        private long _ClientCount = 0;
        private volatile bool _IsStarted = false;
        private volatile bool _IsStoped = false;
        private ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private Dictionary<Session, DateTime> dict = new Dictionary<Session, DateTime>();
        private List<Session> _RemovedSessoinList = new List<Session>(512);
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
                Thread thread = new Thread(MonitorHandle);
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


        public void Add(Session session)
        {
            this._ReadWriteLock.EnterWriteLock();
            try
            {
                if (!this.dict.ContainsKey(session))
                {
                    this.dict.Add(session, DateTime.Now);
                    this._ClientCount++;
                    this._Logger.InfoFormat("ClientCount={0}", this._ClientCount);
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public void Update(Session? session)
        {
            this._ReadWriteLock.EnterWriteLock();
            try
            {
                if (!session.HasValue)
                {
                    return;
                }
                DateTime value;
                if (this.dict.TryGetValue(session.Value,out value))
                {
                    this.dict[session.Value] = DateTime.Now;
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public void Remove(Session session)
        {
            this._ReadWriteLock.EnterWriteLock();
            try
            {
                if (!this.dict.ContainsKey(session))
                {
                    return;
                }
                RemoveHelper(session);
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public bool Exist(Session session)
        {
            this._ReadWriteLock.EnterReadLock();
            try
            {
                return this.dict.ContainsKey(session);
            }
            finally
            {
                this._ReadWriteLock.ExitReadLock();
            }
        }

        private void MonitorHandle()
        {
            while (true)
            {
                if (this._IsStoped)
                {
                    break;
                }
                Thread.Sleep(60000);
                this._ReadWriteLock.EnterWriteLock();
                try
                {
                    foreach(var item in this.dict)
                    {
                        if (DateTime.Now - item.Value> this._ExpiredTimeout)
                        {
                            this._RemovedSessoinList.Add(item.Key);
                        }
                    }
                    for(int i=0;i<this._RemovedSessoinList.Count;i++)
                    {
                        this._Logger.InfoFormat("remove session:{0}", this._RemovedSessoinList[i]);
                        RemoveHelper(this._RemovedSessoinList[i]);
                    }
                    this._RemovedSessoinList.Clear();
                }
                finally
                {
                    this._ReadWriteLock.ExitWriteLock();
                }
            }
        }

        private void RemoveHelper(Session session)
        {
            this.dict.Remove(session);
            ResouceManager.Default.ReleaseResource(session);
            AgentController.Default.EnqueueDisconnectSession(session);
            this._ClientCount--;
            this._Logger.InfoFormat("ClientCount={0}", this._ClientCount);
        }

    }
}
