using System;
using System.Linq;
using System.Text;
using log4net;
using StockChart.Common;
using Easychart.Finance;
using iExchange.Common;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Collections;
using System.Net;
using System.Xml;
using Framework.Time;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Trader.Server.Service;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using Trader.Common;
using System.Xml.Linq;
using Trader.Server.SessionNamespace;
namespace Trader.Server.Bll
{
    public class Service
    {
        private ILog _Logger = LogManager.GetLogger(typeof (Service));
        public Service()
        {
          
        }

        public void Complain(Session  session,string loginName, string complaint)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                string path = ""; // Path.Combine(this.Context.Server.MapPath("~"), "Complaint");
                path = Path.Combine(path, loginName);
                path = Path.Combine(path, token.UserID.ToString());
                string fileName = Path.Combine(path, DateTime.Now.ToString("yyyyMMddHHmmSS") + ".txt");

                if (!File.Exists(path)) Directory.CreateDirectory(path);
                if (File.Exists(fileName)) File.Delete(fileName);

                using (StreamWriter streamWriter = File.CreateText(fileName))
                {
                    streamWriter.WriteLine(token.ToString());
                    string[] info = complaint.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (string item in info)
                    {
                        streamWriter.WriteLine(item);
                    }
                }
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
            }
        }

     

        //Use in TickByTick Chart
        public DataSet GetTickByTickHistoryDatas(Session  session,Guid instrumentId)
        {
            try
            {
                TradingConsoleState state = SessionManager.Default.GetTradingConsoleState(session);
                if (state.Instruments.ContainsKey(instrumentId))
                {
                    Guid quotePolicyId = (Guid)state.Instruments[instrumentId];
                    DataSet dataSet = Application.Default.TradingConsoleServer.GetTickByTickHistoryDatas(instrumentId, quotePolicyId);
                    return dataSet;
                }
                _Logger.Warn(string.Format("Instrument {0} doesn't exists in TradingConsoleState", instrumentId));
                return null;
            }
            catch (Exception e)
            {
                _Logger.Error(e);
                return null;
            }
        }



        public Guid AsyncGetTickByTickHistoryData(Session session, Guid instrumentId)
        {
            try
            {
                AsyncResult asyncResult = new AsyncResult("AsyncGetTickByTickHistoryData", session.ToString());
                if (ThreadPool.QueueUserWorkItem(this.CreateTickByTickHistoryDatas, new TickByTickHistoryDataArgument(instrumentId, asyncResult, session)))
                {
                    return asyncResult.Id;
                }
                _Logger.Warn("ThreadPool.QueueUserWorkItem failed");
                return Guid.Empty;
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return Guid.Empty;
            }

        }

        public Guid AsyncGetTickByTickHistoryData2(Session session,Guid instrumentId, DateTime from, DateTime to)
        {
            try
            {
                _Logger.Info(string.Format("{0}-{1}", from, to));
                AsyncResult asyncResult = new AsyncResult("AsyncGetTickByTickHistoryData2", session.ToString());
                if (ThreadPool.QueueUserWorkItem(this.CreateTickByTickHistoryDatas2, new TickByTickHistoryDataArgument2(instrumentId, from, to, asyncResult, session)))
                {
                    return asyncResult.Id;
                }
                _Logger.Warn("ThreadPool.QueueUserWorkItem failed");
                return Guid.Empty;
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return Guid.Empty;
            }
        }

      

        public void SaveIsCalculateFloat(Session  session,bool isCalculateFloat)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                Application.Default.TradingConsoleServer.SaveIsCalculateFloat(token, isCalculateFloat);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
            }
        }

        public XElement OrderQuery(Session  session,Guid customerId, string accountId, string instrumentId, int lastDays)
        {
            try
            {
                string language = SessionManager.Default.GetToken(session).Language;
                var ds=Application.Default.TradingConsoleServer.OrderQuery(language, customerId, accountId, instrumentId, lastDays);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

    

        public string[] Get99BillBanks(Session  session)
        {
            try
            {
                string language = SessionManager.Default.GetTradingConsoleState(session).Language;
                return Application.Default.TradingConsoleServer.Get99BillBanks(language);
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

        public long GetNextOrderNoFor99Bill(string merchantAcctId)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetNextOrderNoFor99Bill(merchantAcctId);
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                throw;
            }
        }
   

        public DataSet GetQuotePolicyDetailsAndRefreshInstrumentsState2(Session  session,Guid customerID)
        {
            try
            {
                return InternalGetQuotePolicyDetailsAndRefreshInstrumentsState(session,customerID);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

        public XElement GetQuotePolicyDetailsAndRefreshInstrumentsState(Session session, Guid customerID)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                var ds=InternalGetQuotePolicyDetailsAndRefreshInstrumentsState(session,token.UserID);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        private DataSet InternalGetQuotePolicyDetailsAndRefreshInstrumentsState(Session session,Guid customerID)
        {
            DataSet dataSet = Application.Default.TradingConsoleServer.GetQuotePolicyDetails(customerID);
            TradingConsoleState state = SessionManager.Default.GetTradingConsoleState(session);
            Application.Default.TradingConsoleServer.RefreshInstrumentsState2(dataSet, ref state, session.ToString());
            if (state == null) return dataSet;
            TraderState traderState = new TraderState(state);
            traderState.CaculateQuotationFilterSign();
            SessionManager.Default.AddTradingConsoleState(session, traderState);
            return dataSet;
        }

     

        public void LogResultOfGetCommands(Guid userId, bool resultOfGetCommand, int firstSequence, int lastSequence)
        {

            _Logger.Info(string.Format("{0}, {1}, firstSequence = {2}, lastSequence = {3}", userId,
                resultOfGetCommand ? "GetCommands" : "GetCommands2", firstSequence, lastSequence));
        }

        public XElement Quote(Session session,string instrumentID, double quoteLot, int bsStatus)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state != null && state.IsEmployee;
                Application.Default.TradingConsoleServer.Quote(token, isEmployee, Application.Default.StateServer, GetLocalIP(), instrumentID, quoteLot, bsStatus);
                return XmlResultHelper.NewResult(StringConstants.OkResult);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public XElement Quote2(Session session, string instrumentID, double buyQuoteLot, double sellQuoteLot, int tick)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state != null && state.IsEmployee;
                Application.Default.TradingConsoleServer.Quote2(token, isEmployee, Application.Default.StateServer, GetLocalIP(), instrumentID, buyQuoteLot, sellQuoteLot, tick);
                return XmlResultHelper.NewResult(StringConstants.OkResult);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public void CancelQuote(Session session,string instrumentID, double buyQuoteLot, double sellQuoteLot)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                Application.Default.TradingConsoleServer.CancelQuote(token, Application.Default.StateServer, GetLocalIP(), instrumentID, buyQuoteLot, sellQuoteLot);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
            }
        }

        public XElement  CancelLmtOrder(Session session,string transactionID)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                var result = Application.Default.TradingConsoleServer.CancelLMTOrder(token, Application.Default.StateServer, transactionID);
                return XmlResultHelper.NewResult(result.ToString());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public bool UpdateAccountLock(Session session,string agentAccountID, string[][] arrayAccountLock)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session); 
                return Application.Default.TradingConsoleServer.UpdateAccountLock(token, Application.Default.StateServer, agentAccountID, arrayAccountLock);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return false;
            }

        }

        public string GetSystemTime()
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetSystemTime().ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                throw;
            }
        }

        public TimeSyncSettings GetTimeSyncSettings()
        {
            try
            {
                var timeSyncSection = (TimeSyncSection)ConfigurationManager.GetSection("TimeSync");
                var timeSyncSettings = new TimeSyncSettings(timeSyncSection.SyncInterval, timeSyncSection.MinAdjustedTimeOfSyncSoon);
                return timeSyncSettings;
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                throw;
            }
        }

        public DataSet GetCurrencyRateByAccountID(string accountID)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetCurrencyRateByAccountID(accountID);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

   

        public DataSet GetInstruments(Session session,ArrayList instrumentIDs)
        {
            try
            {
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                Token token = SessionManager.Default.GetToken(session);
                DataSet dataSet =Application.Default.TradingConsoleServer.GetInstruments(token, instrumentIDs, state);
                return dataSet;
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }

        }


        public XElement DeleteMessage(Session session, Guid id)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                bool result= Application.Default.TradingConsoleServer.DeleteMessage(token, id);
                return XmlResultHelper.NewResult(result.ToPlainBitString());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public void GetCustomerInfo(out string customerCode, out string customerName)
        {
            customerCode = string.Empty;
            customerName = string.Empty;
        }



        public DataSet GetDealingPolicyDetails(Session session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                using (var sqlConnection = new SqlConnection(SettingManager.Default.ConnectionString))
                {
                    using (var sqlCommand = new SqlCommand("P_GetDealingPolicyDetails", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlConnection.Open();
                        SqlCommandBuilder.DeriveParameters(sqlCommand);
                        sqlCommand.Parameters["@userId"].Value = token.UserID;
                        var dataAdapter = new SqlDataAdapter(sqlCommand);
                        var dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }


        public void NotifyCustomerExecuteOrderForJava(Session session, string[][] arrayNotifyCustomerExecuteOrder, string companyCode, string version)
        {
            try
            {

                Token token = SessionManager.Default.GetToken(session);
                string physicalPath = SettingManager.Default.PhysicPath+"\\" +  companyCode + "\\" + version + "\\";
                Application.Default.TradingConsoleServer.NotifyCustomerExecuteOrder(token, Application.Default.StateServer, physicalPath, arrayNotifyCustomerExecuteOrder);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
            }
        }

        public byte[] GetLogoForJava(string companyCode)//,string fileName)
        {
            try
            {
                string filePath = SettingManager.Default.PhysicPath+"\\" +  companyCode + "\\iExchange.gif";// +fileName;
                return File.ReadAllBytes(filePath);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

        public XmlNode GetColorSettingsForJava(string companyCode)
        {
            try
            {
                string xmlPath = SettingManager.Default.PhysicPath+"\\" +  companyCode + "\\ColorSettingsForJava.xml";
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                var node = doc.GetElementsByTagName("ColorSettings")[0];
                return node;
            }
            catch (System.Exception ex)
            {
                _Logger.Error(ex);
                return null;
            }
        }

        //no use
        public XmlNode GetParameterForJava(Session  session,string companyCode, string version)
        {
           
            //Get xml
            try
            {
                string physicalPath = SettingManager.Default.PhysicPath + "\\" + companyCode + "\\" + version + "\\";
                string xmlPath = physicalPath + "Setting\\Parameter.xml";
                var doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);
                xmlPath = physicalPath + "Setting\\Login.xml";
                var doc2 = new System.Xml.XmlDocument();
                doc2.Load(xmlPath);
                var node2 = doc2.GetElementsByTagName("Login")[0];

                var parameterXmlNode = doc.GetElementsByTagName("Parameter")[0];

                string newsLanguage = node2.SelectNodes("NewsLanguage").Item(0).InnerXml;
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                if (state == null)
                {
                    state = new TraderState(session.ToString());
                }
                state.Language = newsLanguage.ToLower();
                SessionManager.Default.AddTradingConsoleState(session, state);
                XmlElement newChild = doc.CreateElement("NewsLanguage");
                newChild.InnerText = newsLanguage;
                parameterXmlNode.AppendChild(newChild);
                var node = doc.GetElementsByTagName("Parameters")[0];
                return node;
            }
            catch (System.Exception ex)
            {
                _Logger.Error(ex);
                return null;
            }

        }


      public byte[] GetTracePropertiesForJava()
        {
            try
            {
                return File.ReadAllBytes( SettingManager.Default.PhysicPath+"\\" + "TradingConsole.Trace.Properties");
            }
            catch (System.Exception ex)
            {
                _Logger.Error(ex);
                return null;
            }

        }


        public bool UpdateSystemParameters(Session session, string parameters, string objectID)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                return Application.Default.TradingConsoleServer.UpdateSystemParameters(token, parameters, objectID);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return false;
            }
        }

        public DataSet GetNewsList(string newsCategoryID, string language, DateTime date)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetNewsList(newsCategoryID, language, date);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

        public XElement GetNewsContents(Session session, string newsID)
        {
            try
            {
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                var ds=Application.Default.TradingConsoleServer.GetNewsContents(newsID, state.Language);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public DataSet GetInterestRate(Guid[] orderIds)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetInterestRate(orderIds);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

        public DataSet GetInterestRate2(Session session,Guid interestRateId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                return Application.Default.TradingConsoleServer.GetInterestRate2(token, interestRateId);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return null;
            }
        }

        private string GetLocalIP()
        {
            return string.Empty;
        }

        public Decimal RefreshAgentAccountOrder(string orderID)
        {
            try
            {
                return Application.Default.TradingConsoleServer.RefreshAgentAccountOrder(orderID);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                throw;
            }
        }


        public XmlDocument GetOuterNews(string newsUrl)
        {
            var doc = new XmlDocument();
            doc.Load(newsUrl);
            return doc;
        }

        public void SaveLog(Session session,string logCode, DateTime timestamp, string action)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state != null && state.IsEmployee;
                Application.Default.TradingConsoleServer.SaveLog(token, isEmployee, GetLocalIP(), logCode, timestamp, action);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
            }
        }



        public void SaveLogForWeb(Session session, string logCode, string action, string transactionId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state != null && state.IsEmployee;
                Application.Default.TradingConsoleServer.SaveLog(token, isEmployee, GetLocalIP(), logCode, DateTime.Now, action, new Guid(transactionId));
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
            }
        }

        public XElement  Apply(Session session,Guid id, string accountBankApprovedId, string accountId, string countryId, string bankId, string bankName,
            string accountBankNo, string accountBankType,//#00;银行卡|#01;存折
            string accountOpener, string accountBankProp, Guid accountBankBCId, string accountBankBCName,
            string idType,//#0;身份证|#1;户口簿|#2;护照|#3;军官证|#4;士兵证|#5;港澳居民来往内地通行证|#6;台湾同胞来往内地通行证|#7;临时身份证|#8;外国人居留证|#9;警官证|#x;其他证件
            string idNo, string bankProvinceId, string bankCityId, string bankAddress, string swiftCode, int applicationType)
        {
            try
            {
                string connectionString = SettingManager.Default.ConnectionString;
                Token token = SessionManager.Default.GetToken(session);
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    using (var sqlCommand = new SqlCommand("dbo.P_ApplyAccountBank", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlConnection.Open();
                        SqlCommandBuilder.DeriveParameters(sqlCommand);
                        sqlCommand.Parameters["@id"].Value = id;
                        if (accountId != null) sqlCommand.Parameters["@accountId"].Value = new Guid(accountId);
                        if (!string.IsNullOrEmpty(accountBankApprovedId))
                        {
                            sqlCommand.Parameters["@accountBankApprovedId"].Value = new Guid(accountBankApprovedId);
                        }
                        if (!string.IsNullOrEmpty(bankId))
                        {
                            sqlCommand.Parameters["@bankId"].Value = new Guid(bankId);
                        }
                        if (!string.IsNullOrEmpty(countryId))
                        {
                            sqlCommand.Parameters["@countryId"].Value = long.Parse(countryId);
                        }
                        sqlCommand.Parameters["@bankName"].Value = bankName;
                        sqlCommand.Parameters["@accountBankNo"].Value = accountBankNo;
                        sqlCommand.Parameters["@accountBankType"].Value = accountBankType;
                        sqlCommand.Parameters["@accountOpener"].Value = accountOpener;
                        sqlCommand.Parameters["@accountBankProp"].Value = accountBankProp;
                        sqlCommand.Parameters["@accountBankBCId"].Value = accountBankBCId;
                        sqlCommand.Parameters["@accountBankBCName"].Value = accountBankBCName;
                        sqlCommand.Parameters["@idType"].Value = idType;
                        if (!string.IsNullOrEmpty(bankProvinceId)) sqlCommand.Parameters["@bankProvinceId"].Value = long.Parse(bankProvinceId);
                        sqlCommand.Parameters["@idNo"].Value = idNo;
                        if (!string.IsNullOrEmpty(bankCityId)) sqlCommand.Parameters["@bankCityId"].Value = long.Parse(bankCityId);
                        sqlCommand.Parameters["@bankAddress"].Value = bankAddress;
                        sqlCommand.Parameters["@swiftCode"].Value = swiftCode;
                        sqlCommand.Parameters["@applicationType"].Value = applicationType;
                        sqlCommand.Parameters["@updatePersonId"].Value = token.UserID;

                        sqlCommand.ExecuteNonQuery();

                        var result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                        if (result != 0)
                        {
                            return XmlResultHelper.ErrorResult;
                        }
                        var approved = (bool)sqlCommand.Parameters["@approved"].Value;
                        return XmlResultHelper.NewResult(approved.ToPlainBitString());
                    }
                }
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }


        private void CreateTickByTickHistoryDatas(object state)
        {
            var tickByTickHistoryDataArgument = (TickByTickHistoryDataArgument)state;
            try
            {
                TradingConsoleState tradingConsoleState = tickByTickHistoryDataArgument.TradingConsoleState;
                if (tradingConsoleState.Instruments.ContainsKey(tickByTickHistoryDataArgument.InstrumentId))
                {
                    Guid quotePolicyId = (Guid)tradingConsoleState.Instruments[tickByTickHistoryDataArgument.InstrumentId];
                    TradingConsoleServer tradingConsoleServer = tickByTickHistoryDataArgument.TradingConsoleServer;
                    DataSet dataSet = tradingConsoleServer.GetTickByTickHistoryDatas(tickByTickHistoryDataArgument.InstrumentId, quotePolicyId);
                    AsyncResultManager asyncResultManager = tickByTickHistoryDataArgument.AsyncResultManager;
                    asyncResultManager.SetResult(tickByTickHistoryDataArgument.AsyncResult, dataSet);
                    CommandManager.Default.AddCommand( new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult));
                }
                else
                {
                    _Logger.Warn(string.Format("Instrument {0} doesn't exists in TradingConsoleState", tickByTickHistoryDataArgument.InstrumentId));
                    CommandManager.Default.AddCommand(new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, null));
                }
            }
            catch (Exception e)
            {
                _Logger.Error(e);
                CommandManager.Default.AddCommand(new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, e));
            }
        }

        private void CreateTickByTickHistoryDatas2(object state)
        {
            var tickByTickHistoryDataArgument = (TickByTickHistoryDataArgument2)state;
            try
            {
                TradingConsoleState tradingConsoleState = tickByTickHistoryDataArgument.TradingConsoleState;
                if (tradingConsoleState.Instruments.ContainsKey(tickByTickHistoryDataArgument.InstrumentId))
                {
                    Guid quotePolicyId = (Guid)tradingConsoleState.Instruments[tickByTickHistoryDataArgument.InstrumentId];
                    TradingConsoleServer tradingConsoleServer = tickByTickHistoryDataArgument.TradingConsoleServer;
                    DataSet dataSet = tradingConsoleServer.GetTickByTickHistoryDatas2(tickByTickHistoryDataArgument.InstrumentId, quotePolicyId, tickByTickHistoryDataArgument.From, tickByTickHistoryDataArgument.To);
                    AsyncResultManager asyncResultManager = tickByTickHistoryDataArgument.AsyncResultManager;
                    asyncResultManager.SetResult(tickByTickHistoryDataArgument.AsyncResult, dataSet);
                    CommandManager.Default.AddCommand( new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult));
                }
                else
                {
                    _Logger.Warn(string.Format("Instrument {0} doesn't exists in TradingConsoleState",
                        tickByTickHistoryDataArgument.InstrumentId));
                    CommandManager.Default.AddCommand( new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, null));
                }
            }
            catch (Exception e)
            {
                _Logger.Error(e);
                CommandManager.Default.AddCommand(new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, e));
            }
        }
    }
}
