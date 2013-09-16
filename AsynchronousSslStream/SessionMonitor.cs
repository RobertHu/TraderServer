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
        private readonly ILog _Logger = LogManager.GetLogger(typeof(SessionMonitor));
        private readonly TimeSpan _ExpiredTimeout;
        private long _ClientCount;
        private volatile bool _IsStarted;
        private volatile bool _IsStoped ;
        private readonly ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private readonly Dictionary<Session, DateTime> _Dict = new Dictionary<Session, DateTime>();
        private readonly List<Session> _RemovedSessoinList = new List<Session>(512);
        public SessionMonitor(TimeSpan timeout)
        {
            _ExpiredTimeout = timeout;
        }

        public void Start()
        {
            try
            {
                if (_IsStarted) return;
                Thread thread = new Thread(MonitorHandle) {IsBackground = true};
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
            _IsStoped = true;
        }


        public void Add(Session session)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                if (_Dict.ContainsKey(session)) return;
                _Dict.Add(session, DateTime.Now);
                _ClientCount++;
                _Logger.InfoFormat("ClientCount={0}", _ClientCount);
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

        public void Update(Session? session)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                if (!session.HasValue)
                    return;
                DateTime value;
                if (_Dict.TryGetValue(session.Value, out value))
                    _Dict[session.Value] = DateTime.Now;
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

        public void Remove(Session session)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                if (!_Dict.ContainsKey(session))
                    return;
                RemoveHelper(session);
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

        public bool Exist(Session session)
        {
            _ReadWriteLock.EnterReadLock();
            try
            {
                return _Dict.ContainsKey(session);
            }
            finally
            {
                _ReadWriteLock.ExitReadLock();
            }
        }

        private void MonitorHandle()
        {
            while (true)
            {
                if (_IsStoped)
                    break;
                Thread.Sleep(60000);
                _ReadWriteLock.EnterWriteLock();
                try
                {
                    foreach (var item in _Dict.Where(item => DateTime.Now - item.Value> _ExpiredTimeout))
                    {
                        _RemovedSessoinList.Add(item.Key);
                    }
                    foreach (Session session in _RemovedSessoinList)
                    {
                        _Logger.InfoFormat("remove session:{0}", session);
                        RemoveHelper(session);
                    }
                    _RemovedSessoinList.Clear();
                }
                finally
                {
                    _ReadWriteLock.ExitWriteLock();
                }
            }
        }

        private void RemoveHelper(Session session)
        {
            _Dict.Remove(session);
            ResouceManager.Default.ReleaseResource(session);
            AgentController.Default.EnqueueDisconnectSession(session);
            _ClientCount--;
            _Logger.InfoFormat("ClientCount={0}", _ClientCount);
        }

    }
}
