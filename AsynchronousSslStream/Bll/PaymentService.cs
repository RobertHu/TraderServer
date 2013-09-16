using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using log4net;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml.Linq;

namespace Trader.Server.Bll
{
    public class PaymentService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof (PaymentService));
        public static XElement GetMerchantInfoFor99Bill(Guid[] organizationIds)
        {
            try
            {
                string[] infos = Application.Default.TradingConsoleServer.GetMerchantInfoFor99Bill(organizationIds);
                return XmlResultHelper.NewResult(infos.ToJoinString());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
