using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iExchange.Common;
using Trader.Common;

namespace Trader.Server.Report
{
    public class AccountSummaryArgument : HttpContextHolder
    {
        private string _FromDay;
        private string _ToDay;
        private string _AccountIds;
        private string _Rdlc;
        private AsyncResult _AsyncResult;

        public AccountSummaryArgument(string fromDay, string toDay, string accountIds, string rdlc, AsyncResult asyncResult, Session session)
            : base(session)
        {
            this._FromDay = fromDay;
            this._ToDay = toDay;
            this._AccountIds = accountIds;
            this._Rdlc = rdlc;

            this._AsyncResult = asyncResult;
        }

        public string FromDay
        {
            get { return this._FromDay; }
        }

        public string ToDay
        {
            get { return this._FromDay; }
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