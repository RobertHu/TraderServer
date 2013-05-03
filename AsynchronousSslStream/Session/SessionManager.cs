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

namespace Trader.Server.Session
{
    public class SessionManager
    {
        private SessionManager() { }
        public static readonly SessionManager Default = new SessionManager();
        private ILog _Logger = LogManager.GetLogger(typeof(SessionManager));
        private Dictionary<string, Token> _TokenDict = new Dictionary<string, Token>();
        private Dictionary<Guid, string> _UseridToSessionDict = new Dictionary<Guid, string>();
        private Dictionary<string, string> _VersionDict = new Dictionary<string, string>();
        private Dictionary<string, TraderState> _TradingConsoleStateDict = new Dictionary<string, TraderState>();
        private Dictionary<string, int> _NextSequenceDict = new Dictionary<string, int>();

        private ReaderWriterLockSlim _ReadWriteLock = new ReaderWriterLockSlim();


        public void RemoveAllItem(string session)
        {
            this.RemoveNextSequence(session);
            this.RemoveUserIdBySession(session);
            this.RemoveToken(session);
            this.RemoveTradingConsoleState(session);
            this.RemoveVersion(session);
        }

        public int GetNextSequence(string session)
        {
            return GetCommon(session, this._NextSequenceDict);
        }


        public void AddNextSequence(string session, int nextSequence)
        {
            AddCommon(session, nextSequence, this._NextSequenceDict);
        }


        public void RemoveNextSequence(string session)
        {
            RemoveCommon(session, this._NextSequenceDict);
        }
        
        public TraderState GetTradingConsoleState(string session)
        {
            return GetCommon(session,this._TradingConsoleStateDict);
        }

        public void AddTradingConsoleState(string session, TraderState state)
        {
            AddCommon(session, state, this._TradingConsoleStateDict);

        }

        public void RemoveTradingConsoleState(string session)
        {
            RemoveCommon(session, this._TradingConsoleStateDict);
        }

     

        public string GetVersion(string session)
        {
            return GetCommon(session, this._VersionDict);
        }

        public void AddVersion(string session, string version)
        {
            AddCommon(session, version, this._VersionDict);
        }

        public void RemoveVersion(string session)
        {
            RemoveCommon(session, this._VersionDict);
        }

        public Token GetToken(string session)
        {
            return GetCommon(session, this._TokenDict);
        }

        public void AddToken(string session,Token token)
        {
            AddCommon(session, token, this._TokenDict);
        }

        public void RemoveToken(string session)
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

        public void RemoveUserIdBySession(string session)
        {
            var result = this._UseridToSessionDict.Where(p => p.Value == session).FirstOrDefault();
            this.RemoveSession(result.Key);

        }
        

        public void AddSession(Guid userID, string session)
        {
            AddCommon(userID, session,this._UseridToSessionDict);
        }

        public T2 GetCommon<T1, T2>(T1 session, Dictionary<T1, T2> dict)
        {
            try
            {
                this._ReadWriteLock.EnterReadLock();
                if (dict.ContainsKey(session))
                {
                    return dict[session];
                }
                return default(T2);
            }
            finally
            {
                this._ReadWriteLock.ExitReadLock();
            }
            
        }


        public void AddCommon<T1, T2>(T1 key, T2 value, Dictionary<T1, T2> dict)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                }
                else
                {
                    dict.Add(key, value);
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
            
        }


        public void RemoveCommon<T1, T2>(T1 key, Dictionary<T1, T2> dict)
        {
            try
            {
                this._ReadWriteLock.EnterWriteLock();
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
            }
            finally
            {
                this._ReadWriteLock.ExitWriteLock();
            }
            
        }


        public Tuple<Token, TraderState> GetTokenAndState(string session)
        {
            var token = this.GetToken(session);
            var state = this.GetTradingConsoleState(session);
            return Tuple.Create(token, state);
        }
    }
}
