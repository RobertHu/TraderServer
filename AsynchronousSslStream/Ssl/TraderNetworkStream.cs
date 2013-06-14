using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Permissions;
using Trader.Common;

namespace Trader.Server.Ssl
{
    public class TraderNetworkStream : NetworkStream
    {
        public int BufferIndex { get; set; }
        private int _LastReadOffset = -1;
        private byte[] _LastReadBuffer;
        public TraderNetworkStream(Socket socket) : base(socket) { }
        public TraderNetworkStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket) { }
        public TraderNetworkStream(Socket socket, FileAccess access) : base(socket, access) { }
        public TraderNetworkStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket) { }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            int currentOffset = this.BufferIndex + BufferManager.TwoPartLength;
            Buffer.BlockCopy(buffer, offset, BufferManager.Default.Buffer, currentOffset, size);
            return base.BeginWrite(BufferManager.Default.Buffer, currentOffset, size, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            this._LastReadBuffer = buffer;
            this._LastReadOffset = offset;
            int currentOffset = this.BufferIndex + BufferManager.OnePartLength + offset;
            return base.BeginRead(BufferManager.Default.Buffer, currentOffset, size, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int len = base.EndRead(asyncResult);
            int currentOffset = this.BufferIndex + BufferManager.OnePartLength + this._LastReadOffset;
            Buffer.BlockCopy(BufferManager.Default.Buffer, currentOffset, this._LastReadBuffer, this._LastReadOffset,len);
            return len;
        }
    }
}
