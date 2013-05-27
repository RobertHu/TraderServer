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
namespace Trader.Server
{
    public class AgentController
    {
        public static readonly AgentController Default = new AgentController();
        private ILog _Logger = LogManager.GetLogger(typeof(AgentController));
        private ConcurrentDictionary<long,ClientRelation> _Container=new ConcurrentDictionary<long,ClientRelation>();
        private AutoResetEvent _DisconnectEvent = new AutoResetEvent(false);
        private AutoResetEvent _SendQuotationEvent = new AutoResetEvent(false);
        private AutoResetEvent _SendCommandEvent = new AutoResetEvent(false);
        private ConcurrentQueue<long> _DisconnectQueue = new ConcurrentQueue<long>();
        private ConcurrentQueue<QuotationCommand> _Quotations = new ConcurrentQueue<QuotationCommand>();
        private ConcurrentQueue<Command> _Commands = new ConcurrentQueue<Command>();
        private volatile bool _Started = false;
        private volatile bool _Stopped = false;
        private QuotationCommand _CurrentQuotation;
        private Command _CurrentCommand;
        private TraderState _UpdateCommandState = new TraderState(string.Empty);
        private AgentController() 
        { 
        }

        public void Add(long session, ReceiveAgent receiver, Client sender)
        {
            this._Container.TryAdd(session, new ClientRelation(sender, receiver));
        }


        public void Remove(long session)
        {
            RemoveHelper(session);
        }
        private void RemoveHelper(long session)
        {
            ClientRelation relation;
            if (this._Container.TryRemove(session,out relation))
            {
                //relation.Sender.Closed -= this.SenderClosedEventHandle;
            }
        }

        public bool RecoverConnection(long originSession, long currentSession)
        {
            RemoveHelper(originSession);
            ClientRelation currentRelation;
            if (this._Container.TryRemove(currentSession, out currentRelation))
            {
                this._Container.TryAdd(originSession, currentRelation);
                currentRelation.Sender.UpdateSession(originSession);
                return true;
            }
            else
            {
                return false;
            }
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

                Thread quotationThread = new Thread(this.SendQuotation);
                quotationThread.IsBackground = true;
                quotationThread.Start();

                Thread commandThread = new Thread(this.SendCommand);
                commandThread.IsBackground = true;
                commandThread.Start();

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
                while (this._DisconnectQueue.Count!=0)
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

        public void AddQuotation(QuotationCommand quotation)
        {
            if (this._Container.Count == 0)
            {
                return;
            }
            this._Quotations.Enqueue(quotation);
            this._SendQuotationEvent.Set();
        }

        private void SendQuotation()
        {
            while (true)
            {
                if (this._Stopped)
                {
                    break;
                }
                this._SendQuotationEvent.WaitOne();
                while (this._Quotations.TryDequeue(out this._CurrentQuotation))
                {
                    Quotation.Default.Clear();
                    Quotation4Bit.Clear();
                    Parallel.ForEach(this._Container, SendQuotationHandler);
                }
            }
        }

    


        public void SenderClosedEventHandle(object sender,Trader.Helper.Common.SenderClosedEventArgs e)
        {
            this.EnqueueDisconnectSession(e.Session);
        }


        public void AddGeneralCommand(Command command)
        {
            if (this._Container.Count == 0)
            {
                return;
            }
            this._Commands.Enqueue(command);
            this._SendCommandEvent.Set();
        }
        private void SendCommand()
        {
            while (true)
            {
                if (this._Stopped)
                {
                    break;
                }
                this._SendCommandEvent.WaitOne();
                while (this._Commands.TryDequeue(out this._CurrentCommand))
                {
                    if (this._CurrentCommand is UpdateCommand)
                    {
                        byte[] data = Quotation.Default.ToBytesForGeneral(null, _UpdateCommandState, this._CurrentCommand);
                        if (data != null)
                        {
                            byte[] packet = SerializeManager.Default.SerializeCommand(data);
                            foreach (var item in this._Container)
                            {
                                item.Value.Sender.Send(packet);
                            }
                        }
                        continue;
                    }
                    Parallel.ForEach(this._Container, this.SendCommandHandler);
                }
            }
        }

        private void SendCommandHandler(KeyValuePair<long, ClientRelation> p)
        {
            Token token;
            TraderState state = SessionManager.Default.GetTokenAndState(p.Key,out token);
            if (token == null || state == null)
            {
                return;
            }
            byte[] content = Quotation.Default.ToBytesForGeneral(token, state, this._CurrentCommand);
            if (content == null)
            {
                return;
            }
            byte[] packet = SerializeManager.Default.SerializeCommand(content);
            p.Value.Sender.Send(packet);
        }



        private void SendQuotationHandler(KeyValuePair<long,ClientRelation> p)
        {
            Token token;
            TraderState state = SessionManager.Default.GetTokenAndState(p.Key,out token);
            if (token == null || state == null)
            {
                return;
            }
            byte[] quotation =Quotation.Default.ToBytesForQuotation(token, state,this._CurrentQuotation);
            if (quotation == null)
            {
                return;
            }
            byte[] packet = SerializeManager.Default.SerializePrice(quotation);
          
            p.Value.Sender.Send(packet);
        }

    }

    public class ClientRelation
    {
        public ClientRelation(Client sender, ReceiveAgent receiver)
        {
            this.Receiver=receiver;
            this.Sender=sender;
        }
        public ReceiveAgent Receiver { get; private set; }
        public Client Sender { get; private set; }
    }
}
