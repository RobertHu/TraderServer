using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Trader.Server.Session
{
    public static  class SessionMapping
    {
        private static Dictionary<Guid, long> dict = new Dictionary<Guid, long>();
        private static ReaderWriterLockSlim _ReadWriteLock=new ReaderWriterLockSlim();
        private static object _Lock = new object();
        private static long _NextSequence = 0;
        public static long Add(Guid session)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                if (!dict.ContainsKey(session))
                {
                    _NextSequence++;
                    dict.Add(session, _NextSequence);
                }
                return _NextSequence;
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

        public static long Remove(Guid session)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                long result = -1;
                lock (_Lock)
                {
                    if (dict.ContainsKey(session))
                    {
                        result = dict[session];
                        dict.Remove(session);
                    }
                    return result;
                }
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

        public static long? Get(Guid session)
        {
            _ReadWriteLock.EnterReadLock();
            try
            {
                if (dict.ContainsKey(session))
                {
                    return dict[session];
                }
                return null;
            }
            finally
            {
                _ReadWriteLock.ExitReadLock();
            }
        }

    }
}
