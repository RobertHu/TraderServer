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
        private ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private Dictionary<long, DateTime> dict = new Dictionary<long, DateTime>();
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


        public void Add(long session)
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

        public void Update(long? session)
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

        public void Remove(long session)
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

        public bool Exist(long session)
        {
            this._ReadWriteLock.EnterReadLock();
            try
            {
                DateTime value;
                return this.dict.TryGetValue(session, out value);
            }
            finally
            {
                this._ReadWriteLock.ExitReadLock();
            }
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
                this._ReadWriteLock.EnterWriteLock();
                try
                {
                    var target = this.dict.Where(p => DateTime.Now - p.Value > this._ExpiredTimeout).ToArray();
                    foreach (var item in target)
                    {
                        this._Logger.InfoFormat("remove session:{0}", item.Key);
                        RemoveHelper(item.Key);
                    }
                }
                finally
                {
                    this._ReadWriteLock.ExitWriteLock();
                }
            }
        }

        private void RemoveHelper(long session)
        {
            this.dict.Remove(session);
            ResouceManager.Default.ReleaseResource(session);
            AgentController.Default.EnqueueDisconnectSession(session);
            this._ClientCount--;
            this._Logger.InfoFormat("ClientCount={0}", this._ClientCount);
        }

    }
}
