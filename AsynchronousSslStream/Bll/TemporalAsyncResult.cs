using System;
using iExchange.Common;

namespace Trader.Server.Bll
{
    class TemporalAsyncResult
    {
        private readonly AsyncResult _AsyncResult;
        private readonly DateTime _TimeStamp;

        internal TemporalAsyncResult(AsyncResult asyncResult)
        {
            this._AsyncResult = asyncResult;
            this._TimeStamp = DateTime.UtcNow;
        }

        internal AsyncResult AsyncResult
        {
            get { return this._AsyncResult; }
        }

        internal DateTime TimeStamp
        {
            get { return this._TimeStamp; }
        }
    }
}