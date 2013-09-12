using System;
using System.Data;
using System.Configuration;
using System.Web;
using iExchange.Common;
using Trader.Common;

/// <summary>
/// Summary description for LedgerArgument
/// </summary>
public class LedgerArgument : HttpContextHolder
{
    private string _DateFrom;
    private string _DateTo;
    private string _IDs;
    private string _Rdlc;    
    private AsyncResult _AsyncResult;

    public LedgerArgument(string dateFrom, string dateTo, string IDs, string rdlc, AsyncResult asyncResult, Session session)
        : base(session)
    {
        this._DateFrom = dateFrom;
        this._DateTo = dateTo;
        this._IDs = IDs;
        this._Rdlc = rdlc;

        this._AsyncResult = asyncResult;
    }

    public string DateFrom
    {
        get { return this._DateFrom; }
    }

    public string DateTo
    {
        get { return this._DateTo; }
    }

    public string IDs
    {
        get { return this._IDs; }
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