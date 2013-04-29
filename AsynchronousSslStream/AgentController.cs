using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Trader.Helper;
namespace AsyncSslServer
{
    public class AgentController
    {
        public static readonly AgentController Default = new AgentController();
        private ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();
        private ILog _Logger = LogManager.GetLogger(typeof(AgentController));
        private long _ClientCount = 0;
        private Dictionary<Guid,ClientRelation> _Container=new Dictionary<Guid,ClientRelation>();
        private AutoResetEvent _DisconnectEvent = new AutoResetEvent(false);
        private object _DisconnectLock = new object();
        private Queue<Guid> _DisconnectQueue = new Queue<Guid>(1024);
        private volatile bool _IsDisconnectHandlerStarted = false;
        private volatile bool _IsDisconnectHandlerStopped = false;
        private AgentController() { }

        public void Add(Guid session, Common.IReceiveAgent receiver, Common.ICommunicationAgent sender)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (!this._Container.ContainsKey(session))
                {
                    this._Container.Add(session, new ClientRelation(sender, receiver));
                    QuotationAgent.Quotation.Default.Add(session, sender);
                    Console.WriteLine("clientCount={0}", ++this._ClientCount);
                }
                
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public void Remove(Guid session)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (!this._Container.ContainsKey(session))
                {
                    return;
                }
                RemoveHelper(session);

            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }
        private void RemoveHelper(Guid session)
        {
            var relation = this._Container[session];
            relation.Sender.DataArrived -= ReceiveCenter.Default.DataArrivedHandler;
            relation.Sender.Closed -= this.SenderClosedEventHandle;
            relation.Receiver.ResponseSent -= SendCenter.Default.ResponseSentHandle;
            this._Container.Remove(session);
            QuotationAgent.Quotation.Default.Remove(session);
            Console.WriteLine("clientCount={0}", --this._ClientCount);
        }

        public bool RecoverConnection(Guid originSession, Guid currentSession)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (this._Container.ContainsKey(originSession))
                {
                    RemoveHelper(originSession);
                }
                if (this._Container.ContainsKey(currentSession))
                {
                    var currentRelation = this._Container[currentSession];
                    this._Container.Remove(currentSession);
                    QuotationAgent.Quotation.Default.Remove(currentSession);
                    this._Container.Add(originSession, currentRelation);
                    currentRelation.Sender.UpdateSession(originSession);
                    QuotationAgent.Quotation.Default.Add(originSession, currentRelation.Sender);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
        }

        public Common.ICommunicationAgent GetSender(Guid session)
        {
            try
            {
                this._ReadWriteLock.EnterReadLock();
                if (this._Container.ContainsKey(session))
                {
                    return this._Container[session].Sender;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                this._ReadWriteLock.ExitReadLock();
            }
        }

        public Common.IReceiveAgent GetReceiver(Guid session)
        {
            try
            {
                this._ReadWriteLock.EnterReadLock();
                if (this._Container.ContainsKey(session))
                {
                    return this._Container[session].Receiver;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                this._ReadWriteLock.ExitReadLock();
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
            lock (this._DisconnectLock)
            {
                this._DisconnectQueue.Enqueue(session);
                this._DisconnectEvent.Set();
            }
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
                while (true)
                {
                    if (this._IsDisconnectHandlerStopped)
                    {
                        break;
                    }
                    Guid session = Guid.Empty;
                    lock (this._DisconnectLock)
                    {
                        if (this._DisconnectQueue.Count != 0)
                        {
                            session = this._DisconnectQueue.Dequeue();
                        }
                    }
                    if (session == Guid.Empty)
                    {
                        break;
                    }
                    this.Remove(session);
                }
            }
        }


        public void SenderClosedEventHandle(object sender,Common.SenderClosedEventArgs e)
        {
            this.EnqueueDisconnectSession(e.Session);
        }
        

    }

    public class ClientRelation
    {
        public ClientRelation(Common.ICommunicationAgent sender, Common.IReceiveAgent receiver)
        {
            this.Receiver=receiver;
            this.Sender=sender;
        }
        public Common.IReceiveAgent Receiver { get; private set; }
        public Common.ICommunicationAgent Sender { get; private set; }
    }
}
