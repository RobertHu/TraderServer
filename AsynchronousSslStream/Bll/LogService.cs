using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using AsyncSslServer.Session;
using System.Xml;
using AsyncSslServer.Util;
using AsyncSslServer.TypeExtension;
namespace AsyncSslServer.Bll
{
    public class LogService
    {
        public static XmlNode SaveLog(string session, string logCode, DateTime timestamp, string action, Guid transactionId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state == null ? false : state.IsEmployee;
                Application.Default.TradingConsoleServer.SaveLog(token, isEmployee,"", logCode, timestamp, action, transactionId);
                return XmlResultHelper.NewResult("");
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.SaveLog:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
