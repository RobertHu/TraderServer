using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iExchange.Common;

namespace Trader.Server.Report
{
    public class AccountSummaryArgument : HttpContextHolder
    {
        private string _TradeDay;
        private string _AccountIds;
        private string _Rdlc;
        private AsyncResult _AsyncResult;

        public AccountSummaryArgument(string tradeDay, string accountIds, string rdlc, AsyncResult asyncResult, Guid session)
            : base(session)
        {
            this._TradeDay = tradeDay;
            this._AccountIds = accountIds;
            this._Rdlc = rdlc;

            this._AsyncResult = asyncResult;
        }

        public string TradeDay
        {
            get { return this._TradeDay; }
        }

        public string AccountIds
        {
            get { return this._AccountIds; }
        }

        public string Rdlc
        {
            get { return this._Rdlc; }
        }

        public AsyncResult AsyncResult
        {
            get { return this._AsyncResult; }
        }
    }
}