using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public class BufferManager
    {
        private const int CONNECTION_COUNT = 500;
        public const int INNER_READ_BUFFER_SIZE = 4096;
        public const int WRITE_BUFFER_SIZE = 13240;
        public const int OUTTER_READ_BUFFER_SIZE = 3072;
        public const int PREVIOUS_PATIAL_PACKET_SIZE = 3072;
        private const int CAPACITY = 8000;
        private Queue<byte[]> _BufferPool = new Queue<byte[]>(CAPACITY);
        private object _Lock = new object();
        private BufferManager()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                this._BufferPool.Enqueue(new byte[WholePartLength]);
            }
        }

        //outer read / inner read /write

        public static readonly BufferManager Default = new BufferManager();

        public const int OnePartLength = OUTTER_READ_BUFFER_SIZE;
        public const int TwoPartLength = OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE;
        public const int ThreePartLength = OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE + WRITE_BUFFER_SIZE;
        public const int WholePartLength = OUTTER_READ_BUFFER_SIZE + INNER_READ_BUFFER_SIZE + WRITE_BUFFER_SIZE + PREVIOUS_PATIAL_PACKET_SIZE;

        public byte[] Dequeue()
        {
            lock (this._Lock)
            {
                if (this._BufferPool.Count > 0)
                {
                    return this._BufferPool.Dequeue();
                }
                return new byte[WholePartLength];
            }
        }

        public void Enqueue(byte[] buffer)
        {
            lock (this._Lock)
            {
                this._BufferPool.Enqueue(buffer);
            }
        }
        
        
    }
}
