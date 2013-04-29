using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AsyncSslServer.Session;
using iExchange.Common;
using AsyncSslServer.Util;
using System.Xml.Linq;
using AsyncSslServer.TypeExtension;
using System.Xml;
namespace AsyncSslServer.Bll
{
    public static class MessageService
    {
        public static XmlNode GetMessages(string session)
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
