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

namespace Trader.Server.Session
{
    public class SessionManager
    {
        private SessionManager() { }
        public static readonly SessionManager Default = new SessionManager();
        private ILog _Logger = LogManager.GetLogger(typeof(SessionManager));
        private ConcurrentDictionary<Guid, Token> _TokenDict = new ConcurrentDictionary<Guid, Token>();
        private ConcurrentDictionary<Guid, string> _UseridToSessionDict = new ConcurrentDictionary<Guid, string>();
        private ConcurrentDictionary<Guid, string> _VersionDict = new ConcurrentDictionary<Guid, string>();
        private ConcurrentDictionary<Guid, TraderState> _TradingConsoleStateDict = new ConcurrentDictionary<Guid, TraderState>();
        private ConcurrentDictionary<Guid, int> _NextSequenceDict = new ConcurrentDictionary<Guid, int>();

        public void RemoveAllItem(Guid session)
        {
            this.RemoveNextSequence(session);
            this.RemoveUserIdBySession(session);
            this.RemoveToken(session);
            this.RemoveTradingConsoleState(session);
            this.RemoveVersion(session);
        }

        public int GetNextSequence(Guid session)
        {
            return GetCommon(session, this._NextSequenceDict);
        }


        public void AddNextSequence(Guid session, int nextSequence)
        {
            AddCommon(session, nextSequence, this._NextSequenceDict);
        }


        public void RemoveNextSequence(Guid session)
        {
            RemoveCommon(session, this._NextSequenceDict);
        }
        
        public TraderState GetTradingConsoleState(Guid session)
        {
            return GetCommon(session,this._TradingConsoleStateDict);
        }

        public void AddTradingConsoleState(Guid session, TraderState state)
        {
            AddCommon(session, state, this._TradingConsoleStateDict);

        }

        public void RemoveTradingConsoleState(Guid session)
        {
            RemoveCommon(session, this._TradingConsoleStateDict);
        }

     

        public string GetVersion(Guid session)
        {
            return GetCommon(session, this._VersionDict);
        }

        public void AddVersion(Guid session, string version)
        {
            AddCommon(session, version, this._VersionDict);
        }

        public void RemoveVersion(Guid session)
        {
            RemoveCommon(session, this._VersionDict);
        }

        public Token GetToken(Guid session)
        {
            return GetCommon(session, this._TokenDict);
        }

        public void AddToken(Guid session,Token token)
        {
            AddCommon(session, token, this._TokenDict);
        }

        public void RemoveToken(Guid session)
        {
            RemoveCommon(session, this._TokenDict);
        }

        public string GetSession(Guid userID)
        {
            return GetCommon(userID, this._UseridToSessionDict);
        }

        private void RemoveSession(Guid userID)
        {
            RemoveCommon(userID, this._UseridToSessionDict);
        }

        public void RemoveUserIdBySession(Guid session)
        {
            var result = this._UseridToSessionDict.Where(p =>p.Value==(session.ToString())).FirstOrDefault();
            this.RemoveSession(result.Key);

        }
        

        public void AddSession(Guid userID, Guid session)
        {
            AddCommon(userID, session.ToString(),this._UseridToSessionDict);
        }

        public T2 GetCommon<T1, T2>(T1 session, ConcurrentDictionary<T1, T2> dict)
        {
            try
            {
                return dict[session];
            }
            catch (Exception ex)
            {
                return default(T2);

            }
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


        public Tuple<Token, TraderState> GetTokenAndState(Guid session)
        {
            var token = this.GetToken(session);
            var state = this.GetTradingConsoleState(session);
            return Tuple.Create(token, state);
        }
    }
}
