using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using Trader.Common;
using System.Threading;
using System.Collections.Concurrent;
using Trader.Helper;
using System.Threading.Tasks;
using log4net;
namespace Trader.Server.Ssl
{
    public class Client
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(Client));
        private SslStream _Stream;
        private volatile bool _IsClosed = false;
        private ConcurrentQueue<UnmanagedMemory> _Queue = new ConcurrentQueue<UnmanagedMemory>();
        private bool _IsSendingData = false;
        private object _Lock = new object();
        private bool _HasPartialPacket = false;
        private int _PartialReadedLenth = 0;
        private long _Session;
        private const int MAX_WRITE_LENGTH = 10240;
        private int _LastWriteIndex = 0;
        private UnmanagedMemory _LastWriteBuffer;
        private byte[] _Buffer;
        private int _WriteBufferIndex;
        private UnmanagedMemory _CurrentPacket;
        public Client() { }
        public Client(SslStream stream, long session)
        {
            this._Stream = stream;
            this._Session = session;
            Read();
        }

        public void SetBuffer(byte[] buffer)
        {
            this._Buffer = buffer;
            this._WriteBufferIndex = BufferManager.TwoPartLength;
        }

        public void Start(SslStream stream, long session)
        {
            this._IsClosed = false;
            this._Stream = stream;
            this._Session = session;
            UnmanagedMemory packet;
            while (this._Queue.TryDequeue(out packet)) { }
            this._IsSendingData = false;
            this._HasPartialPacket = false;
            this._PartialReadedLenth = 0;
            Read();
        }


        public void Send(UnmanagedMemory packet)
        {
            if (this._IsClosed)
            {
                return;
            }
            this._Queue.Enqueue(packet);
            lock (this._Lock)
            {
                if (!_IsSendingData)
                {
                    this._IsSendingData = true;
                    Write();
                }
            }
        }

        private void Read()
        {
            try
            {
                if (this._IsClosed)
                {
                    return;
                }
                this._Stream.BeginRead(this._Buffer, 0, BufferManager.OUTTER_READ_BUFFER_SIZE, this.EndRead, null);
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }

        private void ProcessPackage(byte[] data, int offset, int len)
        {
            byte[] packet = new byte[len];
            Buffer.BlockCopy(data, offset, packet, 0, len);
            ReceiveData receiveData = new ReceiveData(this._Session, packet);
            ReceiveCenter.Default.Send(receiveData);
        }

        private void EndRead(IAsyncResult ar)
        {

            Action action = new Action(() =>
            {
                try
                {
                    int len = this._Stream.EndRead(ar);
                    int currentIndex = 0;
                    int used = 0;
                    if (len <= 0)
                    {
                        Close();
                        return;
                    }
                    if (this._HasPartialPacket)
                    {
                        int partialStartIndex = BufferManager.ThreePartLength;
                        int partialCurrentIndex = partialStartIndex + this._PartialReadedLenth;
                        if (this._PartialReadedLenth < Constants.HeadCount)
                        {
                            int needToRead = Constants.HeadCount - this._PartialReadedLenth;
                            if (needToRead > len)
                            {
                                Buffer.BlockCopy(this._Buffer, currentIndex, this._Buffer, partialCurrentIndex, len);
                                this._PartialReadedLenth += len;
                                return;
                            }
                            Buffer.BlockCopy(this._Buffer, currentIndex, this._Buffer, partialCurrentIndex, needToRead);
                            partialCurrentIndex += needToRead;
                            len -= needToRead;
                            used += needToRead;
                        }
                        int packetLength = Constants.GetPacketLength(this._Buffer, partialStartIndex);
                        int howMuchNeedToRead = packetLength - Constants.HeadCount;
                        if (howMuchNeedToRead > len)
                        {
                            Buffer.BlockCopy(this._Buffer, currentIndex, this._Buffer, partialCurrentIndex, len);
                            return;
                        }
                        int offset = used + currentIndex;
                        Buffer.BlockCopy(this._Buffer, offset, this._Buffer, partialCurrentIndex, howMuchNeedToRead);
                        ProcessPackage(this._Buffer, partialStartIndex, packetLength);
                        len -= howMuchNeedToRead;
                        used += howMuchNeedToRead;
                    }
                    this._HasPartialPacket = false;
                    this._PartialReadedLenth = 0;
                    while (true)
                    {
                        if (len <= 0)
                        {
                            break;
                        }

                        int offset = currentIndex + used;
                        if (len < Constants.HeadCount)
                        {
                            Buffer.BlockCopy(this._Buffer, offset, this._Buffer,  BufferManager.ThreePartLength, len);
                            this._PartialReadedLenth = len;
                            this._HasPartialPacket = true;
                            break;
                        }
                        int packetLength = Constants.GetPacketLength(this._Buffer, currentIndex + used);
                        if (len < packetLength)
                        {
                            Buffer.BlockCopy(this._Buffer, offset, this._Buffer,  BufferManager.ThreePartLength, len);
                            this._PartialReadedLenth = len;
                            this._HasPartialPacket = true;
                            break;
                        }
                        ProcessPackage(this._Buffer, offset, packetLength);
                        used += packetLength;
                        len -= packetLength;
                    }
                    Read();

                }

                catch (Exception ex)
                {
                    this.Close();
                }
            });
            Task task = new Task(action);
            TaskQueue.Default.Enqueue(task);

        }

        public void UpdateSession(long session)
        {
            this._Session = session;
        }

        private void Write()
        {
            try
            {
                if (this._Queue.TryDequeue(out this._CurrentPacket))
                {
                    BeginWrite(this._CurrentPacket, 0, this._CurrentPacket.Length);
                }
                else
                {
                    lock (this._Lock)
                    {
                        this._IsSendingData = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                this.Close();
            }
        }

        private unsafe void BeginWrite(UnmanagedMemory data, int offset, int len)
        {
            if (data.Data != null) // keep alive data
            {
                this._Stream.BeginWrite(data.Data, 0, data.Data.Length, this.EndWrite, null);
                return;
            }
            if (len <= MAX_WRITE_LENGTH)
            {
                for (int i = 0; i < len; i++)
                {
                    this._Buffer[this._WriteBufferIndex + i] = data.Handle[offset + i];
                }
                this._Stream.BeginWrite(this._Buffer, this._WriteBufferIndex, len, this.EndWrite, null);
            }
            else
            {
                this._LastWriteBuffer = data;
                for (int i = 0; i < MAX_WRITE_LENGTH; i++)
                {
                    this._Buffer[this._WriteBufferIndex + i] = data.Handle[offset + i];
                }
                this._Stream.BeginWrite(this._Buffer, this._WriteBufferIndex, MAX_WRITE_LENGTH, this.EndWrite, null);
            }
        }



        private void EndWrite(IAsyncResult ar)
        {
            Action action = new Action(() =>
            {
                try
                {
                    this._Stream.EndWrite(ar);
                    if (this._LastWriteBuffer == null)
                    {
                        this._CurrentPacket.Dispose();
                        Write();
                        return;
                    }
                    this._LastWriteIndex += MAX_WRITE_LENGTH;
                    int len = this._LastWriteBuffer.Length - this._LastWriteIndex;
                    if (len <= 0)
                    {
                        this._LastWriteBuffer = null;
                        this._LastWriteIndex = 0;
                        this._CurrentPacket.Dispose();
                        Write();
                        return;
                    }
                    int lenToBeWrite = len >= MAX_WRITE_LENGTH ? MAX_WRITE_LENGTH : len;
                    this.BeginWrite(this._LastWriteBuffer, this._LastWriteIndex, lenToBeWrite);
                }
                catch (Exception ex)
                {
                    _Logger.Error(ex);
                    this.Close();
                }
            });
            Task task = new Task(action);
            TaskQueue.Default.Enqueue(task);
        }

        private void Close()
        {
            if (this._IsClosed)
            {
                return;
            }
            this._Stream.Close();
            this._IsClosed = true;
            AgentController.Default.EnqueueDisconnectSession(this._Session);
            BufferManager.Default.Enqueue(this._Buffer);
        }


    }
}