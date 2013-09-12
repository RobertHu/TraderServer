using System;
using System.Data;
using System.Configuration;
using System.Web;
using iExchange.Common;
using Trader.Common;

public class StatementArg : HttpContextHolder
{
    private int _StatementReportType;
    private string _DayBegin;
    private string _DayTo;
    private string _IDs;
    private string _Rdlc;    
    private AsyncResult _AsyncResult;

    public StatementArg(int statementReportType, string dayBegin, string dayTo, string IDs, string rdlc, AsyncResult asyncResult, Session session)
        : base(session)
    {
        this._StatementReportType = statementReportType;
        this._DayBegin = dayBegin;
        this._DayTo = dayTo;
        this._IDs = IDs;
        this._Rdlc = rdlc;
        
        this._AsyncResult = asyncResult;
    }

    public int StatementReportType
    {
        get { return this._StatementReportType; }
    }

    public string DayBegin
    {
        get { return this._DayBegin; }
    }

    public string DayTo
    {
        get { return this._DayTo; }
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