using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public class BufferManager
    {
        private const int CONNECTION_COUNT = 8000;
        public const int INNER_READ_BUFFER_SIZE = 4096;
        public const int WRITE_BUFFER_SIZE = 4096;
        public const int OUTTER_READ_BUFFER_SIZE = 3072;
        public const int PREVIOUS_PATIAL_PACKET_SIZE = 3072;
        private int _NumBytes;
        private const int CAPACITY = 8000;
        private Stack<int> _FreeIndexPool = new Stack<int>(CAPACITY);
        private int _CurrentIndex=0;
        private object _Lock = new object();
        private BufferManager()
        {
            this._NumBytes = CONNECTION_COUNT * GetWholePartLength();
            this.Buffer = new byte[this._NumBytes];
        }

        //outer read / inner read /write
        private int GetWholePartLength()
        {
            return OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE + WRITE_BUFFER_SIZE + PREVIOUS_PATIAL_PACKET_SIZE;
        }

        public static readonly BufferManager Default = new BufferManager();

        public const int OnePartLength = OUTTER_READ_BUFFER_SIZE;
        public const int TwoPartLength = OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE;
        public const int ThreePartLength = OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE + WRITE_BUFFER_SIZE;
        public const int FourPartLength = OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE + WRITE_BUFFER_SIZE + PREVIOUS_PATIAL_PACKET_SIZE;


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
                    this._CurrentIndex += GetWholePartLength();
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
