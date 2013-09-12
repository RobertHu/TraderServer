using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using log4net;
using Trader.Server.Bll;
using Trader.Server.Service;
using System.Threading;
using System.Collections.Concurrent;
using Trader.Common;

namespace Trader.Server.SessionNamespace
{
    public class SessionManager
    {
        private SessionManager() { }
        public static readonly SessionManager Default = new SessionManager();
        private ILog _Logger = LogManager.GetLogger(typeof(SessionManager));
        private ConcurrentDictionary<Session, Token> _TokenDict = new ConcurrentDictionary<Session, Token>();
        private ConcurrentDictionary<Guid, Session> _UseridToSessionDict = new ConcurrentDictionary<Guid, Session>();
        private ConcurrentDictionary<Session, string> _VersionDict = new ConcurrentDictionary<Session, string>();
        private ConcurrentDictionary<Session, TraderState> _TradingConsoleStateDict = new ConcurrentDictionary<Session, TraderState>();
        private ConcurrentDictionary<Session, int> _NextSequenceDict = new ConcurrentDictionary<Session, int>();

        public void RemoveAllItem(Session session)
        {
            this.RemoveNextSequence(session);
            this.RemoveUserIdBySession(session);
            this.RemoveToken(session);
            this.RemoveTradingConsoleState(session);
            this.RemoveVersion(session);
        }

        public int GetNextSequence(Session session)
        {
            return GetCommon(session, this._NextSequenceDict);
        }


        public void AddNextSequence(Session session, int nextSequence)
        {
            AddCommon(session, nextSequence, this._NextSequenceDict);
        }


        public void RemoveNextSequence(Session session)
        {
            RemoveCommon(session, this._NextSequenceDict);
        }
        
        public TraderState GetTradingConsoleState(Session session)
        {
            return GetCommon(session,this._TradingConsoleStateDict);
        }

        public void AddTradingConsoleState(Session session, TraderState state)
        {
            AddCommon(session, state, this._TradingConsoleStateDict);

        }

        public void RemoveTradingConsoleState(Session session)
        {
            RemoveCommon(session, this._TradingConsoleStateDict);
        }

     

        public string GetVersion(Session session)
        {
            return GetCommon(session, this._VersionDict);
        }

        public void AddVersion(Session session, string version)
        {
            AddCommon(session, version, this._VersionDict);
        }

        public void RemoveVersion(Session session)
        {
            RemoveCommon(session, this._VersionDict);
        }

        public Token GetToken(Session session)
        {
            return GetCommon(session, this._TokenDict);
        }

        public void AddToken(Session session,Token token)
        {
            AddCommon(session, token, this._TokenDict);
        }

        public void RemoveToken(Session session)
        {
            RemoveCommon(session, this._TokenDict);
        }

        public Session GetSession(Guid userID)
        {
            return GetCommon(userID, this._UseridToSessionDict);
        }

        private void RemoveSession(Guid userID)
        {
            RemoveCommon(userID, this._UseridToSessionDict);
        }

        public void RemoveUserIdBySession(Session session)
        {
            var result = this._UseridToSessionDict.Where(p =>p.Value==session).FirstOrDefault();
            this.RemoveSession(result.Key);

        }
        

        public void AddSession(Guid userID, Session session)
        {
            AddCommon(userID, session,this._UseridToSessionDict);
        }

        public T2 GetCommon<T1, T2>(T1 session, ConcurrentDictionary<T1, T2> dict)
        {
            T2 result;
            if (dict.TryGetValue(session, out result))
            {
                return result;
            }
            return default(T2);

        }


        public void AddCommon<T1, T2>(T1 key, T2 value, ConcurrentDictionary<T1, T2> dict)
        {
            dict.AddOrUpdate(key, value, (k, v) => v);
        }


        public void RemoveCommon<T1, T2>(T1 key, ConcurrentDictionary<T1, T2> dict)
        {
            T2 result;
            dict.TryRemove(key, out result);
        }


        public TraderState GetTokenAndState(Session session,out Token token)
        {
            token = null;
            this._TokenDict.TryGetValue(session, out token);
            TraderState state = null;
            this._TradingConsoleStateDict.TryGetValue(session, out state);
            return state;
        }
    }
}
