using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Trader.Common
{
    public static  class SessionMapping
    {
        public const long INVALID_VALUE = 0;
        private static long _NextSequence = 0;
        private static ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private static Dictionary<string, long> _dict = new Dictionary<string, long>();
        public static long Get()
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                _NextSequence++;
                _dict.Add(_NextSequence.ToString(), _NextSequence);
                return _NextSequence;
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
               
        }

        public static long Get(string session)
        {
            _ReadWriteLock.EnterReadLock();
            try
            {
                if (_dict.ContainsKey(session))
                {
                    return _dict[session];
                }
                return 0;
            }
            finally
            {
                _ReadWriteLock.ExitReadLock();
            }
        }

    }
}
