using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server.Session;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
namespace Trader.Server.Bll
{
    public class ClientService
    {
        public static XmlNode AdditionalClient(string session, string email, string receive, string organizationName, string customerName, string reportDate, string accountCode,
          string correspondingAddress, string registratedEmailAddress, string tel, string mobile, string fax, string fillName1, string ICNo1,
          string fillName2, string ICNo2, string fillName3, string ICNo3)
        {
            try
            {
                String reference;
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed=Application.Default.TradingConsoleServer.AdditionalClient(token, Application.Default.StateServer, FixBug.Fix(email), FixBug.Fix(receive),
                    FixBug.Fix(organizationName), FixBug.Fix(customerName),
                    FixBug.Fix(reportDate), FixBug.Fix(accountCode),
                    FixBug.Fix(correspondingAddress), FixBug.Fix(registratedEmailAddress),
                    FixBug.Fix(tel), FixBug.Fix(mobile), FixBug.Fix(fax),
                    FixBug.Fix(fillName1), FixBug.Fix(ICNo1),
                    FixBug.Fix(fillName2), FixBug.Fix(ICNo2),
                    FixBug.Fix(fillName3), FixBug.Fix(ICNo3), out reference);
                reference = isSucceed ? reference : string.Empty;
                return XmlResultHelper.NewResult(reference);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.AdditionalClient:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
          
        }



        public static XmlNode Agent(string session, string email, string receive, string organizationName, string customerName, string reportDate, string accountCode, string previousAgentCode,
            string previousAgentName, string newAgentCode, string newAgentName, string newAgentICNo, string dateReply)
        {
            try
            {
                String reference = "";
                String errorMessage = "";
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed = Application.Default.TradingConsoleServer.Agent(token, Application.Default.StateServer, FixBug.Fix(email), FixBug.Fix(receive),
                    FixBug.Fix(organizationName), FixBug.Fix(customerName), FixBug.Fix(reportDate),
                    FixBug.Fix(accountCode), FixBug.Fix(previousAgentCode), FixBug.Fix(previousAgentName),
                    FixBug.Fix(newAgentCode), FixBug.Fix(newAgentName), FixBug.Fix(newAgentICNo), FixBug.Fix(dateReply),
                    out reference, out errorMessage);
                if (isSucceed)
                {
                    return XmlResultHelper.NewResult(reference);
                }
                else
                {
                    return XmlResultHelper.NewResult(errorMessage);
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Agent:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }  
        }


        public static XmlNode CallMarginExtension(string session, string email, string receive, string organizationName, string customerName, string reportDate, string accountCode, string currency,
            string currencyValue, string dueDate)
        {
            try
            {
                String reference = "";
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed = Application.Default.TradingConsoleServer.CallMarginExtension(token, Application.Default.StateServer, FixBug.Fix(email), FixBug.Fix(receive),
                    FixBug.Fix(organizationName), FixBug.Fix(customerName), FixBug.Fix(reportDate),
                    FixBug.Fix(accountCode), FixBug.Fix(currency), FixBug.Fix(currencyValue),
                    FixBug.Fix(dueDate), out reference);
                reference = isSucceed ? reference : string.Empty;
                return XmlResultHelper.NewResult(reference);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.CallMarginExtension:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public static XmlNode FundTransfer(string session, string email, string receive, string organizationName, string customerName, string reportDate, string currency, string currencyValue, string accountCode, string bankAccount, string beneficiaryName, string replyDate)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed = Application.Default.TradingConsoleServer.FundTransfer(token, Application.Default.StateServer, FixBug.Fix(email),
                    FixBug.Fix(receive), FixBug.Fix(organizationName), FixBug.Fix(customerName),
                    FixBug.Fix(reportDate), FixBug.Fix(currency), FixBug.Fix(currencyValue),
                    FixBug.Fix(accountCode), FixBug.Fix(bankAccount), FixBug.Fix(beneficiaryName),
                    FixBug.Fix(replyDate));
                return XmlResultHelper.NewResult(isSucceed.ToXmlResult());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.FundTransfer:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
           
        }

        public static XmlNode PaymentInstruction(string session, string email, string receive, string organizationName, string customerName, string reportDate, string accountCode, string currency,
            string currencyValue, string beneficiaryName, string bankAccount, string bankerName, string bankerAddress, string swiftCode, string remarks, string thisisClient)
        {
            try
            {
                string reference = "";
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed = Application.Default.TradingConsoleServer.PaymentInstruction(token, "PI", Application.Default.StateServer, FixBug.Fix(email),
                    FixBug.Fix(receive), FixBug.Fix(organizationName), FixBug.Fix(customerName),
                    FixBug.Fix(reportDate), FixBug.Fix(accountCode), FixBug.Fix(currency),
                    FixBug.Fix(currencyValue), FixBug.Fix(beneficiaryName), FixBug.Fix(bankAccount),
                    FixBug.Fix(bankerName), FixBug.Fix(bankerAddress), FixBug.Fix(swiftCode),
                    FixBug.Fix(remarks), FixBug.Fix(thisisClient), null, null, out reference);
                reference = isSucceed ? reference : "";
                return XmlResultHelper.NewResult(reference);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.PaymentInstruction:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public static XmlNode PaymentInstructionInternal(string session, string email, string organizationName, string customerName, string reportDate, string accountCode,
            string currency, string amount, string beneficiaryAccount, string beneficiaryAccountOwner, string email2)
        {
            try
            {
                string reference = "";
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed = Application.Default.TradingConsoleServer.PaymentInstruction(token, "PIInterACTransfer", Application.Default.StateServer,
                    FixBug.Fix(email), null, FixBug.Fix(organizationName), FixBug.Fix(customerName),
                    FixBug.Fix(reportDate), FixBug.Fix(accountCode), FixBug.Fix(currency),
                    FixBug.Fix(amount), FixBug.Fix(beneficiaryAccountOwner), FixBug.Fix(beneficiaryAccount),
                    null, null, null, null, null, null, FixBug.Fix(email2), out reference);
                reference = isSucceed ? reference : "";
                return XmlResultHelper.NewResult(reference);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.PaymentInstruction:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public static XmlNode PaymentInstructionCash(string session, string email, string organizationName, string customerName, string reportDate,
            string accountCode, string currency, string amount, string beneficiaryName, string beneficiaryAddress)
        {
            try
            {
                string reference = "";
                Token token = SessionManager.Default.GetToken(session);

                bool isSucceed = Application.Default.TradingConsoleServer.PaymentInstruction(token, "PICash", Application.Default.StateServer, FixBug.Fix(email),
                    null, FixBug.Fix(organizationName), FixBug.Fix(customerName), FixBug.Fix(reportDate),
                    FixBug.Fix(accountCode), FixBug.Fix(currency), FixBug.Fix(amount), FixBug.Fix(beneficiaryName),
                    null, null, null, null, null, null, FixBug.Fix(beneficiaryAddress), null, out reference);
                reference = isSucceed ? reference : "";
                return XmlResultHelper.NewResult(reference);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.PaymentInstruction:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
            
        }

        public static XmlNode Assign(string session)
        {
            try
            {
                XmlNode xmlTransaction = null;
                XmlNode xmlAccount;
                XmlNode xmlInstrument;
                Token token = SessionManager.Default.GetToken(session);
                TransactionError error = Application.Default.TradingConsoleServer.Assign(token, Application.Default.StateServer, ref xmlTransaction, out xmlAccount, out xmlInstrument);
                var dict = new Dictionary<string, string>() { {"xmlTransaction",xmlTransaction.OuterXml},
                {"xmlAccount",xmlAccount.OuterXml},
                {"xmlInstrument",xmlInstrument.OuterXml},
                {"transactionError",error.ToString()}};
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Assign:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public static XmlNode ChangeLeverage(string session, Guid accountId, int leverage)
        {
            try
            {
                decimal necessary;
                Token token = SessionManager.Default.GetToken(session);
                bool isSucceed = Application.Default.TradingConsoleServer.ChangeLeverage(token, Application.Default.StateServer, accountId, leverage, out necessary);
                var dict = new Dictionary<string, string> { { "necessary", necessary.ToString() }, { "successed",isSucceed.ToXmlResult() } };
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.ChangeLeverage:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


    }
}
