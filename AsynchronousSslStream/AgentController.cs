using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Trader.Server.Bll;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Trader.Server.SessionNamespace;
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
        private readonly ILog _Logger = LogManager.GetLogger(typeof(AgentController));
        private readonly ConcurrentDictionary<Session, ClientRelation> _Container = new ConcurrentDictionary<Session, ClientRelation>();
        private readonly AutoResetEvent _DisconnectEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _SendQuotationEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent[] _DisconnectStopEvents;
        private readonly AutoResetEvent[] _QuotationStopEvents;
        private readonly ConcurrentQueue<Session> _DisconnectQueue = new ConcurrentQueue<Session>();
        private readonly ConcurrentQueue<CommandForClient> _Commands = new ConcurrentQueue<CommandForClient>();
        private volatile bool _Started;
        private volatile bool _Stopped;
        private CommandForClient _Current;
        private AgentController()
        {
            _DisconnectStopEvents= new[]{_DisconnectEvent,_StopEvent};
            _QuotationStopEvents = new[] {_SendQuotationEvent,_StopEvent };
        }

        public void Add(Session session, ReceiveAgent receiver, Client sender)
        {
            _Container.TryAdd(session, new ClientRelation(sender, receiver));
        }

        public void Remove(Session session)
        {
            ClientRelation relation;
            _Container.TryRemove(session, out relation);
        }

        public bool RecoverConnection(Session originSession, Session currentSession)
        {
            ClientRelation relation;
            _Container.TryRemove(originSession, out relation);
            ClientRelation currentRelation;
            if (!_Container.TryRemove(currentSession, out currentRelation)) return false;
            _Container.TryAdd(originSession, currentRelation);
            currentRelation.Sender.UpdateClientID(originSession);
            return true;
        }

        public Client GetSender(Session session)
        {
            Client result = null;
            ClientRelation relation;
            if (_Container.TryGetValue(session, out relation))
            {
                result = relation.Sender;
            }
            return result;
        }


        public ReceiveAgent GetReceiver(Session session)
        {
            ReceiveAgent result = null;
            ClientRelation relation;
            if (_Container.TryGetValue(session, out relation))
            {
                result = relation.Receiver;
            }
            return result;
        }

        public void Start()
        {
            try
            {
                if (_Started) return;
                var thread = new Thread(DisconnectHandle) {IsBackground = true};
                thread.Start();

                var quotationThread = new Thread(this.Dispatch) {IsBackground = true};
                quotationThread.Start();
                _Started = true;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }

        public void Stop()
        {
            _Stopped = true;
            _StopEvent.Set();
        }

        public void EnqueueDisconnectSession(Session session)
        {
            _DisconnectQueue.Enqueue(session);
            _DisconnectEvent.Set();
        }


        private void DisconnectHandle()
        {
            while (true)
            {
                if (_Stopped) break;
                WaitHandle.WaitAny(_DisconnectStopEvents);
                while (_DisconnectQueue.Count != 0)
                {
                    if (_Stopped) break;
                    Session session;
                    if (_DisconnectQueue.TryDequeue(out session))
                    {
                        Remove(session);
                    }
                }
            }
        }

        public void SendCommand(QuotationCommand quotationCommand=null,Command command =null)
        {
            if (_Container.Count == 0) return;
            var target = new CommandForClient(quotationCommand:quotationCommand,command:command);
            _Commands.Enqueue(target);
            _SendQuotationEvent.Set();
        }

        private void Dispatch()
        {
            while (true)
            {
                if (_Stopped) break;
                WaitHandle.WaitAny(_QuotationStopEvents);
                while (_Commands.TryDequeue(out _Current))
                {
                    Parallel.ForEach(source: _Container, body: SendCommandHandler);
                }
            }
        }

        private void SendCommandHandler(KeyValuePair<Session, ClientRelation> p)
        {
            p.Value.Sender.Send(_Current);
        }

        public void KickoutAllClient()
        {
            Parallel.ForEach(_Container, p =>
            {
                p.Value.Sender.Send(new CommandForClient(data: NamedCommands.GetKickoutPacket()));
                Application.Default.SessionMonitor.Remove(p.Key);
            });
        }
    }

}