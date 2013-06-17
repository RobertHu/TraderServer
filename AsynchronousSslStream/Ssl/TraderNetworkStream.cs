using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Permissions;
using Trader.Common;
using System.Threading;
using log4net;

namespace Trader.Server.Ssl
{
    public class TraderNetworkStream : NetworkStream
    {
        private int _BufferIndex;
        private int _LastReadOffset = -1;
        private byte[] _LastReadBuffer;
        private bool _IsCustomerRead = false;
        private int _WriteOffset;
        private int _ReadOffset;
        public int BufferIndex { get { return this._BufferIndex; } }
        public TraderNetworkStream(Socket socket, int bufferIndex)
            : base(socket)
        {
            this._BufferIndex = bufferIndex;
            this._WriteOffset = this._BufferIndex + BufferManager.TwoPartLength;
            this._ReadOffset = this._BufferIndex + BufferManager.OnePartLength;
        }
        public TraderNetworkStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket) { }
        public TraderNetworkStream(Socket socket, FileAccess access) : base(socket, access) { }
        public TraderNetworkStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket) { }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            if (size <= BufferManager.WRITE_BUFFER_SIZE)
            {
                Buffer.BlockCopy(buffer, offset, BufferManager.Default.Buffer, this._WriteOffset, size);
                return base.BeginWrite(BufferManager.Default.Buffer, this._WriteOffset, size, callback, state);
            }
            else
            {
                return base.BeginWrite(buffer, offset, size, callback, state);
            }
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            if (size <= BufferManager.INNER_READ_BUFFER_SIZE)
            {

                this._LastReadBuffer = buffer;
                this._LastReadOffset = offset;
                this._IsCustomerRead = true;
                return base.BeginRead(BufferManager.Default.Buffer, this._ReadOffset, size, callback, state);
            }
            else
            {
                this._IsCustomerRead = false;
                return base.BeginRead(buffer, offset, size, callback, state);
            }

        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int len = base.EndRead(asyncResult);
            if (this._IsCustomerRead)
            {
                Buffer.BlockCopy(BufferManager.Default.Buffer, this._ReadOffset, this._LastReadBuffer, this._LastReadOffset, len);
            }
            return len;
        }
    }
}