using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Security;
using Trader.Common;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using log4net;
using Trader.Server._4BitCompress;
using iExchange.Common;
using Trader.Server.Session;
using Serialization;
namespace Trader.Server.Ssl
{
    public class Client
    {
        private const int CAPACITY = 1000;
        private static ILog _Logger = LogManager.GetLogger(typeof(Client));
        private SslStream _Stream;
        private volatile bool _IsClosed = false;
        private Queue<CommandForClient> _Queue = new Queue<CommandForClient>(CAPACITY);
        private object _QueueLock = new object();
        private bool _IsSendingData = false;
        private bool _HasPartialPacket = false;
        private int _PartialReadedLenth = 0;
        private long _Session;
        private const int MAX_WRITE_LENGTH = 10240;
        private int _LastWriteIndex = 0;
        private UnmanagedMemory _LastWriteBuffer;
        private byte[] _Buffer;
        private int _WriteBufferIndex;
        private int _PartialReadIndex;
        private CommandForClient _CurrentCommand;
        private UnmanagedMemory _CurrentPacket;
        private List<QuotationCommand> _QuotationQueue = new List<QuotationCommand>(20);

        public Client(SslStream stream, long session,byte[] buffer)
        {
            this._Stream = stream;
            this._Session = session;
            this._Buffer = buffer;
            this._WriteBufferIndex = BufferManager.TwoPartLength;
            this._PartialReadIndex = BufferManager.ThreePartLength;
            Read();
        }


        private int GetCurrentPartialReadIndex()
        {
            return this._PartialReadIndex + this._PartialReadedLenth;
        }




        public void Send(CommandForClient command)
        {
            if (this._IsClosed)
            {
                return;
            }
            lock (this._QueueLock)
            {
                this._Queue.Enqueue(command);
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
                    int used = 0;
                    if (len <= 0)
                    {
                        Close();
                        return;
                    }
                    if (this._HasPartialPacket)
                    {
                        if (this._PartialReadedLenth < Constants.HeadCount)
                        {
                            int needToRead = Constants.HeadCount - this._PartialReadedLenth;
                            if (needToRead > len)
                            {
                                Buffer.BlockCopy(this._Buffer, used, this._Buffer,GetCurrentPartialReadIndex() , len);
                                this._PartialReadedLenth += len;
                                Read();
                                return;
                            }
                            Buffer.BlockCopy(this._Buffer, used, this._Buffer, GetCurrentPartialReadIndex(), needToRead);
                            len -= needToRead;
                            used += needToRead;
                            this._PartialReadedLenth += needToRead;
                        }
                        int packetLength = Constants.GetPacketLength(this._Buffer, this._PartialReadIndex);
                        int howMuchNeedToRead = packetLength - this._PartialReadedLenth;
                        if (howMuchNeedToRead > len)
                        {
                            Buffer.BlockCopy(this._Buffer, used, this._Buffer, GetCurrentPartialReadIndex(), len);
                            this._PartialReadedLenth += len;
                            Read();
                            return;
                        }
                        Buffer.BlockCopy(this._Buffer, used, this._Buffer, GetCurrentPartialReadIndex(), howMuchNeedToRead);
                        ProcessPackage(this._Buffer, this._PartialReadIndex, packetLength);
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

                        if (len < Constants.HeadCount)
                        {
                            Buffer.BlockCopy(this._Buffer, used, this._Buffer,  this._PartialReadIndex, len);
                            this._PartialReadedLenth = len;
                            this._HasPartialPacket = true;
                            break;
                        }
                        int packetLength = Constants.GetPacketLength(this._Buffer,  used);
                        if (len < packetLength)
                        {
                            Buffer.BlockCopy(this._Buffer, used, this._Buffer,  this._PartialReadIndex, len);
                            this._PartialReadedLenth = len;
                            this._HasPartialPacket = true;
                            break;
                        }
                        ProcessPackage(this._Buffer, used, packetLength);
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
                lock (this._QueueLock)
                {
                    if (this._Queue.Count > 0)
                    {
                        this._CurrentCommand = this._Queue.Dequeue();
                        if (this._CurrentCommand.CommandType == DataType.Command)
                        {
                            WriteForCommand();
                        }
                        else if (this._CurrentCommand.CommandType == DataType.Response)
                        {
                            this._CurrentPacket = this._CurrentCommand.Data;
                            this.BeginWrite(this._CurrentPacket, 0, this._CurrentPacket.Length);
                        }
                        else
                        {
                            WriteForQuotation();
                        }
                    }
                    else
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

        private void WriteForQuotation()
        {
            Token token;
            TraderState state = SessionManager.Default.GetTokenAndState(this._Session, out token);
            if (state == null || token == null)
            {
                Write();
                return;
            }
            this._QuotationQueue.Clear();
            this._QuotationQueue.Add(this._CurrentCommand.Quotation.QuotationCommand);
            while (this._Queue.Count > 0)
            {
                CommandForClient item = this._Queue.Peek();
                if (item.CommandType != DataType.Quotation)
                {
                    break;
                }
                item = this._Queue.Dequeue();
                this._QuotationQueue.Add(item.Quotation.QuotationCommand);
            }
            byte[] data;
            if (this._QuotationQueue.Count == 1)
            {
                data = this._CurrentCommand.Quotation.GetPriceInBytes(token, state);
            }
            else
            {
                QuotationCommand command = new QuotationCommand();
                command.Merge(this._QuotationQueue);
                data = Quotation4Bit.GetPriceInBytes(token, state, command, this._QuotationQueue[0].Sequence, this._QuotationQueue[this._QuotationQueue.Count - 1].Sequence);
            }
            if (data == null)
            {
                Write();
                return;
            }
            this._CurrentPacket = SerializeManager.Default.SerializePrice(data);
            this.BeginWrite(this._CurrentPacket, 0, this._CurrentPacket.Length);

        }

        private void WriteForCommand()
        {
            Token token;
            TraderState state = SessionManager.Default.GetTokenAndState(this._Session, out token);
            if (state == null || token == null)
            {
                Write();
                return;
            }
            byte[] data = Quotation4Bit.GetDataForCommand(token, state, this._CurrentCommand.Command);
            if (data == null)
            {
                Write();
                return;
            }
            this._CurrentPacket = SerializeManager.Default.SerializeCommand(data);
            this.BeginWrite(this._CurrentPacket, 0, this._CurrentPacket.Length);
        }

        

        private unsafe void BeginWrite(UnmanagedMemory data, int offset, int len)
        {
            try
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
            catch (Exception ex)
            {
                _Logger.Error("client closed",ex);
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
                    _Logger.Error("client closed",ex);
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