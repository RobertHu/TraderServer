using System;
using System.Data;
using System.Configuration;
using System.Web;
using iExchange.Common;
using Trader.Common;

public class TickByTickHistoryDataArgument2 : HttpContextHolder
{
    private Guid _instrumentId;
    DateTime _from;
    DateTime _to;

    private AsyncResult _asyncResult;

    public TickByTickHistoryDataArgument2(Guid instrumentId, DateTime from, DateTime to,
        AsyncResult asyncResult, Session session)
        : base(session)
    {
        this._instrumentId = instrumentId;
        this._from = from;
        this._to = to;

        this._asyncResult = asyncResult;
    }

    public Guid InstrumentId
    {
        get { return this._instrumentId; }
    }

    public DateTime From
    {
        get { return this._from; }
    }

    public DateTime To
    {
        get { return this._to; }
    }

    public AsyncResult AsyncResult
    {
        get { return this._asyncResult; }
    }
}