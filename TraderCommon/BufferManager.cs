using System.Collections.Generic;

namespace Trader.Common
{
    public class BufferManager
    {
        public static readonly BufferManager Default = new BufferManager();
        public const int InnerReadBufferSize = 4096;
        public const int WriteBufferSize = 13240;
        public const int OutterReadBufferSize = 3072;
        public const int PreviousPatialPacketSize = 3072;
        private const int CAPACITY = 8000;
        private readonly Queue<byte[]> _BufferPool = new Queue<byte[]>(CAPACITY);
        private readonly object _Lock = new object();
        private BufferManager()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                this._BufferPool.Enqueue(new byte[WholePartLength]);
            }
        }

        //outer read / inner read /write
        public const int OnePartLength = OutterReadBufferSize;
        public const int TwoPartLength = OutterReadBufferSize + InnerReadBufferSize;
        public const int ThreePartLength = OutterReadBufferSize + InnerReadBufferSize + WriteBufferSize;
        public const int WholePartLength = OutterReadBufferSize + InnerReadBufferSize + WriteBufferSize + PreviousPatialPacketSize;

        public byte[] Dequeue()
        {
            lock (this._Lock)
            {
                return this._BufferPool.Count > 0 ?
                    this._BufferPool.Dequeue() :
                    new byte[WholePartLength];
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
