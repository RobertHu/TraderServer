using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
namespace AsyncSslServer
{
    public class SessionMonitor
    {
        private ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private ILog _Logger = LogManager.GetLogger(typeof(SessionMonitor));
        private TimeSpan _ExpiredTimeout;
        private volatile bool _IsStarted = false;
        private volatile bool _IsStoped = false;
        private Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
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


        public void Add(string session)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (!this.dict.ContainsKey(session))
                {
                    this.dict.Add(session, DateTime.Now);
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public void Update(string session)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (this.dict.ContainsKey(session))
                {
                    dict[session] = DateTime.Now;
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public void Remove(string session)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (this.dict.ContainsKey(session))
                {
                    RemoveHelper(session);
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public bool Exist(string session)
        {
            try
            {
                this._ReadWriteLock.EnterReadLock();
                return this.dict.ContainsKey(session);
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
                try
                {
                    this._ReadWriteLock.EnterWriteLock();
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

        private void RemoveHelper(string session)
        {
            this.dict.Remove(session);
            ResouceManager.Default.ReleaseResource(session);
            ThreadPool.QueueUserWorkItem(s =>
            {
                Thread.Sleep(1000);
                AgentController.Default.EnqueueDisconnectSession(Guid.Parse(session));
            });

                
        }


    }
}
