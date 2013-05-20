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
        private AgentController() { }

        public void Add(long session, Trader.Helper.Common.IReceiveAgent receiver, Trader.Helper.Common.ICommunicationAgent sender)
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

        public Trader.Helper.Common.ICommunicationAgent GetSender(long session)
        {
            Trader.Helper.Common.ICommunicationAgent result = null;
            ClientRelation relation;
            if (this._Container.TryGetValue(session, out relation))
            {
                result = relation.Sender;
            }
            return result;
        }


        public Trader.Helper.Common.IReceiveAgent GetReceiver(long session)
        {
            Trader.Helper.Common.IReceiveAgent result = null;
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
                    Quotation item;
                    if (this._Quotations.TryDequeue(out item))
                    {
                        if (this._Container.Count == 0)
                        {
                            continue;
                        }
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


        private void SendCommand(Quotation command, long session, Trader.Helper.Common.ICommunicationAgent sendAgent)
        {
            if (command == null) return;
            Token token;
            TraderState state = SessionManager.Default.GetTokenAndState(session,out token);
            if (token == null || state == null)
            {
                return;
            }
            bool isQuotation;
            byte[] quotation;
            quotation= command.ToBytes(token, state,out isQuotation);
            if (quotation == null)
            {
                return;
            }
            SerializedObject job = new SerializedObject();
            if (token.AppType == AppType.TradingConsole && isQuotation)
            {
                job.IsPrice = true;
                job.Price = quotation;
            }
            else
            {
                job.ContentInByte = quotation;
                job.Session = session;
            }
            byte[] packet = SerializeManager.Default.Serialize(job);
            sendAgent.Send(packet);
        }

    }

    public class ClientRelation
    {
        public ClientRelation( Trader.Helper.Common.ICommunicationAgent sender, Trader.Helper.Common.IReceiveAgent receiver)
        {
            this.Receiver=receiver;
            this.Sender=sender;
        }
        public Trader.Helper.Common.IReceiveAgent Receiver { get; private set; }
        public Trader.Helper.Common.ICommunicationAgent Sender { get; private set; }
    }
}
