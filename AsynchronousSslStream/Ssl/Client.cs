using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using  Trader.Common;
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
        private ConcurrentQueue<byte[]> _Queue = new ConcurrentQueue<byte[]>();
        private bool _IsSendingData = false;
        private object _Lock = new object();
        private bool _HasPartialPacket = false;
        private int _PartialReadedLenth = 0;
        private long _Session;
        private const int MAX_WRITE_LENGTH = 10240;
        private const int FIRST_READ_COUNT = 1;
        private int _LastWriteIndex = 0;
        private byte[] _LastWriteBuffer;
        public Client() { }
        public Client(SslStream stream, long session)
        {
            this._Stream = stream;
            this._Session = session;
            Read();
        }
        public int BufferIndex { get; set; }

        public void Start(SslStream stream,long session)
        {
            this._IsClosed = false;
            this._Stream = stream;
            this._Session = session;
            byte[] packet;
            while (this._Queue.TryDequeue(out packet)) { }
            this._IsSendingData = false;
            this._HasPartialPacket = false;
            this._PartialReadedLenth = 0;
            Read();
        }


        public void Send(byte[] packet)
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
                this._Stream.BeginRead(BufferManager.Default.Buffer, this.BufferIndex, FIRST_READ_COUNT, this.EndRead, null);
            }
            catch (Exception ex)
            {
                this.Close();
            }
        }

        private void ProcessPackage(byte[] data,int offset,int len)
        {
            byte[] packet = new byte[len];
            Buffer.BlockCopy(data, offset, packet, 0, len);
            ReceiveData receiveData = ReceiveDataPool.Default.Pop();
            if (receiveData == null)
            {
                receiveData = new Common.ReceiveData(this._Session, packet);
            }
            else
            {
                receiveData.Session = this._Session;
                receiveData.Data = packet;
            }
            ReceiveCenter.Default.Send(receiveData);
        }

        private void EndRead(IAsyncResult ar)
        {

            Action action = new Action(() =>
            {
                try
                {
                    int lenFirst =this._Stream.EndRead(ar);
                    if (lenFirst <= 0)
                    {
                        Close();
                        return;
                    }
                    int count = this._Stream.Read(BufferManager.Default.Buffer, this.BufferIndex + lenFirst, BufferManager.OUTTER_READ_BUFFER_SIZE - lenFirst);
                    if(count <=0){
                        Close();
                        return;
                    }
                    int len = lenFirst + count;
                    int currentIndex = this.BufferIndex;
                    int used = 0;
                    byte[] buffer = BufferManager.Default.Buffer;
                    if (len <= 0)
                    {
                        Close();
                        return;
                    }
                    if (this._HasPartialPacket)
                    {
                        int partialStartIndex = this.BufferIndex + BufferManager.ThreePartLength;
                        int partialCurrentIndex = partialStartIndex + this._PartialReadedLenth;
                        if (this._PartialReadedLenth < Constants.HeadCount)
                        {
                            int needToRead = Constants.HeadCount - this._PartialReadedLenth;
                            if (needToRead > len)
                            {
                                Buffer.BlockCopy(buffer, currentIndex, buffer, partialCurrentIndex, len);
                                this._PartialReadedLenth += len;
                                return;
                            }
                            Buffer.BlockCopy(buffer, currentIndex, buffer, partialCurrentIndex, needToRead);
                            partialCurrentIndex += needToRead;
                            len -= needToRead;
                            used += needToRead;
                        }
                        int packetLength = Constants.GetPacketLength(buffer, partialStartIndex);
                        int howMuchNeedToRead = packetLength - Constants.HeadCount;
                        if (howMuchNeedToRead > len)
                        {
                            Buffer.BlockCopy(buffer, currentIndex, buffer, partialCurrentIndex, len);
                            return;
                        }
                        int offset = used + currentIndex;
                        Buffer.BlockCopy(buffer, offset, buffer, partialCurrentIndex, howMuchNeedToRead);
                        ProcessPackage(buffer, partialStartIndex, packetLength);
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
                            Buffer.BlockCopy(buffer, offset, buffer, this.BufferIndex + BufferManager.ThreePartLength, len);
                            this._PartialReadedLenth = len;
                            this._HasPartialPacket = true;
                            break;
                        }
                        int packetLength = Constants.GetPacketLength(buffer, currentIndex + used);
                        if (len < packetLength)
                        {
                            Buffer.BlockCopy(buffer, offset, buffer, this.BufferIndex + BufferManager.ThreePartLength, len);
                            this._PartialReadedLenth = len;
                            this._HasPartialPacket = true;
                            break;
                        }
                        ProcessPackage(buffer, offset, packetLength);
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
                byte[] packet;
                if (this._Queue.TryDequeue(out packet))
                {
                    BeginWrite(packet, 0, packet.Length);
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

        private void BeginWrite(byte[] data ,int offset, int len)
        {
            if (len <= MAX_WRITE_LENGTH)
            {
                this._Stream.BeginWrite(data, offset, len, this.EndWrite, null);
            }
            else
            {
                this._LastWriteBuffer = data;
                this._Stream.BeginWrite(data, offset, MAX_WRITE_LENGTH, this.EndWrite, null);
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
                        Write();
                        return;
                    }
                    this._LastWriteIndex += MAX_WRITE_LENGTH;
                    int len = this._LastWriteBuffer.Length - this._LastWriteIndex;
                    if (len <= 0)
                    {
                        this._LastWriteBuffer = null;
                        this._LastWriteIndex = 0;
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
            if (this._IsClosed )
            {
                return;
            }
            this._Stream.Close();
            this._IsClosed = true;
            AgentController.Default.EnqueueDisconnectSession(this._Session);
            BufferManager.Default.FreeBuffer(this.BufferIndex);
        }


    }
}
