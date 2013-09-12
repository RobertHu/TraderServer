using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server.SessionNamespace;
using iExchange.Common;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml.Linq;
using Trader.Common;

namespace Trader.Server.Bll
{
    public class TransactionService
    {

        public static XElement Place(Session  session, XmlNode tran)
        {
            try
            {
                string tranCode=string.Empty;
                Token token = SessionManager.Default.GetToken(session);
                foreach (XmlNode child in tran.ChildNodes)
                {
                    if (child.Name == "Order")
                    {
                        if (child.Attributes["Extension"] != null
                            && child.Attributes["Extension"].Value.StartsWith("IfDone"))
                        {
                            string oldValue = child.Attributes["Extension"].Value;
                            XmlDocument document = new XmlDocument();
                            XmlElement element = document.CreateElement("IfDone");
                            string[] items = oldValue.Split(new char[] { ' ' });
                            foreach (string item in items)
                            {
                                if (item != "IfDone")
                                {
                                    string[] keyValue = item.Split(new char[] { '=' });
                                    if (keyValue[0] == "LimitPrice" || keyValue[0] == "StopPrice")
                                    {
                                        element.SetAttribute(keyValue[0], keyValue[1]);
                                    }
                                }
                            }
                            child.Attributes["Extension"].Value = element.OuterXml;
                        }
                    }
                }
                TransactionError error=Application.Default.TradingConsoleServer.Place(token, Application.Default.StateServer, tran, out tranCode);
                var dict = new Dictionary<string, string>() { { "transactionError", error.ToString() }, { "tranCode",tranCode } };
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Place:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public static XElement VerifyTransaction(Session  session, Guid[] transactionIDs)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                Guid[] result =Application.Default.TradingConsoleServer.VerifyTransaction(token, Application.Default.StateServer, transactionIDs);
                return XmlResultHelper.NewResult(result.ToJoinString());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.VerifyTransaction:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public static XElement MultipleClose(Session  session, Guid[] orderIds)
        {
            XmlNode xmlTran;
            XmlNode xmlAccount;
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TransactionError error =Application.Default.TradingConsoleServer.MultipleClose(token, Application.Default.StateServer, orderIds, out xmlTran, out xmlAccount);
                var dict = new Dictionary<string, string>()
                {
                    {"transactionError",error.ToString()},
                    {"xmlTran",xmlTran==null?string.Empty:xmlTran.OuterXml},
                    {"xmlAccount",xmlAccount==null?string.Empty:xmlAccount.OuterXml}
                };
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.MultipleClose:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
