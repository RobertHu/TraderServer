using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Trader.Common
{
    public static  class SessionMapping
    {
        private static long _NextSequence = 0;
        private static ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private static Dictionary<string, Session> _dict = new Dictionary<string, Session>();
        public static Session Get()
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                _NextSequence++;
                Session session = new Session(_NextSequence);
                _dict.Add(_NextSequence.ToString(), session);
                return session;
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
               
        }

        public static Session Get(string session)
        {
            _ReadWriteLock.EnterReadLock();
            try
            {
                if (_dict.ContainsKey(session))
                {
                    return _dict[session];
                }
                return Session.INVALID_VALUE;
            }
            finally
            {
                _ReadWriteLock.ExitReadLock();
            }
        }

        public static void Remove(string session)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                if (!_dict.ContainsKey(session))
                {
                    return;
                }
                _dict.Remove(session);
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

    }
}
