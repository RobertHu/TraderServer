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
        private int _LastReadOffset = -1;
        private byte[] _LastReadBuffer;
        private bool _IsCustomerRead = false;
        private int _WriteOffset;
        private int _ReadOffset;
        public byte[] BufferInUsed{get;private set;}
        public TraderNetworkStream(Socket socket, byte[] buffer)
            : base(socket)
        {
            this.BufferInUsed = buffer;
            this._WriteOffset =BufferManager.TwoPartLength;
            this._ReadOffset = BufferManager.OnePartLength;
        }
        public TraderNetworkStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket) { }
        public TraderNetworkStream(Socket socket, FileAccess access) : base(socket, access) { }
        public TraderNetworkStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket) { }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            if (size <= BufferManager.WriteBufferSize)
            {
                Buffer.BlockCopy(buffer, offset, this.BufferInUsed, this._WriteOffset, size);
                return this.Socket.BeginSend(this.BufferInUsed, this._WriteOffset,size, SocketFlags.None , callback, state);
            }
            else
            {
                return this.Socket.BeginSend(buffer, offset,size, SocketFlags.None , callback, state);
            }
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.Socket.EndSend(asyncResult);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            if (size <= BufferManager.InnerReadBufferSize)
            {

                this._LastReadBuffer = buffer;
                this._LastReadOffset = offset;
                this._IsCustomerRead = true;
                return this.Socket.BeginReceive(this.BufferInUsed, this._ReadOffset, size,SocketFlags.None, callback, state);
            }
            else
            {
                this._IsCustomerRead = false;
                return this.Socket.BeginReceive(this.BufferInUsed, offset, size,SocketFlags.None, callback, state);
            }

        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int len = this.Socket.EndReceive(asyncResult);
            if (this._IsCustomerRead)
            {
                Buffer.BlockCopy(this.BufferInUsed, this._ReadOffset, this._LastReadBuffer, this._LastReadOffset, len);
            }
            return len;
        }
    }
}