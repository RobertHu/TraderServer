using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Trader.Common
{
    public static  class SessionMapping
    {
        private static long _NextSequence;
        private static ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, Session> _Dict = new Dictionary<string, Session>();
        public static Session Get()
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                _NextSequence++;
                Session session = new Session(_NextSequence);
                _Dict.Add(_NextSequence.ToString(), session);
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
                return _Dict.ContainsKey(session) ? _Dict[session] : Session.InvalidValue;
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
                if (!_Dict.ContainsKey(session))
                {
                    return;
                }
                _Dict.Remove(session);
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

    }
}
