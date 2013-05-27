using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Serialization;
using iExchange.Common;
using Trader.Server.Util;
using Trader.Server.Service;
using Mobile = iExchange3Promotion.Mobile;
using Trader.Server.TypeExtension;
using log4net;
using System.Xml.Linq;
using Trader.Common;
using Wintellect.Threading.AsyncProgModel;
namespace Trader.Server.Bll
{
    public class RequestTable
    {
        private Dictionary<string, Func<SerializedObject,Token,XElement >> table = new Dictionary<string, Func<SerializedObject,Token,XElement >>();
        private ILog _Logger = LogManager.GetLogger(typeof(RequestTable));
        public static readonly RequestTable Default = new RequestTable();
        private Service _Service = new Service();
        private RequestTable()
        {
            table.Add("Login", LoginAction);
            table.Add("GetInitData", GetInitDataAction);
            table.Add("GetTimeInfo", GetTimeInfoAction);
            table.Add("GetNewsList2", GetNewsList2Action);
            table.Add("GetMessages", GetMessagesAction);
            table.Add("GetAccountBankReferenceData", GetAccountBankReferenceDataAction);
            table.Add("GetTickByTickHistoryData", GetTickByTickHistoryDataAction);
            table.Add("GetLostCommands", GetLostCommandsAction);
            table.Add("GetInstrumentForSetting", GetInstrumentForSettingAction);
            table.Add("UpdateInstrumentSetting", UpdateInstrumentSettingAction);
            table.Add("saveLog", saveLogAction);
            table.Add("GetAccountsForSetting", GetAccountsForSettingAction);
            table.Add("UpdateAccountsSetting", UpdateAccountsSettingAction);
            table.Add("UpdatePassword", UpdatePasswordAction);
            table.Add("StatementForJava2", StatementForJava2Action);
            table.Add("GetReportContent", GetReportContentAction);
            table.Add("GetMerchantInfoFor99Bill", GetMerchantInfoFor99BillAction);
            table.Add("AdditionalClient", AdditionalClientAction);
            table.Add("Agent", AgentAction);
            table.Add("CallMarginExtension", CallMarginExtensionAction);
            table.Add("FundTransfer", FundTransferAction);
            table.Add("PaymentInstruction", PaymentInstructionAction);
            table.Add("PaymentInstructionInternal", PaymentInstructionInternalAction);
            table.Add("PaymentInstructionCash", PaymentInstructionCashAction);
            table.Add("Assign", AssignAction);
            table.Add("ChangeLeverage", ChangeLeverageAction);
            table.Add("AsyncGetChartData2", AsyncGetChartData2Action);
            table.Add("GetChartData", GetChartDataAction);
            table.Add("VerifyTransaction", VerifyTransactionAction);
            table.Add("Place", PlaceAction);
            table.Add("Recover", RecoverAction);
            table.Add("Logout", LogoutAction);
            table.Add("LedgerForJava2", LedgerForJava2Action);
            table.Add("Quote", QuoteAction);
            table.Add("Quote2", Quote2Action);
            table.Add("RecoverPasswordDatas", RecoverPasswordDatasAction);
            table.Add("ChangeMarginPin", ChangeMarginPinAction);
            table.Add("ModifyTelephoneIdentificationCode", ModifyTelephoneIdentificationCodeAction);
            table.Add("GetAccountBanksApproved", GetAccountBanksApprovedAction);
            table.Add("Apply", ApplyAction);
            table.Add("Cancel", CancelAction);
            table.Add("ModifyOrder", ModifyOrderAction);
            table.Add("QueryOrder", QueryOrderAction);
            table.Add("DeleteMessage", DeleteMessageAction);
            table.Add("MultipleClose", MultipleCloseAction);
            table.Add("VerifyMarginPin", VerifyMarginPinAction);
            table.Add("CancelLMTOrder", CancelLMTOrderAction);
            table.Add("GetInterestRateByInterestRateId", GetInterestRateByInterestRateIdAction);
            table.Add("GetInterestRateByOrderId", GetInterestRateByOrderIdAction);
            table.Add("AccountSummaryForJava2", AccountSummaryForJava2Action);

        }


        public XElement Execute(string methodName,SerializedObject request,Token token)
        {
            if (table.ContainsKey(methodName))
            {
                return table[methodName](request, token);
            }
            else
            {
                this._Logger.InfoFormat("the request methed {0} not exist", methodName);
                return XmlResultHelper.ErrorResult;
            }
        }
        private XElement AccountSummaryForJava2Action(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.AccountSummaryForJava2(request.Session, args[0], args[1], args[2]);
        }
        private XElement GetInterestRateByOrderIdAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return InterestRateService.GetInterestRate(args[0].ToGuidArray());

        }
        private XElement GetInterestRateByInterestRateIdAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return InterestRateService.GetInterestRate2(request.Session, args[0].ToGuid());
        }

        private XElement CancelLMTOrderAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.CancelLMTOrder(request.Session, args[0]);
        }
        private XElement VerifyMarginPinAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.VerifyMarginPin(args[0].ToGuid(), args[1]);
        }

        private XElement MultipleCloseAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TransactionService.MultipleClose(request.Session, args[0].ToGuidArray());

        }
        private XElement DeleteMessageAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.DeleteMessage(request.Session, args[0].ToGuid());
        }
        private XElement QueryOrderAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);

            if (token.AppType == AppType.Mobile)
            {
                Guid? instrumentId = null;
                if (!string.IsNullOrEmpty(args[0]))
                {
                    instrumentId = new Guid(args[0]);
                }

                int lastDays = int.Parse(args[1]);
                int orderStatus = int.Parse(args[2]);
                int orderType = int.Parse(args[3]);
                return null;
                //return Mobile.Manager.QueryOrder(token, instrumentId, lastDays, orderStatus, orderType);
            }
            else
            {
                return this._Service.OrderQuery(request.Session, args[0].ToGuid(), args[1], args[2], args[3].ToInt());
            }

        }
        private XElement ApplyAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.Apply(request.Session, args[0].ToGuid(), args[1], args[2],
                args[3], args[4], args[5], args[6],
                args[7], args[8], args[9], args[10].ToGuid(),
                args[11], args[12], args[13], args[14], args[15], args[16], args[17]
                , args[18].ToInt());

        }
        private XElement GetAccountBanksApprovedAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return AccountManager.GetAccountBanksApproved(Guid.Parse(args[0]),args[1]);
        }

        private XElement ModifyTelephoneIdentificationCodeAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.ModifyTelephoneIdentificationCode(request.Session, Guid.Parse(args[0]), args[1], args[2]);
        }

        private XElement ChangeMarginPinAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.ChangeMarginPin(Guid.Parse(args[0]), args[1], args[2]);
        }

        private XElement RecoverPasswordDatasAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            var args = argList[0].To2DArray();
            return PasswordService.RecoverPasswordDatas(request.Session, args);
        }

        private XElement QuoteAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.Quote(request.Session, argList[0], double.Parse(argList[1]), int.Parse(argList[2]));
        }

        private XElement Quote2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.Quote2(request.Session, argList[0], double.Parse(argList[1]), double.Parse(argList[2]), int.Parse(argList[3]));
        }

        private XElement LedgerForJava2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.LedgerForJava2(request.Session, argList[0], argList[1], argList[2], argList[3]);
        }

        private XElement LoginAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            XElement result = null;
            AsyncEnumerator ae = new AsyncEnumerator();
            int appType = argList[3].ToInt();
            IAsyncResult asyncResult = ae.BeginExecute(LoginManager.Default.Login(request, argList[0], argList[1], argList[2], appType, ae), ae.EndExecute);
            if ((int)AppType.Mobile == appType)
            {
                ae.EndExecute(asyncResult);
                token = Trader.Server.Session.SessionManager.Default.GetToken(request.Session);
                result = iExchange3Promotion.Mobile.Manager.Login(token);
            }
            return result;
        }

        private XElement  GetInitDataAction(SerializedObject request, Token token)
        {
            XElement  result=null;
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                System.Data.DataSet initData = Mobile.Manager.GetInitData(token);
                List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
                Guid selectedAccountId = (argList != null && argList.Count > 0 ? new Guid(argList[0]) : Guid.Empty);
                InitDataService.Init(request.Session, initData);
                result = Mobile.Manager.Initialize(token, initData, selectedAccountId);
                //test:
                if (System.Configuration.ConfigurationManager.AppSettings["MobileDebug"]=="true")
                {
                    ////test place
                    //string s = "<PlacingInstruction AccountId=\"cbcdb06f-141a-415f-bdda-a676bd5759b7\" InstrumentId=\"864ac5d7-b872-45a6-887e-7189463beb12\" PlacingType=\"LimitStop\" EndTime=\"2013-05-02 12:19:07.007\" ExpireType=\"GoodTillSession\" ExpireDate=\"2013-05-02 12:19:07.007\" PriceIsQuote=\"false\" PriceTimestamp=\"2013-05-02 12:19:07.007\"><PlacingOrders><PlacingOrder Id=\"e2a72e0d-ddf6-4d83-8a04-9883a0eee84e\" Lot=\"1\" IsOpen=\"true\" IsBuy=\"false\" SetPrice=\"2.0988\" TradeOption=\"Better\" /></PlacingOrders></PlacingInstruction>";                    
                    //XmlDocument doc = new XmlDocument();
                    //doc.LoadXml(s);
                    //ICollection<Mobile.Server.Transaction> transactionsTest = Mobile.Manager.ConvertPlacingRequest(token, doc.FirstChild);

                    //foreach (Mobile.Server.Transaction transaction in transactionsTest)
                    //{
                    //    string tranCode;
                    //    TransactionError error = Application.Default.TradingConsoleServer.Place(token, Application.Default.StateServer, transaction.ToXmlNode(), out tranCode);
                    //}
                    //// test quote
                    //this._Service.Quote(request.Session, "282fa038-1330-43a6-aa87-9d7d01c1a594", 30, 2);
                    // test order query
                   // XElement  node =Mobile.Manager.QueryOrder(token, new Guid("1DFC557A-3FB5-4A9F-BD08-E165FA6DFCE6"), 10, 4, 3);
                }
            }
            else
            {
                AsyncEnumerator ae = new AsyncEnumerator();
                ae.BeginExecute(InitDataService.GetInitData(request, null, ae),ae.EndExecute);
            }
            return result;
        }

        private XElement  GetTimeInfoAction(SerializedObject request, Token token)
        {
            return TimeService.GetTimeInfo();
        }

        private XElement  GetNewsList2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return NewsService.GetNewsList2(argList[0], argList[1], DateTime.Parse(argList[2]));
        }

        private XElement  GetMessagesAction(SerializedObject request, Token token)
        {
            return MessageService.GetMessages(request.Session);

        }

        private XElement  GetAccountBankReferenceDataAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return AccountManager.Default.GetAccountBankReferenceData(argList[0], argList[1]);
        }

        private XElement  GetTickByTickHistoryDataAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return TickService.GetTickByTickHistoryData(request.Session, Guid.Parse(argList[0]), DateTime.Parse(argList[1]), DateTime.Parse(argList[2]));
        }

        private XElement  GetLostCommandsAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return CommandManager.Default.GetLostCommands(request.Session, int.Parse(argList[0]), int.Parse(argList[1]));
        }

        private XElement  GetInstrumentForSettingAction(SerializedObject request, Token token)
        {
            return InstrumentManager.Default.GetInstrumentForSetting(request.Session);
        }

        private XElement  UpdateInstrumentSettingAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            string[] instrumentIds = argList[0].Split(StringConstants.ArrayItemSeparator);
            return InstrumentManager.Default.UpdateInstrumentSetting(request.Session, instrumentIds);

        }

        private XElement  saveLogAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return LogService.SaveLog(request.Session, argList[0], DateTime.Parse(argList[1]), argList[2], Guid.Parse(argList[3]));
        }

        private XElement  GetAccountsForSettingAction(SerializedObject request, Token token)
        {
            return AccountManager.Default.GetAccountsForTradingConsole(request.Session);
        }


        private XElement  UpdateAccountsSettingAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            Guid[] accountIds = argList[0].ToGuidArray();
            return AccountManager.Default.UpdateAccountSetting(request.Session, accountIds);
        }

        private XElement  UpdatePasswordAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.UpdatePassword(request.Session, argList[0], argList[1], argList[2]);
        }

        private XElement  StatementForJava2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.StatementForJava2(request.Session, int.Parse(argList[0]), argList[1], argList[2], argList[3], argList[4]);
        }

        private XElement  GetReportContentAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.GetReportContent(Guid.Parse(argList[0]));
        }

        private XElement  GetMerchantInfoFor99BillAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            Guid[] organizationIds = argList[0].ToGuidArray();
            return PaymentService.GetMerchantInfoFor99Bill(organizationIds);
        }

        private XElement  AdditionalClientAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.AdditionalClient(request.Session, argList[0], argList[1], argList[2],
                argList[3], argList[4], argList[5], argList[6], argList[7], argList[8],
                argList[9], argList[10], argList[11], argList[12], argList[13], argList[14],
                argList[15], argList[16]);
        }

        private XElement  AgentAction(SerializedObject request, Token token)
        {
            List<string> args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.Agent(request.Session, args[0], args[1], args[2],
                args[3], args[4], args[5], args[6], args[7], args[8],
                args[9], args[10], args[11]);
        }

        private XElement  CallMarginExtensionAction(SerializedObject request, Token token)
        {
            List<string> args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.CallMarginExtension(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
        }

        private XElement  FundTransferAction(SerializedObject request, Token token)
        {
            List<string> args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.FundTransfer(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
        }

        private XElement  PaymentInstructionAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.PaymentInstruction(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10],
                args[11], args[12], args[13], args[14]);
        }

        private XElement  PaymentInstructionInternalAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.PaymentInstructionInternal(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);

        }

        private XElement  PaymentInstructionCashAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.PaymentInstructionCash(request.Session, args[0], args[1], args[2],
                args[3], args[4], args[5], args[6], args[7], args[8]);

        }

        private XElement  AssignAction(SerializedObject request, Token token)
        {
            return ClientService.Assign(request.Session);
        }

        private XElement  ChangeLeverageAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.ChangeLeverage(request.Session, Guid.Parse(args[0]), int.Parse(args[1]));

        }

        private XElement  AsyncGetChartData2Action(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TickService.AsyncGetChartData2(request.Session, Guid.Parse(args[0]), DateTime.Parse(args[1]), DateTime.Parse(args[2]), args[3]);

        }

        private XElement  GetChartDataAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TickService.GetChartData(Guid.Parse(args[0]));
        }

        private XElement  VerifyTransactionAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TransactionService.VerifyTransaction(request.Session, args[0].ToGuidArray());

        }

        private XElement  PlaceAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                ICollection<Mobile.Server.Transaction> transactions = Mobile.Manager.ConvertPlacingRequest(token, args[0].ToXmlNode());
                XElement element = new XElement("Result");
                foreach (Mobile.Server.Transaction transaction in transactions)
                {
                    ICollection<XElement> errorCodes = this.GetPlaceResultForMobile(transaction, token);
                    foreach(XElement orderErrorElement in errorCodes){
                        element.Add(orderErrorElement);
                    }
                }
                return element;
            }
            else
            {
                return TransactionService.Place(request.Session, args[0].ToXmlNode());
            }
        }

        private XElement  ModifyOrderAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                Guid orderId = new Guid(args[0]);
                string price = args[1];
                Guid? orderId2 = null;
                if (!string.IsNullOrEmpty(args[2]))
                {
                    orderId2 = new Guid(args[2]);
                }
                string price2 = args[3];
                string order1DoneLimitPrice = args[4];
                string order1DoneStopPrice = args[5];
                string order2DoneLimitPrice = args[6];
                string order2DoneStopPrice = args[7];
                bool isOco = bool.Parse(order1DoneLimitPrice = args[8]);

                Mobile.Server.Transaction transaction = Mobile.Manager.ConvertModifyRequest(token, orderId, price, orderId2, price2, order1DoneLimitPrice, order1DoneStopPrice, order2DoneLimitPrice, order2DoneStopPrice, isOco);
                XElement element = new XElement("Result");

                ICollection<XElement> errorCodes = this.GetPlaceResultForMobile(transaction, token);
                foreach (XElement orderErrorElement in errorCodes)
                {
                    element.Add(orderErrorElement);
                }
                return element;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public XElement  CancelAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            Guid transactionId = new Guid(args[0]);
            TransactionError transactionError = Application.Default.StateServer.Cancel(token, transactionId, CancelReason.CustomerCanceled);
            if (token.AppType == AppType.Mobile)
            {
                XElement errorElement = new XElement("Transaction");
                errorElement.SetAttributeValue("Id", transactionId);
                errorElement.SetAttributeValue("ErrorCode", transactionError.ToString());

                XElement element = new XElement("Result");
                element.Add(errorElement);
                return element;
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private XElement  RecoverAction(SerializedObject request, Token token)
        {
            return RecoverService.Recover(request.Session, request.CurrentSession);
        }

        private XElement  LogoutAction(SerializedObject request, Token token)
        {
           return LoginManager.Default.Logout(request.Session);
        }

        private ICollection<XElement> GetPlaceResultForMobile(Mobile.Server.Transaction transaction, Token token)
        {
            ICollection<XElement> elements = new List<XElement>();
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                string tranCode;
                TransactionError error = Application.Default.TradingConsoleServer.Place(token, Application.Default.StateServer, transaction.ToXmlNode(), out tranCode);
                if (error == TransactionError.Action_ShouldAutoFill)
                {
                    error = TransactionError.OK;
                }

                foreach (Mobile.Server.Order order in transaction.Orders)
                {
                    XElement orderErrorElement = new XElement("Order");
                    orderErrorElement.SetAttributeValue("Id", order.Id);
                    orderErrorElement.SetAttributeValue("ErrorCode", error.ToString());
                    elements.Add(orderErrorElement);
                }
                return elements;
            }
            else
            {
                return null;
            }
        }
    }
}
