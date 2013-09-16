using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using log4net;
using Trader.Server.SessionNamespace;
using iExchange.Common;
using Trader.Server.Util;
using System.Xml.Linq;
using Trader.Server.TypeExtension;
using System.Xml;
using Trader.Common;
namespace Trader.Server.Bll
{
    public static class MessageService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof (MessageService));
        public static XElement GetMessages(Session session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                DataSet ds = Application.Default.TradingConsoleServer.GetMessages(token);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
