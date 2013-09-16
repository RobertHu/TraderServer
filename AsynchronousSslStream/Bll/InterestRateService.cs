using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using System.Xml;
using log4net;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml.Linq;
using Trader.Common;
using Trader.Server.SessionNamespace;
namespace Trader.Server.Bll
{
    public class InterestRateService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof (InterestRateService));
        public static XElement GetInterestRate(Guid[] orderIds)
        {
            try
            {
                DataSet ds = Application.Default.TradingConsoleServer.GetInterestRate(orderIds);
                return XmlResultHelper.NewResult( ds.ToXml());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public static XElement GetInterestRate2(Session session, Guid interestRateId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                DataSet ds = Application.Default.TradingConsoleServer.GetInterestRate2(token, interestRateId);
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
