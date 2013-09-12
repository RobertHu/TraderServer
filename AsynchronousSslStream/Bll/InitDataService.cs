using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using Trader.Server.SessionNamespace;
using Trader.Server.Bll;
using System.Diagnostics;
using System.Xml;
using System.Configuration;
using System.Xml.Linq;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using Trader.Server._4BitCompress;
using Trader.Server.Service;
using Wintellect.Threading.AsyncProgModel;
using log4net;
using Trader.Server.Serialization;
using Trader.Common;
namespace Trader.Server.Bll
{
    public class InitDataService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(InitDataService));
        public static IEnumerator<int> GetInitData(SerializedObject request, DataSet initData, AsyncEnumerator ae)
        {
            Session session = request.Session;
            Token token = SessionManager.Default.GetToken(session);
            if (initData == null)
            {
                Application.Default.StateServer.BeginGetInitData(token, null, ae.End(), null);
                yield return 1;
                int sequence;
                try
                {
                    initData = Application.Default.StateServer.EndGetInitData(ae.DequeueAsyncResult(), out sequence);
                }
                catch (Exception ex)
                {
                    SendErrorResult(request, ex);
                    yield break;
                }
            }
            try
            {
                DataSet ds = Init(session, initData);
                request.ContentInPointer = ds.ToPointer();
                SendCenter.Default.Send(request);
            }
            catch (Exception ex)
            {
                SendErrorResult(request,ex);
            }
        }

        private static void SendErrorResult(SerializedObject request,Exception ex)
        {
            _Logger.Error(ex);
            request.Content = XmlResultHelper.ErrorResult;
            SendCenter.Default.Send(request);
        }


        public static DataSet Init(Session session, DataSet initData)
        {
            DataRowCollection rows;
            TraderState state = SessionManager.Default.GetTradingConsoleState(session);
            //Customer
            //rows=initData.Tables["Customer"].Rows;
            //Instrument

            StringBuilder quotePolicyInfo = new StringBuilder();
            quotePolicyInfo.Append("SessionId = " + state.SessionId + "\t");
            rows = initData.Tables["Instrument"].Rows;
            List<Guid> instrumentsFromBursa = new List<Guid>();
            foreach (DataRow instrumentRow in rows)
            {
                state.AddInstrumentIDToQuotePolicyMapping((Guid)instrumentRow["ID"], (Guid)instrumentRow["QuotePolicyID"]);
                if (quotePolicyInfo.Length > 0) quotePolicyInfo.Append(";");
                quotePolicyInfo.Append(instrumentRow["ID"]);
                quotePolicyInfo.Append("=");
                quotePolicyInfo.Append(instrumentRow["QuotePolicyID"]);
                if (IsFromBursa(instrumentRow))
                {
                    instrumentsFromBursa.Add((Guid)instrumentRow["ID"]);
                }
            }
            //Account			
            rows = initData.Tables["Account"].Rows;
            Guid[] accountIDs = new Guid[rows.Count];
            int i = 0;
            foreach (DataRow accountRow in rows)
            {
                if (!state.Accounts.ContainsKey(accountRow["ID"]))
                {
                    state.Accounts.Add(accountRow["ID"], null);
                    state.AccountGroups[accountRow["GroupID"]] = null;
                }
                accountIDs[i++] = (Guid)accountRow["ID"];
            }
            SessionManager.Default.AddTradingConsoleState(session, state);
            int commandSequence = CommandManager.Default.LastSequence;
            SessionManager.Default.AddNextSequence(session, commandSequence);
            DataTable customerTable = initData.Tables["Customer"];
            state.IsEmployee = (bool)customerTable.Rows[0]["IsEmployee"];
            Token token = SessionManager.Default.GetToken(session);
            DataSet ds = Merge(token, initData, accountIDs);
            bool supportBursa = Convert.ToBoolean(ConfigurationManager.AppSettings["SupportBursa"]);
            if (supportBursa)
            {
                DateTime tradeDay = DateTime.Now.Date;
                AddDefaultTimeTableForBursa(ds, tradeDay);
            }
            AddBestLimitsForBursa(ds, instrumentsFromBursa);
            ds.SetInstrumentGuidMapping();
            ds.SetCommandSequence(commandSequence);
            state.CaculateQuotationFilterSign();
            return ds;
        }


        private static bool IsFromBursa(DataRow instrumentRow)
        {
            string externalExchangeCode = instrumentRow["ExternalExchangeCode"] == DBNull.Value ? null : (string)instrumentRow["ExternalExchangeCode"];
            if (!string.IsNullOrEmpty(externalExchangeCode) && externalExchangeCode == "Bursa")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private static DataSet Merge(Token token, DataSet initData, Guid[] accountIDs)
        {
            if (initData == null) return initData;

            XmlNode accountsData = Application.Default.StateServer.GetAccountsForInit(accountIDs);

            try
            {
                DataTable accountTable = initData.Tables["Account"];
                accountTable.Columns.Add("Balance", typeof(decimal));
                accountTable.Columns.Add("ValueAsMargin", typeof(decimal));
                accountTable.Columns.Add("FrozenFund", typeof(decimal));
                accountTable.Columns.Add("Necessary", typeof(decimal));
                accountTable.Columns.Add("InterestPLNotValued", typeof(decimal));
                accountTable.Columns.Add("StoragePLNotValued", typeof(decimal));
                accountTable.Columns.Add("TradePLNotValued", typeof(decimal));
                accountTable.Columns.Add("InterestPLFloat", typeof(decimal));
                accountTable.Columns.Add("StoragePLFloat", typeof(decimal));
                accountTable.Columns.Add("TradePLFloat", typeof(decimal));
                accountTable.Columns.Add("AlertLevel", typeof(System.Int32));
                accountTable.PrimaryKey = new DataColumn[] { accountTable.Columns["ID"] };
                DataRowCollection accountRows = accountTable.Rows;

                DataTable accountCurrencyTable = initData.Tables["AccountCurrency"];
                accountCurrencyTable.Columns.Add("Balance", typeof(decimal));
                accountCurrencyTable.Columns.Add("Necessary", typeof(decimal));
                accountCurrencyTable.Columns.Add("ValueAsMargin", typeof(decimal));
                accountCurrencyTable.Columns.Add("FrozenFund", typeof(decimal));
                accountCurrencyTable.Columns.Add("InterestPLNotValued", typeof(decimal));
                accountCurrencyTable.Columns.Add("StoragePLNotValued", typeof(decimal));
                accountCurrencyTable.Columns.Add("TradePLNotValued", typeof(decimal));
                accountCurrencyTable.Columns.Add("InterestPLFloat", typeof(decimal));
                accountCurrencyTable.Columns.Add("StoragePLFloat", typeof(decimal));
                accountCurrencyTable.Columns.Add("TradePLFloat", typeof(decimal));
                accountCurrencyTable.PrimaryKey = new DataColumn[] { accountCurrencyTable.Columns["AccountID"], accountCurrencyTable.Columns["CurrencyID"] };
                DataRowCollection accountCurrencyRows = accountCurrencyTable.Rows;

                DataTable orderTable = initData.Tables["Order"];
                orderTable.Columns.Add("InterestPLFloat", typeof(decimal));
                orderTable.Columns.Add("StoragePLFloat", typeof(decimal));
                orderTable.Columns.Add("TradePLFloat", typeof(decimal));
                orderTable.Columns.Add("Necessary", typeof(decimal));
                orderTable.Columns.Add("ValueAsMargin", typeof(decimal));
                orderTable.Columns.Add("LivePrice", typeof(string));
                orderTable.Columns.Add("AutoLimitPriceString", typeof(string));
                orderTable.Columns.Add("AutoStopPriceString", typeof(string));
                orderTable.PrimaryKey = new DataColumn[] { orderTable.Columns["ID"] };
                DataRowCollection orderRows = orderTable.Rows;

                if (accountsData != null)
                {
                    foreach (XmlElement account in accountsData.ChildNodes)
                    {
                        Guid accountID = XmlConvert.ToGuid(account.Attributes["ID"].Value);
                        DataRow accountRow = accountRows.Find(accountID);
                        accountRow["Balance"] = XmlConvert.ToDecimal(account.Attributes["Balance"].Value);
                        accountRow["Necessary"] = XmlConvert.ToDecimal(account.Attributes["Necessary"].Value);
                        accountRow["FrozenFund"] = XmlConvert.ToDecimal(account.Attributes["FrozenFund"].Value);
                        accountRow["ValueAsMargin"] = XmlConvert.ToDecimal(account.Attributes["ValueAsMargin"].Value);
                        accountRow["InterestPLNotValued"] = XmlConvert.ToDecimal(account.Attributes["InterestPLNotValued"].Value);
                        accountRow["StoragePLNotValued"] = XmlConvert.ToDecimal(account.Attributes["StoragePLNotValued"].Value);
                        accountRow["TradePLNotValued"] = XmlConvert.ToDecimal(account.Attributes["TradePLNotValued"].Value);
                        accountRow["InterestPLFloat"] = XmlConvert.ToDecimal(account.Attributes["InterestPLFloat"].Value);
                        accountRow["StoragePLFloat"] = XmlConvert.ToDecimal(account.Attributes["StoragePLFloat"].Value);
                        accountRow["TradePLFloat"] = XmlConvert.ToDecimal(account.Attributes["TradePLFloat"].Value);
                        accountRow["AlertLevel"] = XmlConvert.ToInt32(account.Attributes["AlertLevel"].Value);

                        foreach (XmlElement xmlChild in account.ChildNodes)
                        {
                            if (xmlChild.Name == "Currency")
                            {
                                XmlElement accountCurrency = xmlChild;

                                Guid currencyID = XmlConvert.ToGuid(accountCurrency.Attributes["ID"].Value);
                                DataRow accountCurrencyRow = accountCurrencyRows.Find(new object[] { accountID, currencyID });
                                if (accountCurrencyRow == null)
                                {
                                    accountCurrencyRow = initData.Tables["AccountCurrency"].NewRow();
                                    accountCurrencyRow["AccountID"] = accountID;
                                    accountCurrencyRow["CurrencyID"] = currencyID;
                                    accountCurrencyRows.Add(accountCurrencyRow);
                                }
                                accountCurrencyRow["Balance"] = XmlConvert.ToDecimal(accountCurrency.Attributes["Balance"].Value);
                                accountCurrencyRow["FrozenFund"] = XmlConvert.ToDecimal(accountCurrency.Attributes["FrozenFund"].Value);
                                accountCurrencyRow["ValueAsMargin"] = XmlConvert.ToDecimal(accountCurrency.Attributes["ValueAsMargin"].Value);
                                accountCurrencyRow["Necessary"] = XmlConvert.ToDecimal(accountCurrency.Attributes["Necessary"].Value);
                                accountCurrencyRow["InterestPLNotValued"] = XmlConvert.ToDecimal(accountCurrency.Attributes["InterestPLNotValued"].Value);
                                accountCurrencyRow["StoragePLNotValued"] = XmlConvert.ToDecimal(accountCurrency.Attributes["StoragePLNotValued"].Value);
                                accountCurrencyRow["TradePLNotValued"] = XmlConvert.ToDecimal(accountCurrency.Attributes["TradePLNotValued"].Value);
                                accountCurrencyRow["InterestPLFloat"] = XmlConvert.ToDecimal(accountCurrency.Attributes["InterestPLFloat"].Value);
                                accountCurrencyRow["StoragePLFloat"] = XmlConvert.ToDecimal(accountCurrency.Attributes["StoragePLFloat"].Value);
                                accountCurrencyRow["TradePLFloat"] = XmlConvert.ToDecimal(accountCurrency.Attributes["TradePLFloat"].Value);
                            }
                            else if (xmlChild.Name == "Orders")
                            {
                                foreach (XmlElement order in xmlChild.ChildNodes)
                                {
                                    Guid orderID = XmlConvert.ToGuid(order.Attributes["ID"].Value);
                                    DataRow orderRow = orderRows.Find(new object[] { orderID });
                                    if (orderRow != null)
                                    {
                                        orderRow["InterestPLFloat"] = XmlConvert.ToDecimal(order.Attributes["InterestPLFloat"].Value);
                                        orderRow["StoragePLFloat"] = XmlConvert.ToDecimal(order.Attributes["StoragePLFloat"].Value);
                                        orderRow["TradePLFloat"] = XmlConvert.ToDecimal(order.Attributes["TradePLFloat"].Value);
                                        orderRow["Necessary"] = XmlConvert.ToDecimal(order.Attributes["Necessary"].Value);
                                        orderRow["ValueAsMargin"] = XmlConvert.ToDecimal(order.Attributes["ValueAsMargin"].Value);
                                        orderRow["LivePrice"] = order.Attributes["LivePrice"].Value;
                                        orderRow["AutoLimitPriceString"] = order.Attributes["AutoLimitPrice"].Value;
                                        orderRow["AutoStopPriceString"] = order.Attributes["AutoStopPrice"].Value;
                                    }
                                }
                            }
                        }
                    }
                }

                return initData;
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Merge", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw ex;
            }
        }


        private static void AddDefaultTimeTableForBursa(DataSet ds, DateTime tradeDay)
        {
            DataTable defultTimeTable = new DataTable("BursaDefaultTimeTable");
            defultTimeTable.Columns.Add("PreOpen", typeof(DateTime));
            defultTimeTable.Columns.Add("Open", typeof(DateTime));
            defultTimeTable.Columns.Add("PreClose", typeof(DateTime));
            defultTimeTable.Columns.Add("TradeAtLast", typeof(DateTime));
            defultTimeTable.Columns.Add("End", typeof(DateTime));

            TimeSpan bursaTimeZoneOffset = TimeSpan.FromHours(7);
            TimeSpan localTimeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            DataRow forenoonRow = defultTimeTable.NewRow();
            forenoonRow["PreOpen"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(8, 30, 0));
            forenoonRow["Open"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(9, 0, 0));
            forenoonRow["PreClose"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(12, 15, 0));
            forenoonRow["TradeAtLast"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(12, 20, 0));
            forenoonRow["End"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(12, 30, 0));

            DataRow afternoonRow = defultTimeTable.NewRow();
            afternoonRow["PreOpen"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(14, 0, 0));
            afternoonRow["Open"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(14, 30, 0));
            afternoonRow["PreClose"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(16, 45, 0));
            afternoonRow["TradeAtLast"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(16, 50, 0));
            afternoonRow["End"] = tradeDay.Add(bursaTimeZoneOffset - localTimeZoneOffset + new TimeSpan(17, 0, 0));

            defultTimeTable.Rows.Add(forenoonRow);
            defultTimeTable.Rows.Add(afternoonRow);

            defultTimeTable.AcceptChanges();
            ds.Tables.Add(defultTimeTable);
        }


        private static void AddBestLimitsForBursa(DataSet initData, List<Guid> instrumentsFromBursa)
        {
            if (instrumentsFromBursa.Count > 0)
            {
                List<MatchInfoCommand> matchInfos = Application.Default.MarketDepthManager.GetCommands(instrumentsFromBursa);
                if (matchInfos.Count > 0)
                {
                    DateTime now = DateTime.Now;

                    DataTable bestLimit = new DataTable("BestLimits");
                    bestLimit.Columns.Add(new DataColumn("InstrumentId", typeof(Guid)));
                    bestLimit.Columns.Add(new DataColumn("Sequence", typeof(short)));
                    bestLimit.Columns.Add(new DataColumn("IsBuy", typeof(bool)));
                    bestLimit.Columns.Add(new DataColumn("Price", typeof(double)));
                    bestLimit.Columns.Add(new DataColumn("Quantity", typeof(double)));
                    bestLimit.Columns.Add(new DataColumn("Timestamp", typeof(DateTime)));

                    foreach (MatchInfoCommand matchInfo in matchInfos)
                    {
                        int index = 1;
                        foreach (PendingItem pendingItem in matchInfo.BestBuys)
                        {
                            DataRow dataRow = bestLimit.NewRow();
                            bestLimit.Rows.Add(dataRow);
                            dataRow["InstrumentId"] = matchInfo.InstrumentId;
                            dataRow["Sequence"] = index++;
                            dataRow["IsBuy"] = true;
                            dataRow["Price"] = double.Parse(pendingItem.Price);
                            dataRow["Quantity"] = (double)pendingItem.Quantity;
                            dataRow["Timestamp"] = now;
                        }

                        index = 1;
                        foreach (PendingItem pendingItem in matchInfo.BestSells)
                        {
                            DataRow dataRow = bestLimit.NewRow();
                            bestLimit.Rows.Add(dataRow);
                            dataRow["InstrumentId"] = matchInfo.InstrumentId;
                            dataRow["Sequence"] = index++;
                            dataRow["IsBuy"] = false;
                            dataRow["Price"] = double.Parse(pendingItem.Price);
                            dataRow["Quantity"] = (double)pendingItem.Quantity;
                            dataRow["Timestamp"] = now;
                        }
                    }

                    bestLimit.AcceptChanges();
                    initData.Tables.Add(bestLimit);
                }
            }
        }

    }
}