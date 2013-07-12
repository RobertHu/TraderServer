using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Trader.Server.Bll;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Trader.Server.Session;
using iExchange.Common;
using Trader.Common;
using Trader.Server.Ssl;
using Trader.Server._4BitCompress;
using Trader.Server.Service;
using Trader.Server.ValueObjects;
namespace Trader.Server
{
   
    public class AgentController
    {
        public static readonly AgentController Default = new AgentController();
        private ILog _Logger = LogManager.GetLogger(typeof(AgentController));
        private ConcurrentDictionary<long, ClientRelation> _Container = new ConcurrentDictionary<long, ClientRelation>();
        private AutoResetEvent _DisconnectEvent = new AutoResetEvent(false);
        private AutoResetEvent _SendQuotationEvent = new AutoResetEvent(false);
        private AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private AutoResetEvent[] _DisconnectStopEvents;
        private AutoResetEvent[] _QuotationStopEvents;
        private ConcurrentQueue<long> _DisconnectQueue = new ConcurrentQueue<long>();
        private ConcurrentQueue<CommandForClient> _Commands = new ConcurrentQueue<CommandForClient>();
        private volatile bool _Started = false;
        private volatile bool _Stopped = false;
        private CommandForClient _Current;
        private AgentController()
        {
            this._DisconnectStopEvents= new AutoResetEvent[]{this._DisconnectEvent,this._StopEvent};
            this._QuotationStopEvents = new AutoResetEvent[] {this._SendQuotationEvent,this._StopEvent };
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
                currentRelation.Sender.UpdateClientID(originSession);
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
            this._StopEvent.Set();
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
                WaitHandle.WaitAny(this._DisconnectStopEvents);
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
                WaitHandle.WaitAny(this._QuotationStopEvents);
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

        public void KickoutAllClient()
        {
            Parallel.ForEach(this._Container, p =>
            {
                p.Value.Sender.Send(new ValueObjects.CommandForClient(data: NamedCommands.GetKickoutPacket()));
                Application.Default.SessionMonitor.Remove(p.Key);
            });
        }
    }

}