using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Trader.Server.Session;
using iExchange.Common;
using Trader.Server.Util;
using System.Xml.Linq;
using Trader.Server.TypeExtension;
using System.Xml;
namespace Trader.Server.Bll
{
    public static class MessageService
    {
        public static XElement GetMessages(long session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                DataSet ds = Application.Default.TradingConsoleServer.GetMessages(token);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetMessages:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
