﻿using System;
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
                return this.Socket.BeginSend(BufferManager.Default.Buffer, this._WriteOffset,size, SocketFlags.None , callback, state);
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
            if (size <= BufferManager.INNER_READ_BUFFER_SIZE)
            {

                this._LastReadBuffer = buffer;
                this._LastReadOffset = offset;
                this._IsCustomerRead = true;
                return this.Socket.BeginReceive(BufferManager.Default.Buffer, this._ReadOffset, size,SocketFlags.None, callback, state);
            }
            else
            {
                this._IsCustomerRead = false;
                return this.Socket.BeginReceive(buffer, offset, size,SocketFlags.None, callback, state);
            }

        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int len = this.Socket.EndReceive(asyncResult);
            if (this._IsCustomerRead)
            {
                Buffer.BlockCopy(BufferManager.Default.Buffer, this._ReadOffset, this._LastReadBuffer, this._LastReadOffset, len);
            }
            return len;
        }

        public override int Read( byte[] buffer, int offset, int size)
        {
            if (size <= BufferManager.INNER_READ_BUFFER_SIZE)
            {
                int len = base.Read(BufferManager.Default.Buffer, this._ReadOffset, size);
                Buffer.BlockCopy(BufferManager.Default.Buffer, this._ReadOffset, buffer, offset, len);
                return len;
            }
            return base.Read(buffer, offset, size);
        }
    }
}