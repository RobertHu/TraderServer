using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public class BufferManager
    {
        private const int CONNECTION_COUNT = 8000;
        public const int BUFFER_SIZE = 2048;
        private int _NumBytes;
        private const int CAPACITY = 8000;
        private Stack<int> _FreeIndexPool = new Stack<int>(CAPACITY);
        private int _CurrentIndex=0;
        private object _Lock = new object();
        private BufferManager()
        {
            this._NumBytes = CONNECTION_COUNT * BUFFER_SIZE;
            this.Buffer = new byte[this._NumBytes];
        }

        public static readonly BufferManager Default = new BufferManager();

        public int SetBuffer()
        {
            lock (this._Lock)
            {
                int index;
                if (this._FreeIndexPool.Count > 0)
                {
                    index = this._FreeIndexPool.Pop();
                }
                else
                {
                    index = this._CurrentIndex;
                    this._CurrentIndex += BUFFER_SIZE;
                }
                return index;
            }
        }

        public void FreeBuffer(int index)
        {
            lock (this._Lock)
            {
                this._FreeIndexPool.Push(index);
            }
        }

        public byte[] Buffer { get; private set; }



    }
}
