using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Trader.Server._4BitCompress
{
    public class GuidMapping
    {

        private static readonly ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private static int _NextSequence;
        private static readonly Dictionary<Guid, int> _Sequenes = new Dictionary<Guid, int>();

        public static int Add(Guid id)
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                int result;
                if (_Sequenes.ContainsKey(id))
                {
                    result = _Sequenes[id];
                }
                else
                {
                    _NextSequence++;
                    _Sequenes.Add(id, _NextSequence);
                    result = _NextSequence;
                }
                return result;
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

        public static int Get(Guid id)
        {
            _ReadWriteLock.EnterReadLock();
            try
            {
                int result = -1;
                if (_Sequenes.ContainsKey(id))
                {
                    result = _Sequenes[id];
                }
                return result;
            }
            finally
            {
                _ReadWriteLock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _ReadWriteLock.EnterWriteLock();
            try
            {
                _Sequenes.Clear();
                _NextSequence = 0;
            }
            finally
            {
                _ReadWriteLock.ExitWriteLock();
            }
        }

    }
}