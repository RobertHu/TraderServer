using System;
using System.Data;
using System.Configuration;
using System.Web;
using iExchange.Common;
using System.Net;
using Trader.Server.Bll;
using Trader.Server.Service;
using Trader.Common;
using Trader.Server.SessionNamespace;

public class TickByTickHistoryDataArgument : HttpContextHolder
{
    private Guid _instrumentId;
    
    private AsyncResult _asyncResult;

    public TickByTickHistoryDataArgument(Guid instrumentId, AsyncResult asyncResult,Session session)
        :base(session)
    {
        this._instrumentId = instrumentId;        
        this._asyncResult = asyncResult;
    }

    public Guid InstrumentId
    {
        get { return this._instrumentId; }
    }

    public AsyncResult AsyncResult
    {
        get { return this._asyncResult; }
    }
}

public class HttpContextHolder
{
    private String _Version;

    private TradingConsoleServer _TradingConsoleServer;
    private AsyncResultManager _AsyncResultManager;
 
    private Token _Token;
    private TradingConsoleState _TradingConsoleState;

    public HttpContextHolder(Session session)
    {
        string version = SessionManager.Default.GetVersion(session);
        this._Version = version == null ? "ENG" : version;

        this._TradingConsoleServer = Application.Default.TradingConsoleServer;
        this._AsyncResultManager = Application.Default.AsyncResultManager;

        this._Token = SessionManager.Default.GetToken(session);
        this._TradingConsoleState = SessionManager.Default.GetTradingConsoleState(session);
    }


    public TradingConsoleServer TradingConsoleServer
    {
        get { return this._TradingConsoleServer; }
    }

    public AsyncResultManager AsyncResultManager
    {
        get { return this._AsyncResultManager; }
    }


    public TradingConsoleState TradingConsoleState
    {
        get { return this._TradingConsoleState; }
    }

    public Token Token
    {
        get { return this._Token; }
    }

    public String Version
    {
        get { return this._Version; }
    }
}