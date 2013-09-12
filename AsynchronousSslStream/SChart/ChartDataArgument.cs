using System;
using System.Data;
using System.Configuration;
using System.Web;
using iExchange.Common;
using Trader.Server.SChart;
using Trader.Common;

public class ChartDataArgument : HttpContextHolder
{
    private Guid _instrumentId;
    private DateTime _lastDate;
    private int _count;
    private string _dataCycle;
    private DataManager _dataManager;

    private AsyncResult _asyncResult;

    public ChartDataArgument(Guid instrumentId, DateTime lastDate, int count, string dataCycle, DataManager dataManager, AsyncResult asyncResult, Session session)
        : base(session)
    {
        this._instrumentId = instrumentId;
        this._lastDate = lastDate;
        this._count = count;
        this._dataCycle = dataCycle;
        this._dataManager = dataManager;

        this._asyncResult = asyncResult;
    }

    public Guid InstrumentId
    {
        get { return this._instrumentId; }
    }

    public DateTime LastDate
    {
        get { return this._lastDate; }
    }

    public int Count
    {
        get { return this._count; }
    }

    public string DataCycle
    {
        get { return this._dataCycle; }
    }

    public DataManager DataManager
    {
        get { return this._dataManager; }
    }

    public AsyncResult AsyncResult
    {
        get { return this._asyncResult; }
    }
}