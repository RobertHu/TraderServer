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
namespace Trader.Server
{
    public class AgentController
    {
        public static readonly AgentController Default = new AgentController();
        private ILog _Logger = LogManager.GetLogger(typeof(AgentController));
        private ConcurrentDictionary<Guid,ClientRelation> _Container=new ConcurrentDictionary<Guid,ClientRelation>();
        private AutoResetEvent _DisconnectEvent = new AutoResetEvent(false);
        private AutoResetEvent _SendQuotationEvent = new AutoResetEvent(false);
        private ConcurrentQueue<Guid> _DisconnectQueue = new ConcurrentQueue<Guid>();
        private ConcurrentQueue<Quotation> _Quotations = new ConcurrentQueue<Quotation>();
        private volatile bool _IsDisconnectHandlerStarted = false;
        private volatile bool _IsDisconnectHandlerStopped = false;
        private AgentController() { }

        public void Add(Guid session, Trader.Helper.Common.IReceiveAgent receiver, Trader.Helper.Common.ICommunicationAgent sender)
        {
            this._Container.TryAdd(session, new ClientRelation(sender, receiver));
        }

        public void AddForLogined(Guid session)
        {
            ClientRelation relation;
            if (this._Container.TryGetValue(session, out relation))
            {
               // QuotationAgent.Quotation.Default.Add(session, relation.Sender);
            }
        }

        public void Remove(Guid session)
        {
            RemoveHelper(session);
        }
        private void RemoveHelper(Guid session)
        {
            ClientRelation relation;
            if (this._Container.TryRemove(session,out relation))
            {
                relation.Sender.DataArrived -= ReceiveCenter.Default.DataArrivedHandler;
                relation.Sender.Closed -= this.SenderClosedEventHandle;
                //QuotationAgent.Quotation.Default.Remove(session);
            }
        }

        public bool RecoverConnection(Guid originSession, Guid currentSession)
        {
            RemoveHelper(originSession);
            ClientRelation currentRelation;
            if (this._Container.TryRemove(currentSession, out currentRelation))
            {
                //QuotationAgent.Quotation.Default.Remove(currentSession);
                this._Container.TryAdd(originSession, currentRelation);
                currentRelation.Sender.UpdateSession(originSession);
                //QuotationAgent.Quotation.Default.Add(originSession, currentRelation.Sender);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Trader.Helper.Common.ICommunicationAgent GetSender(Guid session)
        {
            try
            {
                ClientRelation relation = this._Container[session];
                if (relation == null)
                {
                    return null;
                }
                else
                {
                    return relation.Sender;
                }
            }
            catch
            {
                return null;
            }
        }


        public Trader.Helper.Common.IReceiveAgent GetReceiver(Guid session)
        {
            try
            {
                ClientRelation relation = this._Container[session];
                if (relation == null)
                {
                    return null;
                }
                else
                {
                    return relation.Receiver;
                }
            }
            catch
            {
                return null;
            }
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

        public void EnqueueDisconnectSession(Guid session)
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
                    Guid session;
                    if (this._DisconnectQueue.TryDequeue(out session))
                    {
                        this.Remove(session);
                    }
                }
            }
        }

        public void AddQuotation(Quotation quotation)
        {
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
                    Quotation item;
                    if (this._Quotations.TryDequeue(out item))
                    {
                        Parallel.ForEach(this._Container, p =>
                        {
                            SendCommand(item, p.Key, p.Value.Sender);
                        });
                    }
                }
            }
        }


        public void SenderClosedEventHandle(object sender,Trader.Helper.Common.SenderClosedEventArgs e)
        {
            this.EnqueueDisconnectSession(e.Session);
        }


        private void SendCommand(Quotation command, Guid session, Trader.Helper.Common.ICommunicationAgent sendAgent)
        {
            if (command == null) return;
            var tokenAndState = SessionManager.Default.GetTokenAndState(session);
            Token token = tokenAndState.Item1;
            TraderState state = tokenAndState.Item2;
            if (token == null || state == null)
            {
                return;
            }
            var result = command.ToBytes(token, state);
            bool isQuotation = result.Item1;
            byte[] quotation = result.Item2;
            if (quotation == null)
            {
                return;
            }
            JobItem job = new JobItem();
            if (token.AppType == AppType.TradingConsole && isQuotation)
            {
                job.Type = JobType.Price;
                job.Price = quotation;
            }
            else
            {
                job.Type = JobType.Transaction;
                job.ContentInByte = quotation;
                job.SessionID = session;
            }
            byte[] packet = SendManager.SerializeMsg(job);
            sendAgent.Send(packet);
        }

    }

    public class ClientRelation
    {
        public ClientRelation(Trader.Helper.Common.ICommunicationAgent sender, Trader.Helper.Common.IReceiveAgent receiver)
        {
            this.Receiver=receiver;
            this.Sender=sender;
        }
        public Trader.Helper.Common.IReceiveAgent Receiver { get; private set; }
        public Trader.Helper.Common.ICommunicationAgent Sender { get; private set; }
    }
}
