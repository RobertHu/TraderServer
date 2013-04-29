using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using AsyncSslServer.Util;
using AsyncSslServer.TypeExtension;
namespace AsyncSslServer.Bll
{
    public class PaymentService
    {
        public static XmlNode GetMerchantInfoFor99Bill(Guid[] organizationIds)
        {
            try
            {
                string[] infos = Application.Default.TradingConsoleServer.GetMerchantInfoFor99Bill(organizationIds);
                return XmlResultHelper.NewResult(infos.ToJoinString());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetMerchantInfoFor99Bill:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
