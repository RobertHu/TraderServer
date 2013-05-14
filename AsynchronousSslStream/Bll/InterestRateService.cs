using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using Trader.Server.Session;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml.Linq;
namespace Trader.Server.Bll
{
    public class InterestRateService
    {
        public static XElement GetInterestRate(Guid[] orderIds)
        {
            try
            {
                DataSet ds = Application.Default.TradingConsoleServer.GetInterestRate(orderIds);
                return XmlResultHelper.NewResult( ds.ToXml());
               
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetInterestRate:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public static XElement GetInterestRate2(Guid session, Guid interestRateId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                DataSet ds = Application.Default.TradingConsoleServer.GetInterestRate2(token, interestRateId);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetInterestRate2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
