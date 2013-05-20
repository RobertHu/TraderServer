using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server.Session;
using System.Threading;
using System.Data;
using Trader.Server.Setting;
using Trader.Server.Bll;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Configuration;
using System.IO;
using Trader.Server.Service;
using Trader.Server.Report;
using System.Xml.Linq;
namespace Trader.Server.Bll
{
    public class StatementService
    {
        private static TimeSpan _StatementReportTimeout = TimeSpan.MinValue;
        private static TimeSpan _LedgerReportTimeout = TimeSpan.MinValue;

        public static XElement LedgerForJava2(long session, string dateFrom, string dateTo, string IDs, string rdlc)
        {
            Guid result = Guid.Empty;
            try
            {
                AsyncResult asyncResult = new AsyncResult("LedgerForJava2", session.ToString());
                Token token = SessionManager.Default.GetToken(session);
                if (ThreadPool.QueueUserWorkItem(CreateLedger, new LedgerArgument(dateFrom, dateTo, IDs, rdlc, asyncResult, session)))
                {
                    result = asyncResult.Id;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.LedgerForJava2:", "ThreadPool.QueueUserWorkItem failed", System.Diagnostics.EventLogEntryType.Warning);
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.LedgerForJava2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return XmlResultHelper.NewResult(result.ToString());
        }


        private static void CreateLedger(object state)
        {
            LedgerArgument ledgerArgument = (LedgerArgument)state;
            Token token = ledgerArgument.Token;
            string sql = "EXEC P_RptLedger @xmlAccounts=\'" + XmlTransform.Transform(ledgerArgument.IDs, ',', "Accounts", "Account", "ID") + "\',@tradeDayBegin=\'"
                + ledgerArgument.DateFrom + "\',@tradeDayTo=\'" + ledgerArgument.DateTo + "\',@language=\'" + ledgerArgument.Version + "\',@userID=\'" + ledgerArgument.Token.UserID.ToString() + "\'";
            try
            {
                DataSet dataSet = DataAccess.GetData(sql, SettingManager.Default.ConnectionString, LedgerReportTimeout);
                try
                {
                    TradingConsoleServer tradingConsoleServer = ledgerArgument.TradingConsoleServer;
                    tradingConsoleServer.SaveLedger(token, "",ledgerArgument.IDs);
                }
                catch
                {
                }

                if (dataSet.Tables.Count > 0)
                {
                    string filepath = Path.Combine(SettingManager.Default.PhysicPath, ledgerArgument.Rdlc); //this.Server.MapPath(ledgerArgument.Rdlc);
                    byte[] reportContent = PDFHelper.ExportPDF(filepath, dataSet.Tables[0]);
                    AsyncResultManager asyncResultManager = ledgerArgument.AsyncResultManager;
                    asyncResultManager.SetResult(ledgerArgument.AsyncResult, reportContent);
                    CommandManager.Default.AddCommand(ledgerArgument.Token, new AsyncCommand(0, ledgerArgument.AsyncResult));
                }
            }
            catch (System.Exception ex)
            {
                CommandManager.Default.AddCommand(ledgerArgument.Token, new AsyncCommand(0, ledgerArgument.AsyncResult, true, ex));
                AppDebug.LogEvent("TradingConsole.CreateLedger", sql + "\r\n" + ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }




        public static XElement  StatementForJava2(long session, int statementReportType, string dayBegin, string dayTo, string IDs, string rdlc)
        {
            Guid result = Guid.Empty;
            try
            {
                AsyncResult asyncResult = new AsyncResult("StatementForJava2", session.ToString());
                Token token = SessionManager.Default.GetToken(session);
                if (ThreadPool.QueueUserWorkItem(CreateStatement, new StatementArg(statementReportType, dayBegin, dayTo, IDs, rdlc, asyncResult, session)))
                {
                    result = asyncResult.Id;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.StatementForJava2:", "ThreadPool.QueueUserWorkItem failed", System.Diagnostics.EventLogEntryType.Warning);
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.StatementForJava2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return XmlResultHelper.NewResult(result.ToString());

        }


        public static XElement  GetReportContent(Guid asyncResultId)
        {
            String result = "";
            try
            {
                byte[] data = (byte[])Application.Default.AsyncResultManager.GetResult(asyncResultId);
                result = Convert.ToBase64String(data);
                Console.WriteLine("get report content");
                return XmlResultHelper.NewResult(result);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                AppDebug.LogEvent("TradingConsole.GetReportContent", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
           
        }

        private static void CreateStatement(object state)
        {
            StatementArg statementArg = (StatementArg)state;
            Token token = statementArg.Token;
            string sql = string.Empty;
            if (statementArg.Rdlc.ToLower().Contains("statement_mc"))
            {
                sql = "EXEC P_RptStatement_RC2 ";
            }
            else
            {
                switch (statementArg.StatementReportType)
                {
                    case 0:
                        sql = "EXEC P_RptStatement_RC2 ";
                        break;
                    case 1:
                        sql = "EXEC P_RptStatement2_RC2 ";
                        break;
                    case 2:
                        sql = "EXEC P_RptStatement4_RC2 ";
                        break;
                    case 3:
                        sql = "EXEC P_RptStatement5_RC2 ";
                        break;
                }
            }
            sql += "@xmlAccounts=" + "\'" + XmlTransform.Transform(statementArg.IDs, ',', "Accounts", "Account", "ID")
                + "\',@tradeDayBegin=\'" + statementArg.DayBegin + "\',@tradeDayTo=\'" + statementArg.DayTo + "\',@language=\'" + statementArg.Version + "\',@userID=\'" + statementArg.Token.UserID.ToString() + "\'";
            try
            {
                DataSet dataSet = DataAccess.GetData(sql, SettingManager.Default.ConnectionString, StatementReportTimeout);
                try
                {
                    TradingConsoleServer tradingConsoleServer = statementArg.TradingConsoleServer;
                    tradingConsoleServer.SaveStatement(token, "", statementArg.IDs);
                }
                catch
                {
                }
                if (dataSet.Tables.Count > 0)
                {
                    string filepath = Path.Combine(SettingManager.Default.PhysicPath,statementArg.Rdlc); // this.Server.MapPath(statementArg.Rdlc);
                    Console.WriteLine(filepath);
                    if (statementArg.Rdlc.ToLower().Contains("statement_mc") && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                    {
                        if (!(bool)dataSet.Tables[0].Rows[0]["IsMultiCurrency"])
                        {
                            filepath = filepath.ToLower().Replace("rptStatement_mc.rdlc", "RptStatement.rdlc");
                        }
                    }
                    byte[] reportContent = PDFHelper.ExportPDF(filepath, dataSet.Tables[0]);
                    AsyncResultManager asyncResultManager = statementArg.AsyncResultManager;
                    asyncResultManager.SetResult(statementArg.AsyncResult, reportContent);
                    CommandManager.Default.AddCommand(statementArg.Token, new AsyncCommand(0, statementArg.AsyncResult));
                }
            }
            catch (System.Exception ex)
            {
                CommandManager.Default.AddCommand(statementArg.Token, new AsyncCommand(0, statementArg.AsyncResult, true, ex));
                AppDebug.LogEvent("TradingConsole.CreateStatement", sql + "\r\n" + ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }


        public static XElement AccountSummaryForJava2(long session, string tradeDay, string accountIds, string rdlc)
        {
            Guid result = Guid.Empty;
            try
            {
                AsyncResult asyncResult = new AsyncResult("AccountSummaryForJava2", session.ToString());
                Token token = SessionManager.Default.GetToken(session);
                if (ThreadPool.QueueUserWorkItem(CreateAccountSummary, new AccountSummaryArgument(tradeDay, accountIds, rdlc, asyncResult, session)))
                {
                    result = asyncResult.Id;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.LedgerForJava2:", "ThreadPool.QueueUserWorkItem failed", System.Diagnostics.EventLogEntryType.Warning);
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.LedgerForJava2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
            return XmlResultHelper.NewResult(result.ToString());
        }



        private static void CreateAccountSummary(object state)
        {
            AccountSummaryArgument accountSummaryArgument = (AccountSummaryArgument)state;
            Token token = accountSummaryArgument.Token;
            string sql = "EXEC P_RptAccountSummary @xmlAccounts=\'" + XmlTransform.Transform(accountSummaryArgument.AccountIds, ',', "Accounts", "Account", "ID") + "\',@tradeDay=\'"
                + accountSummaryArgument.TradeDay + "\',@language=\'" + accountSummaryArgument.Version + "\',@userID=\'" + accountSummaryArgument.Token.UserID.ToString() + "\', @skipNoTransactionAccount=0";
            try
            {
                DataSet dataSet = DataAccess.GetData(sql, SettingManager.Default.ConnectionString, LedgerReportTimeout);
                if (dataSet.Tables.Count > 0)
                {
                    string filepath = Path.Combine(SettingManager.Default.PhysicPath, accountSummaryArgument.Rdlc);
                    //this.Server.MapPath(accountSummaryArgument.Rdlc);
                    byte[] reportContent = PDFHelper.ExportPDF(filepath, dataSet.Tables[0]);
                    AsyncResultManager asyncResultManager = accountSummaryArgument.AsyncResultManager;
                    asyncResultManager.SetResult(accountSummaryArgument.AsyncResult, reportContent);
                    CommandManager.Default.AddCommand(accountSummaryArgument.Token, new AsyncCommand(0, accountSummaryArgument.AsyncResult));
                }
            }
            catch (System.Exception ex)
            {
                CommandManager.Default.AddCommand(accountSummaryArgument.Token, new AsyncCommand(0, accountSummaryArgument.AsyncResult, true, ex));
                AppDebug.LogEvent("TradingConsole.CreateAccountSummary", sql + "\r\n" + ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }



        private static TimeSpan StatementReportTimeout
        {
            get
            {
                if (_StatementReportTimeout == TimeSpan.MinValue)
                {
                    _StatementReportTimeout = TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["StatementReportTimeoutInMillsecond"]));
                }
                return _StatementReportTimeout;
            }
        }

        private static TimeSpan LedgerReportTimeout
        {
            get
            {
                if (_LedgerReportTimeout == TimeSpan.MinValue)
                {
                    _LedgerReportTimeout = TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["LedgerReportTimeoutInMillsecond"]));
                }
                return _LedgerReportTimeout;
            }
        }
    }
}
