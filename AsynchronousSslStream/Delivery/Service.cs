using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using Trader.Server.Bll;
using Trader.Common;
using Trader.Server.SessionNamespace;
using System.Data;
using Trader.Server.TypeExtension;
using System.Xml.Linq;
using Trader.Server.Util;
using log4net;

namespace Trader.Server.Delivery
{
    public class Service
    {
        private readonly ILog _Logger = LogManager.GetLogger(typeof(Service));
        public XElement ApplyDelivery(Session session, XmlNode deliveryRequire)
        {
            try
            { 
                string balance;
                string usableMargin;
                Token token = SessionManager.Default.GetToken(session);
                string code = null;
                TransactionError tranError= Application.Default.StateServer.ApplyDelivery(token, ref deliveryRequire, out code, out balance, out usableMargin);
                var dict = new Dictionary<string,string>
                {
                    {"transactionError",tranError.ToString()},
                    {"balance",balance},
                    {"usableMargin",usableMargin}
                };
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                _Logger.Error("ApplyDelivery",exception);
                return XmlResultHelper.ErrorResult;
            }
        }


        public XElement GetDeliveryAddress(Session session,Guid deliveryPointGroupId)
        {
            try
            {
                string language = SessionManager.Default.GetToken(session).Language;
                TradingConsoleServer tradingConsoleServer = Application.Default.TradingConsoleServer;
                string[] addresses = tradingConsoleServer.GetDeliveryAddress(deliveryPointGroupId, language);
                return XmlResultHelper.NewResult(addresses.ToJoinString());
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public XElement GetOrderInstalment(Guid orderId)
        {
            try
            {
                TradingConsoleServer tradingConsoleServer = Application.Default.TradingConsoleServer;
                DataSet ds = tradingConsoleServer.GetOrderInstalment(orderId);
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
