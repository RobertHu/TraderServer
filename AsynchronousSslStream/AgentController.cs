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
using ReceiveAgent = Trader.Helper.Common.IReceiveAgent;
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
        private ConcurrentQueue<long> _DisconnectQueue = new ConcurrentQueue<long>();
        private ConcurrentQueue<Quotation> _Quotations = new ConcurrentQueue<Quotation>();
        private volatile bool _IsDisconnectHandlerStarted = false;
        private volatile bool _IsDisconnectHandlerStopped = false;
        private Quotation _CurrentQuotation;
        private TraderState _UpdateCommandState = new TraderState(string.Empty);
        private AgentController() 
        { 
        }

        public void Add(long session, ReceiveAgent receiver, CommunicationAgent sender)
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
                relation.Sender.Closed -= this.SenderClosedEventHandle;
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

        public CommunicationAgent GetSender(long session)
        {
            CommunicationAgent result = null;
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
                if (this._IsDisconnectHandlerStarted)
                {
                    return;
                }
                Thread thread = new Thread(this.DisconnectHandle);
                thread.IsBackground = true;
                thread.Start();
                Thread quotationThread = new Thread(this.SendQuotation);
                quotationThread.IsBackground = true;
                quotationThread.Start();
                this._IsDisconnectHandlerStarted = true;
            }
            catch (Exception ex)
            {
                this._Logger.Error(ex);
            }
        }

        public void Stop()
        {
            this._IsDisconnectHandlerStopped = true;
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
                if (this._IsDisconnectHandlerStopped)
                {
                    break;
                }
                this._DisconnectEvent.WaitOne();
                while (this._DisconnectQueue.Count!=0)
                {
                    if (this._IsDisconnectHandlerStopped)
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

        public void AddQuotation(Quotation quotation)
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
                if (this._IsDisconnectHandlerStopped)
                {
                    break;
                }
                this._SendQuotationEvent.WaitOne();
                while (this._Quotations.Count!=0)
                {
                    if (this._IsDisconnectHandlerStopped)
                    {
                        break;
                    }
                    if (this._Quotations.TryDequeue(out this._CurrentQuotation))
                    {
                        if (this._Container.Count == 0)
                        {
                            continue;
                        }
                        if (this._CurrentQuotation == null)
                        {
                            continue;
                        }
                        if (this._CurrentQuotation.Command is QuotationCommand)
                        {
                            Quotation4Bit.Clear();
                        }
                        else if (this._CurrentQuotation.Command is UpdateCommand)
                        {
                            bool isQuotation;
                            
                            byte[] data = this._CurrentQuotation.ToBytes(null, _UpdateCommandState, out isQuotation);
                            if (data != null)
                            {
                                byte[] packet = SerializeManager.Default.SerializeCommand(data);
                                foreach (var item in this._Container.Values)
                                {
                                    item.Sender.Send(packet);
                                }
                            }
                            continue;
                        }

                        Parallel.ForEach(this._Container, SendCommand);
                    }
                }
            }
        }


        public void SenderClosedEventHandle(object sender,Trader.Helper.Common.SenderClosedEventArgs e)
        {
            this.EnqueueDisconnectSession(e.Session);
        }


        private void SendCommand(KeyValuePair<long,ClientRelation> p)
        {
            Token token;
            TraderState state = SessionManager.Default.GetTokenAndState(p.Key,out token);
            if (token == null || state == null)
            {
                return;
            }
            bool isQuotation;
            byte[] quotation = this._CurrentQuotation.ToBytes(token, state,out isQuotation);
            if (quotation == null)
            {
                return;
            }
            byte[] packet;
            if (token.AppType == AppType.TradingConsole && isQuotation)
            {
                packet = SerializeManager.Default.SerializePrice(quotation);
            }
            else
            {
                packet = SerializeManager.Default.SerializeCommand(quotation);
            }
            p.Value.Sender.Send(packet);
        }

    }

    public class ClientRelation
    {
        public ClientRelation( CommunicationAgent sender,ReceiveAgent  receiver)
        {
            this.Receiver=receiver;
            this.Sender=sender;
        }
        public ReceiveAgent Receiver { get; private set; }
        public CommunicationAgent Sender { get; private set; }
    }
}
