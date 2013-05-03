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
namespace Trader.Server.Bll
{
    public class RequestTable
    {
        private Dictionary<string, Func<SerializedObject,Token,XmlNode>> table = new Dictionary<string, Func<SerializedObject,Token,XmlNode>>();
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
            table.Add("KeepAlive", KeepAliveAction);
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
            table.Add("QueryOrder", QueryOrderAction);
            table.Add("DeleteMessage", DeleteMessageAction);
            table.Add("MultipleClose", MultipleCloseAction);
            table.Add("VerifyMarginPin", VerifyMarginPinAction);
        }


        public XmlNode Execute(string methodName,SerializedObject request,Token token)
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

        private XmlNode VerifyMarginPinAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.VerifyMarginPin(args[0].ToGuid(), args[1]);
        }

        private XmlNode MultipleCloseAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TransactionService.MultipleClose(request.Session, args[0].ToGuidArray());

        }
        private XmlNode DeleteMessageAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.DeleteMessage(request.Session, args[0].ToGuid());
        }
        private XmlNode QueryOrderAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.OrderQuery(request.Session, args[0].ToGuid(), args[1], args[2], args[3].ToInt());

        }
        private XmlNode ApplyAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.Apply(request.Session, args[0].ToGuid(), args[1], args[2],
                args[3], args[4], args[5], args[6],
                args[7], args[8], args[9], args[10].ToGuid(),
                args[11], args[12], args[13], args[14], args[15], args[16], args[17]
                , args[18].ToInt());

        }
        private XmlNode GetAccountBanksApprovedAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return AccountManager.GetAccountBanksApproved(Guid.Parse(args[0]),args[1]);
        }

        private XmlNode ModifyTelephoneIdentificationCodeAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.ModifyTelephoneIdentificationCode(request.Session, Guid.Parse(args[0]), args[1], args[2]);
        }

        private XmlNode ChangeMarginPinAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.ChangeMarginPin(Guid.Parse(args[0]), args[1], args[2]);
        }

        private XmlNode RecoverPasswordDatasAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            var args = argList[0].To2DArray();
            return PasswordService.RecoverPasswordDatas(request.Session, args);
        }

        private XmlNode QuoteAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.Quote(request.Session, argList[0], double.Parse(argList[1]), int.Parse(argList[2]));
        }

        private XmlNode Quote2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return this._Service.Quote2(request.Session, argList[0], double.Parse(argList[1]), double.Parse(argList[2]), int.Parse(argList[3]));
        }

        private XmlNode LedgerForJava2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.LedgerForJava2(request.Session, argList[0], argList[1], argList[2], argList[3]);
        }

        private XmlNode LoginAction(SerializedObject request,Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            var result = LoginManager.Default.Login(request.Session, argList[0], argList[1], argList[2], int.Parse(argList[3]));
            token = Trader.Server.Session.SessionManager.Default.GetToken(request.Session);
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                result=iExchange3Promotion.Mobile.Manager.Login(token).ToXmlNode();
            }
            return result;
        }

        private XmlNode GetInitDataAction(SerializedObject request, Token token)
        {
            XmlNode result;
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                System.Data.DataSet initData = Mobile.Manager.GetInitData(token);
                List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
                Guid selectedAccountId = (argList != null && argList.Count > 0 ? new Guid(argList[0]) : Guid.Empty);
                InitDataService.GetInitData(request.Session, initData);
                result = Mobile.Manager.Initialize(token, initData, selectedAccountId);

                //test:
                if (System.Configuration.ConfigurationManager.AppSettings["MobileDebug"]=="true")
                {
                    string s = "<PlacingInstruction AccountId=\"cbcdb06f-141a-415f-bdda-a676bd5759b7\" InstrumentId=\"864ac5d7-b872-45a6-887e-7189463beb12\" PlacingType=\"LimitStop\" EndTime=\"2013-05-02 12:19:07.007\" ExpireType=\"GoodTillSession\" ExpireDate=\"2013-05-02 12:19:07.007\" PriceIsQuote=\"false\" PriceTimestamp=\"2013-05-02 12:19:07.007\"><PlacingOrders><PlacingOrder Id=\"e2a72e0d-ddf6-4d83-8a04-9883a0eee84e\" Lot=\"1\" IsOpen=\"true\" IsBuy=\"false\" SetPrice=\"2.0988\" TradeOption=\"Better\" /></PlacingOrders></PlacingInstruction>";
                    //s = System.Web.HttpUtility.HtmlDecode(s);
                    //s = System.Web.HttpUtility.HtmlDecode(s);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(s);
                    ICollection<Mobile.Server.Transaction> transactionsTest = Mobile.Manager.ConvertPlacingRequest(token, doc.FirstChild);

                    foreach (Mobile.Server.Transaction transaction in transactionsTest)
                    {
                        //XmlNode TransactionService.Place(request.Session, args[0].ToXmlNode());
                        string tranCode;
                        TransactionError error = Application.Default.TradingConsoleServer.Place(token, Application.Default.StateServer, transaction.ToXmlNode(), out tranCode);

                    }
                    
                    this._Service.Quote(request.Session, "282fa038-1330-43a6-aa87-9d7d01c1a594", 30, 2);
                }
            }
            else
            {
                result = InitDataService.GetInitData(request.Session, null);
            }
            return result;
        }

        private XmlNode GetTimeInfoAction(SerializedObject request, Token token)
        {
            return TimeService.GetTimeInfo();
        }

        private XmlNode GetNewsList2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return NewsService.GetNewsList2(argList[0], argList[1], DateTime.Parse(argList[2]));
        }

        private XmlNode GetMessagesAction(SerializedObject request, Token token)
        {
            return MessageService.GetMessages(request.Session);

        }

        private XmlNode GetAccountBankReferenceDataAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return AccountManager.Default.GetAccountBankReferenceData(argList[0], argList[1]);
        }

        private XmlNode GetTickByTickHistoryDataAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return TickService.GetTickByTickHistoryData(request.Session, Guid.Parse(argList[0]), DateTime.Parse(argList[1]), DateTime.Parse(argList[2]));
        }

        private XmlNode GetLostCommandsAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return CommandManager.Default.GetLostCommands(request.Session, int.Parse(argList[0]), int.Parse(argList[1]));
        }

        private XmlNode GetInstrumentForSettingAction(SerializedObject request, Token token)
        {
            return InstrumentManager.Default.GetInstrumentForSetting(request.Session);
        }

        private XmlNode UpdateInstrumentSettingAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            string[] instrumentIds = argList[0].Split(StringConstants.ArrayItemSeparator);
            return InstrumentManager.Default.UpdateInstrumentSetting(request.Session, instrumentIds);

        }

        private XmlNode saveLogAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return LogService.SaveLog(request.Session, argList[0], DateTime.Parse(argList[1]), argList[2], Guid.Parse(argList[3]));
        }

        private XmlNode GetAccountsForSettingAction(SerializedObject request, Token token)
        {
            return AccountManager.Default.GetAccountsForTradingConsole(request.Session);
        }


        private XmlNode UpdateAccountsSettingAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            Guid[] accountIds = argList[0].ToGuidArray();
            return AccountManager.Default.UpdateAccountSetting(request.Session, accountIds);
        }

        private XmlNode UpdatePasswordAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return PasswordService.UpdatePassword(request.Session, argList[0], argList[1], argList[2]);
        }

        private XmlNode StatementForJava2Action(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.StatementForJava2(request.Session, int.Parse(argList[0]), argList[1], argList[2], argList[3], argList[4]);
        }

        private XmlNode GetReportContentAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return StatementService.GetReportContent(Guid.Parse(argList[0]));
        }

        private XmlNode GetMerchantInfoFor99BillAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            Guid[] organizationIds = argList[0].ToGuidArray();
            return PaymentService.GetMerchantInfoFor99Bill(organizationIds);
        }

        private XmlNode AdditionalClientAction(SerializedObject request, Token token)
        {
            List<string> argList = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.AdditionalClient(request.Session, argList[0], argList[1], argList[2],
                argList[3], argList[4], argList[5], argList[6], argList[7], argList[8],
                argList[9], argList[10], argList[11], argList[12], argList[13], argList[14],
                argList[15], argList[16]);
        }

        private XmlNode AgentAction(SerializedObject request, Token token)
        {
            List<string> args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.Agent(request.Session, args[0], args[1], args[2],
                args[3], args[4], args[5], args[6], args[7], args[8],
                args[9], args[10], args[11]);
        }

        private XmlNode CallMarginExtensionAction(SerializedObject request, Token token)
        {
            List<string> args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.CallMarginExtension(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
        }

        private XmlNode FundTransferAction(SerializedObject request, Token token)
        {
            List<string> args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.FundTransfer(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
        }

        private XmlNode PaymentInstructionAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.PaymentInstruction(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10],
                args[11], args[12], args[13], args[14]);
        }

        private XmlNode PaymentInstructionInternalAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.PaymentInstructionInternal(request.Session, args[0], args[1],
                args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);

        }

        private XmlNode PaymentInstructionCashAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.PaymentInstructionCash(request.Session, args[0], args[1], args[2],
                args[3], args[4], args[5], args[6], args[7], args[8]);

        }

        private XmlNode AssignAction(SerializedObject request, Token token)
        {
            return ClientService.Assign(request.Session);
        }

        private XmlNode ChangeLeverageAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return ClientService.ChangeLeverage(request.Session, Guid.Parse(args[0]), int.Parse(args[1]));

        }

        private XmlNode AsyncGetChartData2Action(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TickService.AsyncGetChartData2(request.Session, Guid.Parse(args[0]), DateTime.Parse(args[1]), DateTime.Parse(args[2]), args[3]);

        }

        private XmlNode GetChartDataAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TickService.GetChartData(Guid.Parse(args[0]));
        }

        private XmlNode VerifyTransactionAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            return TransactionService.VerifyTransaction(request.Session, args[0].ToGuidArray());

        }

        private XmlNode PlaceAction(SerializedObject request, Token token)
        {
            var args = XmlRequestCommandHelper.GetArguments(request.Content);
            if (token != null && token.AppType == iExchange.Common.AppType.Mobile)
            {
                ICollection<Mobile.Server.Transaction> transactions = Mobile.Manager.ConvertPlacingRequest(token, args[0].ToXmlNode());
                XElement element = new XElement("Result");
                foreach (Mobile.Server.Transaction transaction in transactions)
                {
                    //XmlNode TransactionService.Place(request.Session, args[0].ToXmlNode());
                    string tranCode;
                    TransactionError error=Application.Default.TradingConsoleServer.Place(token, Application.Default.StateServer, transaction.ToXmlNode(), out tranCode);
                    if (error == TransactionError.Action_ShouldAutoFill)
                    {
                        error = TransactionError.OK;
                    }

                    XElement errorElement = new XElement("ErrorCode");
                    errorElement.Value=error.ToString();
                    element.Add(errorElement);
                }
                return element.ToXmlNode();
            }
            else
            {
                return TransactionService.Place(request.Session, args[0].ToXmlNode());
            }

        }

        private XmlNode KeepAliveAction(SerializedObject request, Token token)
        {
            return ProcessAliveKeepRequest();

        }
        private  XmlNode ProcessAliveKeepRequest()
        {
            string content = "1";
            return XmlResultHelper.NewResult(content);
        }


        private XmlNode RecoverAction(SerializedObject request, Token token)
        {
            return RecoverService.Recover(request.Session, request.CurrentSession);
        }

        private XmlNode LogoutAction(SerializedObject request, Token token)
        {
           return LoginManager.Default.Logout(request.Session);

        }

    }
}
