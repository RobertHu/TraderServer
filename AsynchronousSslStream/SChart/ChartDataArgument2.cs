using System;
using System.Data;
using System.Configuration;
using System.Web;
using iExchange.Common;
using Trader.Common;

public class ChartDataArgument2 : HttpContextHolder
{
    private Guid _instrumentId;
    private string _dataCycle;
    private DateTime _from;
    private DateTime _to;

    private AsyncResult _asyncResult;

    public ChartDataArgument2(Guid instrumentId, String dataCycle, DateTime from, DateTime to,
            AsyncResult asyncResult, Session session)
        : base(session)
    {
        this._instrumentId = instrumentId;
        this._dataCycle = dataCycle;
        this._from = from;
        this._to = to;
                
        this._asyncResult = asyncResult;
    }

    public Guid InstrumentId
    {
        get { return this._instrumentId; }
    }

    public string DataCycle
    {
        get { return this._dataCycle; }
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