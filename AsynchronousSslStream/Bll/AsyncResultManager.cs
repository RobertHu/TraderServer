using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections.Generic;
using iExchange.Common;


public class AsyncResultManager
{
    private Dictionary<Guid, object> _Results = new Dictionary<Guid, object>();
    private Dictionary<Guid, TemporalAsyncResult> _TemporalAsyncResults = new Dictionary<Guid, TemporalAsyncResult>();
    private TimeSpan _LiveTime;

    private object _Lock = new object();    
    
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
            List<TemporalAsyncResult> willRemovedTemporalAsyncResults = new List<TemporalAsyncResult>();
            DateTime now = DateTime.UtcNow;
            foreach(TemporalAsyncResult item in this._TemporalAsyncResults.Values)
            {
                if (now - item.TimeStamp > this._LiveTime)
                {
                    willRemovedTemporalAsyncResults.Add(item);
                }
            }
            foreach (TemporalAsyncResult item in willRemovedTemporalAsyncResults)
            {
                //AppDebug.LogEvent("AsyncResultManager.SetResult", string.Format("Delete timeout async result {0}, {1}", item.AsyncResult.Id, item.AsyncResult.MethodName), System.Diagnostics.EventLogEntryType.Information);

                this._Results.Remove(item.AsyncResult.Id);
                this._TemporalAsyncResults.Remove(item.AsyncResult.Id);
            }

            //AppDebug.LogEvent("AsyncResultManager.SetResult", string.Format("Set result of {0}, {1}", asyncResult.Id, asyncResult.MethodName), System.Diagnostics.EventLogEntryType.Information);
            this._Results.Add(asyncResult.Id, result);
            this._TemporalAsyncResults.Add(asyncResult.Id, new TemporalAsyncResult(asyncResult));
        }
    }

    public object GetResult(Guid asyncResultId)
    {
        lock (this._Lock)
        {
            //AppDebug.LogEvent("AsyncResultManager.GetResult", string.Format("asyncResultId = {0}", asyncResultId), System.Diagnostics.EventLogEntryType.Information);

            object result = null;
            if (this._Results.TryGetValue(asyncResultId, out result))
            {
                this._Results.Remove(asyncResultId);
                this._TemporalAsyncResults.Remove(asyncResultId);
            }
            else
            {
                AppDebug.LogEvent("AsyncResultManager.GetResult", string.Format("Result of [{0}] is still unavaiable", asyncResultId), System.Diagnostics.EventLogEntryType.Warning);
            }
            return result;
        }
    }
}

class TemporalAsyncResult
{
    private AsyncResult _AsyncResult;
    private DateTime _TimeStamp;

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