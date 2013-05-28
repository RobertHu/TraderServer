using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public class ReceiveDataPool
    {
        private const int CAPACITY = 5000;
        private Stack<ReceiveData> _Pool = new Stack<ReceiveData>(CAPACITY);
        private object _Lock = new object();
        private ReceiveDataPool() { }
        public static readonly ReceiveDataPool Default = new ReceiveDataPool();
        public void Push(ReceiveData data)
        {
            lock (this._Lock)
            {
                this._Pool.Push(data);
            }
        }

        public ReceiveData Pop()
        {
            lock (this._Lock)
            {
                if (this._Pool.Count > 0)
                {
                    return this._Pool.Pop();
                }
                return null;
            }
        }
    }
}
