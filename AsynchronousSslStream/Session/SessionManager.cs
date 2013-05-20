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
        private ConcurrentDictionary<long, Token> _TokenDict = new ConcurrentDictionary<long, Token>();
        private ConcurrentDictionary<Guid, long> _UseridToSessionDict = new ConcurrentDictionary<Guid, long>();
        private ConcurrentDictionary<long, string> _VersionDict = new ConcurrentDictionary<long, string>();
        private ConcurrentDictionary<long, TraderState> _TradingConsoleStateDict = new ConcurrentDictionary<long, TraderState>();
        private ConcurrentDictionary<long, int> _NextSequenceDict = new ConcurrentDictionary<long, int>();

        public void RemoveAllItem(long session)
        {
            this.RemoveNextSequence(session);
            this.RemoveUserIdBySession(session);
            this.RemoveToken(session);
            this.RemoveTradingConsoleState(session);
            this.RemoveVersion(session);
        }

        public int GetNextSequence(long session)
        {
            return GetCommon(session, this._NextSequenceDict);
        }


        public void AddNextSequence(long session, int nextSequence)
        {
            AddCommon(session, nextSequence, this._NextSequenceDict);
        }


        public void RemoveNextSequence(long session)
        {
            RemoveCommon(session, this._NextSequenceDict);
        }
        
        public TraderState GetTradingConsoleState(long session)
        {
            return GetCommon(session,this._TradingConsoleStateDict);
        }

        public void AddTradingConsoleState(long session, TraderState state)
        {
            AddCommon(session, state, this._TradingConsoleStateDict);

        }

        public void RemoveTradingConsoleState(long session)
        {
            RemoveCommon(session, this._TradingConsoleStateDict);
        }

     

        public string GetVersion(long session)
        {
            return GetCommon(session, this._VersionDict);
        }

        public void AddVersion(long session, string version)
        {
            AddCommon(session, version, this._VersionDict);
        }

        public void RemoveVersion(long session)
        {
            RemoveCommon(session, this._VersionDict);
        }

        public Token GetToken(long session)
        {
            return GetCommon(session, this._TokenDict);
        }

        public void AddToken(long session,Token token)
        {
            AddCommon(session, token, this._TokenDict);
        }

        public void RemoveToken(long session)
        {
            RemoveCommon(session, this._TokenDict);
        }

        public long GetSession(Guid userID)
        {
            return GetCommon(userID, this._UseridToSessionDict);
        }

        private void RemoveSession(Guid userID)
        {
            RemoveCommon(userID, this._UseridToSessionDict);
        }

        public void RemoveUserIdBySession(long session)
        {
            var result = this._UseridToSessionDict.Where(p =>p.Value==session).FirstOrDefault();
            this.RemoveSession(result.Key);

        }
        

        public void AddSession(Guid userID, long session)
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


        public TraderState GetTokenAndState(long session,out Token token)
        {
            try
            {
                token = this._TokenDict[session];
                var state = this._TradingConsoleStateDict[session];
                return state;
            }
            catch
            {
                token = null;
                return null;
            }
        }
    }
}
