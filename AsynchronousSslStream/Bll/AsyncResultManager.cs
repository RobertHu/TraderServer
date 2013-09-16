using System;
using System.Collections.Generic;
using System.Linq;
using iExchange.Common;
using log4net;

namespace Trader.Server.Bll
{
    public class AsyncResultManager
    {
        private readonly Dictionary<Guid, object> _Results = new Dictionary<Guid, object>();
        private readonly Dictionary<Guid, TemporalAsyncResult> _TemporalAsyncResults = new Dictionary<Guid, TemporalAsyncResult>();
        private readonly TimeSpan _LiveTime;
        private readonly object _Lock = new object();
        private readonly ILog _Logger = LogManager.GetLogger(typeof(AsyncResultManager));
        public AsyncResultManager(TimeSpan liveTime)
        {
            this._LiveTime = liveTime;
        }

        public object GetResult(AsyncResult asyncResult)
        {
            return this.GetResult(asyncResult.Id);
        }

        public void SetResult(AsyncResult asyncResult, object result)
        {
            lock (this._Lock)
            {
                DateTime now = DateTime.UtcNow;
                var willRemovedTemporalAsyncResults = this._TemporalAsyncResults.Values.Where(item => now - item.TimeStamp > this._LiveTime).ToList();
                foreach (TemporalAsyncResult item in willRemovedTemporalAsyncResults)
                {
                    this._Results.Remove(item.AsyncResult.Id);
                    this._TemporalAsyncResults.Remove(item.AsyncResult.Id);
                }
                this._Results.Add(asyncResult.Id, result);
                this._TemporalAsyncResults.Add(asyncResult.Id, new TemporalAsyncResult(asyncResult));
            }
        }

        public object GetResult(Guid asyncResultId)
        {
            lock (this._Lock)
            {
                object result = null;
                if (this._Results.TryGetValue(asyncResultId, out result))
                {
                    this._Results.Remove(asyncResultId);
                    this._TemporalAsyncResults.Remove(asyncResultId);
                }
                else
                {
                    _Logger.Warn(string.Format("Result of [{0}] is still unavaiable", asyncResultId));
                }
                return result;
            }
        }
    }
}