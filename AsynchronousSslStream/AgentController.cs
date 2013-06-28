using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Trader.Helper;
using Trader.Server.Bll;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Trader.Server.Session;
using iExchange.Common;
using Trader.Common;
using Serialization;
using Trader.Server.Ssl;
using CommunicationAgent = Trader.Helper.Common.ICommunicationAgent;
using Trader.Server._4BitCompress;
using Trader.Server.Service;
namespace Trader.Server
{
    public enum DataType
    {
        Quotation,
        Command,
        Response
    }

    public struct CommandForClient
    {
        private Quotation4Bit _Quotation;
        private Command _Command;
        private DataType _CommandType;
        private UnmanagedMemory _Data;
        public CommandForClient(UnmanagedMemory data=null,QuotationCommand quotationCommand=null, Command command = null)
        {
            if (command != null)
            {
                this._Command = command;
                this._Quotation = null;
                this._Data=null;
                this._CommandType = DataType.Command;
            }
            else if (quotationCommand != null)
            {
                this._Quotation = new Quotation4Bit(quotationCommand);
                this._Command = null;
                this._Data = null;
                this._CommandType = DataType.Quotation;
            }
            else
            {
                this._Data = data;
                this._Command = null;
                this._Quotation = null;
                this._CommandType = DataType.Response;
            }
        }

        public Quotation4Bit Quotation { get { return this._Quotation; } }
        public Command Command { get { return this._Command; } }
        public DataType CommandType { get { return this._CommandType; } }
        public UnmanagedMemory Data { get { return this._Data; } }
    }


    public class AgentController
    {
        public static readonly AgentController Default = new AgentController();
        private ILog _Logger = LogManager.GetLogger(typeof(AgentController));
        private ConcurrentDictionary<long, ClientRelation> _Container = new ConcurrentDictionary<long, ClientRelation>();
        private AutoResetEvent _DisconnectEvent = new AutoResetEvent(false);
        private AutoResetEvent _SendQuotationEvent = new AutoResetEvent(false);
        private ConcurrentQueue<long> _DisconnectQueue = new ConcurrentQueue<long>();
        private ConcurrentQueue<CommandForClient> _Commands = new ConcurrentQueue<CommandForClient>();
        private volatile bool _Started = false;
        private volatile bool _Stopped = false;
        private CommandForClient _Current;
        private AgentController()
        {
        }

        public void Add(long session, ReceiveAgent receiver, Client sender)
        {
            this._Container.TryAdd(session, new ClientRelation(sender, receiver));
        }


        public void Remove(long session)
        {
            ClientRelation relation;
            this._Container.TryRemove(session, out relation);
        }

        public bool RecoverConnection(long originSession, long currentSession)
        {
            ClientRelation relation;
            this._Container.TryRemove(originSession, out relation);
            ClientRelation currentRelation;
            if (this._Container.TryRemove(currentSession, out currentRelation))
            {
                this._Container.TryAdd(originSession, currentRelation);
                currentRelation.Sender.UpdateSession(originSession);
                return true;
            }
            return false;
        }

        public Client GetSender(long session)
        {
            Client result = null;
            ClientRelation relation;
            if (this._Container.TryGetValue(session, out relation))
            {
                result = relation.Sender;
            }
            return result;
        }


        public ReceiveAgent GetReceiver(long session)
        {
            ReceiveAgent result = null;
            ClientRelation relation;
            if (this._Container.TryGetValue(session, out relation))
            {
                result = relation.Receiver;
            }
            return result;
        }

        public void Start()
        {
            try
            {
                if (this._Started)
                {
                    return;
                }

                Thread thread = new Thread(this.DisconnectHandle);
                thread.IsBackground = true;
                thread.Start();

                Thread quotationThread = new Thread(this.Dispatch);
                quotationThread.IsBackground = true;
                quotationThread.Start();

                this._Started = true;
            }
            catch (Exception ex)
            {
                this._Logger.Error(ex);
            }
        }

        public void Stop()
        {
            this._Stopped = true;
        }

        public void EnqueueDisconnectSession(long session)
        {
            this._DisconnectQueue.Enqueue(session);
            this._DisconnectEvent.Set();
        }


        private void DisconnectHandle()
        {
            while (true)
            {
                if (this._Stopped)
                {
                    break;
                }
                this._DisconnectEvent.WaitOne();
                while (this._DisconnectQueue.Count != 0)
                {
                    if (this._Stopped)
                    {
                        break;
                    }
                    long session;
                    if (this._DisconnectQueue.TryDequeue(out session))
                    {
                        this.Remove(session);
                    }
                }
            }
        }

        public void SendCommand(QuotationCommand quotationCommand=null,Command command =null)
        {
            if (this._Container.Count == 0)
            {
                return;
            }
            CommandForClient target = new CommandForClient(quotationCommand:quotationCommand,command:command);
            this._Commands.Enqueue(target);
            this._SendQuotationEvent.Set();
        }

        private void Dispatch()
        {
            while (true)
            {
                if (this._Stopped)
                {
                    break;
                }
                this._SendQuotationEvent.WaitOne();
                while (this._Commands.TryDequeue(out this._Current))
                {
                    Parallel.ForEach(this._Container, this.SendCommandHandler);
                }
            }

        }


        private void SendCommandHandler(KeyValuePair<long, ClientRelation> p)
        {
            p.Value.Sender.Send(this._Current);
        }



    }

    public struct ClientRelation
    {
        private ReceiveAgent _Receiver;
        private Client _Sender;
        public ClientRelation(Client sender, ReceiveAgent receiver)
        {
            this._Receiver = receiver;
            this._Sender = sender;
        }
        public ReceiveAgent Receiver { get { return this._Receiver; } }
        public Client Sender { get { return this._Sender; } }
    }
}