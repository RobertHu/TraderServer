using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockChart.Common;
using Easychart.Finance;
using iExchange.Common;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Collections;
using System.Net;
using System.Xml;
using Trader.Server.Session;
using Framework.Time;
using System.Configuration;
using Trader.Server.Setting;
using System.Data.SqlClient;
using Trader.Server.Report;
using System.IO;
using Trader.Server.Service;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using Trader.Common;
using System.Xml.Linq;
namespace Trader.Server.Bll
{
    public class Service 
    {
        private bool _DebugGetCommands = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["IsDebugGetCommands"]);
        private bool _RunAsTradingProxy = Convert.ToBoolean(ConfigurationSettings.AppSettings["RunAsTradingProxy"]);

     
       

     

        public Service()
        {
          

        }

       

        //private Hashtable InstrumentCodes
        //{
        //    get
        //    {
        //        if (this.Session["InstrumentCodes"] == null)
        //        {
        //            this.SetInstrumentCodes();
        //        }
        //        Hashtable instrumentCodes = (Hashtable)this.Session["InstrumentCodes"];
        //        if (instrumentCodes == null)
        //        {
        //            AppDebug.LogEvent("TradingConsole.Service.asmx.InstrumentCodes:", "Session['InstrumentCodes'] == null", EventLogEntryType.Error);
        //        }
        //        return instrumentCodes;
        //    }
        //}

        //Use in Chart
        private InstrumentInfo GetInstrumentInfoByID(Hashtable instrumentInfos, Guid instrumentId)
        {
            InstrumentInfo instrumentInfo = null;
            foreach (string instrumentInfoKey in instrumentInfos.Keys)
            {
                InstrumentInfo instrumentInfo2 = (InstrumentInfo)instrumentInfos[instrumentInfoKey];
                if (instrumentInfo2.Id.Equals(instrumentId))
                {
                    instrumentInfo = instrumentInfo2;
                    break;
                }
            }
            if (instrumentInfo == null)
            {
                AppDebug.LogEvent("TradingConsole.Service.asmx.GetInstrumentInfoByID:" + instrumentId.ToString(), "instrumentInfo == null", EventLogEntryType.Error);
            }
            return instrumentInfo;
        }

     

     



      

        public void Complain(Guid  session,string loginName, string complaint)
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
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Complain:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }


    

    

       

   



        private DataSet GetTickDatas(Guid  session,Guid instrumentId, DateTime dateTime, int minutes)
        {
            TradingConsoleState state = SessionManager.Default.GetTradingConsoleState(session);
            return this.GetTickDatas(instrumentId, dateTime, minutes, state, Application.Default.TradingConsoleServer);
        }

        private DataSet GetTickDatas(Guid instrumentId, DateTime dateTime, int minutes, TradingConsoleState state, TradingConsoleServer tradingConsoleServer)
        {
            try
            {
                if (state.Instruments.ContainsKey(instrumentId))
                {
                    Guid quotePolicyId = (Guid)state.Instruments[instrumentId];
                    DataSet dataSet = tradingConsoleServer.GetTickDatas(instrumentId, quotePolicyId, dateTime, minutes);
                    return dataSet;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.GetTickDatas", string.Format("Instrument {0} doesn't exists in TradingConsoleState", instrumentId), EventLogEntryType.Warning);
                    return null;
                }
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("TradingConsole.GetTickDatas", e.ToString(), EventLogEntryType.Error);
                return null;
            }
        }

     

        //Use in TickByTick Chart
        public DataSet GetTickByTickHistoryDatas(Guid  session,Guid instrumentId)
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
                else
                {
                    AppDebug.LogEvent("TradingConsole.GetTickByTickHistoryDatas", string.Format("Instrument {0} doesn't exists in TradingConsoleState", instrumentId), EventLogEntryType.Warning);
                    return null;
                }
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("TradingConsole.GetTickByTickHistoryDatas", e.ToString(), EventLogEntryType.Error);
                return null;
            }
        }



        public Guid AsyncGetTickByTickHistoryData(Guid session, Guid instrumentId)
        {
            try
            {
                AsyncResult asyncResult = new AsyncResult("AsyncGetTickByTickHistoryData", session.ToString());
                if (ThreadPool.QueueUserWorkItem(this.CreateTickByTickHistoryDatas, new TickByTickHistoryDataArgument(instrumentId, asyncResult, session)))
                {
                    return asyncResult.Id;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.AsyncGetTickByTickHistoryData:", "ThreadPool.QueueUserWorkItem failed", System.Diagnostics.EventLogEntryType.Warning);
                    return Guid.Empty;
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.AsyncGetTickByTickHistoryData:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return Guid.Empty;
        }

        public Guid AsyncGetTickByTickHistoryData2(Guid session,Guid instrumentId, DateTime from, DateTime to)
        {
            try
            {
                AppDebug.LogEvent("TradingConsole.AsyncGetTickByTickHistoryData2", string.Format("{0}-{1}", from, to), System.Diagnostics.EventLogEntryType.Information);

                AsyncResult asyncResult = new AsyncResult("AsyncGetTickByTickHistoryData2", session.ToString());
                if (ThreadPool.QueueUserWorkItem(this.CreateTickByTickHistoryDatas2, new TickByTickHistoryDataArgument2(instrumentId, from, to, asyncResult, session)))
                {
                    return asyncResult.Id;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.AsyncGetTickByTickHistoryData2:", "ThreadPool.QueueUserWorkItem failed", System.Diagnostics.EventLogEntryType.Warning);
                    return Guid.Empty;
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.AsyncGetTickByTickHistoryData2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return Guid.Empty;
        }

      


    

        public void SaveIsCalculateFloat(Guid  session,bool isCalculateFloat)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                Application.Default.TradingConsoleServer.SaveIsCalculateFloat(token, isCalculateFloat);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.SaveIsCalculateFloat:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public XElement OrderQuery(Guid  session,Guid customerId, string accountId, string instrumentId, int lastDays)
        {
            try
            {
                string language = SessionManager.Default.GetToken(session).Language;
                var ds=Application.Default.TradingConsoleServer.OrderQuery(language, customerId, accountId, instrumentId, lastDays);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.OrderQuery:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

    

       
     

      

      

        public string[] Get99BillBanks(Guid  session)
        {
            try
            {
                string language = SessionManager.Default.GetTradingConsoleState(session).Language;
                return Application.Default.TradingConsoleServer.Get99BillBanks(language);
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Get99BillBanks:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        public long GetNextOrderNoFor99Bill(string merchantAcctId)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetNextOrderNoFor99Bill(merchantAcctId);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetNextOrderNoFor99Bill:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw exception;
            }
        }

   

        public DataSet GetQuotePolicyDetailsAndRefreshInstrumentsState2(Guid  session,Guid customerID)
        {
            try
            {
                return this.InternalGetQuotePolicyDetailsAndRefreshInstrumentsState(session,customerID);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetQuotePolicyDetailsAndRefreshInstrumentsState2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        public DataSet GetQuotePolicyDetailsAndRefreshInstrumentsState(Guid session, Guid customerID)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                return this.InternalGetQuotePolicyDetailsAndRefreshInstrumentsState(session,token.UserID);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetQuotePolicyDetailsAndRefreshInstrumentsState:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        private DataSet InternalGetQuotePolicyDetailsAndRefreshInstrumentsState(Guid session,Guid customerID)
        {
            DataSet dataSet = Application.Default.TradingConsoleServer.GetQuotePolicyDetails(customerID);
            TradingConsoleState state = SessionManager.Default.GetTradingConsoleState(session);
            Application.Default.TradingConsoleServer.RefreshInstrumentsState2(dataSet, ref state, session.ToString());
            if (state != null)
            {
                TraderState traderState = new TraderState(state);
                traderState.CaculateQuotationFilterSign();
                SessionManager.Default.AddTradingConsoleState(session, traderState);
            }
            return dataSet;
        }

     

        public void LogResultOfGetCommands(Guid userId, bool resultOfGetCommand, int firstSequence, int lastSequence)
        {
            AppDebug.LogEvent("TradingConsole.LogResultOfGetCommands:",
                string.Format("{0}, {1}, firstSequence = {2}, lastSequence = {3}", userId, resultOfGetCommand ? "GetCommands" : "GetCommands2", firstSequence, lastSequence), EventLogEntryType.Information);
        }

        public XElement Quote(Guid session,string instrumentID, double quoteLot, int BSStatus)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state == null ? false : state.IsEmployee;
                Application.Default.TradingConsoleServer.Quote(token, isEmployee, Application.Default.StateServer, GetLocalIP(), instrumentID, quoteLot, BSStatus);
                return XmlResultHelper.NewResult(StringConstants.OK_RESULT);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Quote:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public XElement Quote2(Guid session, string instrumentID, double buyQuoteLot, double sellQuoteLot, int tick)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state == null ? false : state.IsEmployee;
                Application.Default.TradingConsoleServer.Quote2(token, isEmployee, Application.Default.StateServer, GetLocalIP(), instrumentID, buyQuoteLot, sellQuoteLot, tick);
                return XmlResultHelper.NewResult(StringConstants.OK_RESULT);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Quote2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public void CancelQuote(Guid session,string instrumentID, double buyQuoteLot, double sellQuoteLot)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                Application.Default.TradingConsoleServer.CancelQuote(token, Application.Default.StateServer, GetLocalIP(), instrumentID, buyQuoteLot, sellQuoteLot);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.CancelQuote:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public XElement   CancelLMTOrder(Guid session,string transactionID)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);

                var result = Application.Default.TradingConsoleServer.CancelLMTOrder(token, Application.Default.StateServer, transactionID);
                return XmlResultHelper.NewResult(result.ToString());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.CancelLMTOrder:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public bool UpdateAccountLock(Guid session,string agentAccountID, string[][] arrayAccountLock)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session); 
                return Application.Default.TradingConsoleServer.UpdateAccountLock(token, Application.Default.StateServer, agentAccountID, arrayAccountLock);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdateAccountLock:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return false;
        }

        public string GetSystemTime()
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetSystemTime().ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetSystemTime:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw exception;
            }
        }

        public TimeSyncSettings GetTimeSyncSettings()
        {
            try
            {
                TimeSyncSection timeSyncSection = (TimeSyncSection)ConfigurationManager.GetSection("TimeSync");
                TimeSyncSettings timeSyncSettings
                    = new TimeSyncSettings(timeSyncSection.SyncInterval, timeSyncSection.MinAdjustedTimeOfSyncSoon);
                return timeSyncSettings;
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetTimeSyncSettings:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw exception;
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
                AppDebug.LogEvent("TradingConsole.GetCurrencyRateByAccountID:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

   

        public DataSet GetInstruments(Guid session,ArrayList instrumentIDs)
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
                AppDebug.LogEvent("TradingConsole.GetInstruments:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }



        public XElement DeleteMessage(Guid session, Guid id)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                bool result= Application.Default.TradingConsoleServer.DeleteMessage(token, id);
                return XmlResultHelper.NewResult(result.ToXmlResult());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.DeleteMessage:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public void GetCustomerInfo(out string customerCode, out string customerName)
        {
            customerCode = string.Empty;
            customerName = string.Empty;
        }















        public DataSet GetDealingPolicyDetails(Guid session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                using (SqlConnection sqlConnection = new SqlConnection(SettingManager.Default.ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("P_GetDealingPolicyDetails", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlConnection.Open();
                        SqlCommandBuilder.DeriveParameters(sqlCommand);
                        sqlCommand.Parameters["@userId"].Value = token.UserID;
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);
                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetDealingPolicyDetails", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

















        public void NotifyCustomerExecuteOrderForJava(Guid session, string[][] arrayNotifyCustomerExecuteOrder, string companyCode, string version)
        {
            try
            {

                Token token = SessionManager.Default.GetToken(session);
                string physicalPath = SettingManager.Default.PhysicPath+"\\" +  companyCode + "\\" + version + "\\";
                Application.Default.TradingConsoleServer.NotifyCustomerExecuteOrder(token, Application.Default.StateServer, physicalPath, arrayNotifyCustomerExecuteOrder);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.NotifyCustomerExecuteOrderForJava:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
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
                AppDebug.LogEvent("TradingConsole.GetLogoForJava:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        public XmlNode GetColorSettingsForJava(string companyCode)
        {
            try
            {
                string xmlPath = SettingManager.Default.PhysicPath+"\\" +  companyCode + "\\ColorSettingsForJava.xml";

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);
                System.Xml.XmlNode node = doc.GetElementsByTagName("ColorSettings")[0];
                return node;
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Service.GetColorSettingsForJava", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        //no use
        public XmlNode GetParameterForJava(Guid  session,string companyCode, string version)
        {
            string physicalPath = SettingManager.Default.PhysicPath+"\\" + companyCode + "\\" + version + "\\";

            //Get xml
            try
            {
                string xmlPath = physicalPath + "Setting\\Parameter.xml";

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);

                xmlPath = physicalPath + "Setting\\Login.xml";
                System.Xml.XmlDocument doc2 = new System.Xml.XmlDocument();
                doc2.Load(xmlPath);
                System.Xml.XmlNode node2 = doc2.GetElementsByTagName("Login")[0];

                System.Xml.XmlNode parameterXmlNode = doc.GetElementsByTagName("Parameter")[0];

                string newsLanguage = node2.SelectNodes("NewsLanguage").Item(0).InnerXml;

                //this.Context.Session["NewsLanguage"] = newsLanguage.ToLower();
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
                System.Xml.XmlNode node = doc.GetElementsByTagName("Parameters")[0];
                return node;
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Service.GetParameterForJava", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        /*
        private CultureInfo GetNewsCultureInfo(string newsLanguage)
        {
            CultureInfo newsCultureInfo;
            switch (newsLanguage)
            {
                case "chs":
                    newsCultureInfo = new CultureInfo("zh-CN", false);
                    break;
                case "cht":
                    newsCultureInfo = new CultureInfo("zh-HK", false);
                    break;
                case "eng":
                    newsCultureInfo = new CultureInfo("en-US", false);
                    break;
                case "jpn":
                    newsCultureInfo = new CultureInfo("ja-JP", false);
                    break;
                case "kor":
                    newsCultureInfo = new CultureInfo("ko-KR", false);
                    break;
                default:
                    newsCultureInfo = new CultureInfo("en-US", false);
                    break;
            }
            return newsCultureInfo;
        }
        */

        public byte[] GetTracePropertiesForJava()
        {
            try
            {
                return File.ReadAllBytes( SettingManager.Default.PhysicPath+"\\" + "TradingConsole.Trace.Properties");
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Service.GetTracePropertiesForJava", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        /*
        public DataSet LoadSystemParameters()
        {
            TradingConsoleServer tradingConsoleServer = (TradingConsoleServer)Application["TradingConsoleServer"];
            Token token=(Token)Session["Token"];
            return tradingConsoleServer.LoadSystemParameters(token);
        }
        */

        public bool UpdateSystemParameters(Guid session, string parameters, string objectID)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                return Application.Default.TradingConsoleServer.UpdateSystemParameters(token, parameters, objectID);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdateSystemParameters:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return false;
        }

        public System.Data.DataSet GetNewsList(string newsCategoryID, string language, DateTime date)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetNewsList(newsCategoryID, language, date);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetNewsList:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }



        public DataSet GetNewsContents(Guid session, string newsID)
        {
            try
            {
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                return Application.Default.TradingConsoleServer.GetNewsContents(newsID, state.Language);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetNewsContents:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        public DataSet GetInterestRate(Guid[] orderIds)
        {
            try
            {
                return Application.Default.TradingConsoleServer.GetInterestRate(orderIds);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetInterestRate:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        public DataSet GetInterestRate2(Guid session,Guid interestRateId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                return Application.Default.TradingConsoleServer.GetInterestRate2(token, interestRateId);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetInterestRate2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        private string GetLocalIP()
        {
            //this.Context.Request.ServerVariables[""].ToString();
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
                AppDebug.LogEvent("TradingConsole.RefreshAgentAccountOrder:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw exception;
            }
        }

      

        public XmlDocument GetOuterNews(string newsUrl)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(newsUrl);
                //doc.Load("D:\\Solutions.old\\iExchange\\TradingConsole\\News.XML");
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return doc;
        }

        public void SaveLog(Guid session,string logCode, DateTime timestamp, string action)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state == null ? false : state.IsEmployee;
                Application.Default.TradingConsoleServer.SaveLog(token, isEmployee, GetLocalIP(), logCode, timestamp, action);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.SaveLog:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }



        public void SaveLogForWeb(Guid session, string logCode, string action, string transactionId)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                bool isEmployee = state == null ? false : state.IsEmployee;
                Application.Default.TradingConsoleServer.SaveLog(token, isEmployee, GetLocalIP(), logCode, DateTime.Now, action, new Guid(transactionId));
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.SaveLogForWeb:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

       


     


        public XElement  Apply(Guid session,Guid id, string accountBankApprovedId, string accountId, string countryId, string bankId, string bankName,
            string accountBankNo, string accountBankType,//#00;银行卡|#01;存折
            string accountOpener, string accountBankProp, Guid accountBankBCId, string accountBankBCName,
            string idType,//#0;身份证|#1;户口簿|#2;护照|#3;军官证|#4;士兵证|#5;港澳居民来往内地通行证|#6;台湾同胞来往内地通行证|#7;临时身份证|#8;外国人居留证|#9;警官证|#x;其他证件
            string idNo, string bankProvinceId, string bankCityId, string bankAddress, string swiftCode, int applicationType)
        {
            try
            {
                string connectionString = SettingManager.Default.ConnectionString;
                Token token = SessionManager.Default.GetToken(session);
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("dbo.P_ApplyAccountBank", sqlConnection))
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

                        int result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                        if (result != 0)
                        {
                            return XmlResultHelper.ErrorResult;
                        }

                        bool approved = (bool)sqlCommand.Parameters["@approved"].Value;
                        return XmlResultHelper.NewResult(approved.ToXmlResult());
                    }
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.Apply:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

       

       

      

     
       

       

        private void CreateTickByTickHistoryDatas(object state)
        {
            TickByTickHistoryDataArgument tickByTickHistoryDataArgument = (TickByTickHistoryDataArgument)state;
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
                    CommandManager.Default.AddCommand(tickByTickHistoryDataArgument.Token, new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult));
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.CreateTickByTickHistoryDatas",
                        string.Format("Instrument {0} doesn't exists in TradingConsoleState", tickByTickHistoryDataArgument.InstrumentId), EventLogEntryType.Warning);
                    CommandManager.Default.AddCommand(tickByTickHistoryDataArgument.Token, new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, null));
                }
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("TradingConsole.CreateTickByTickHistoryDatas", e.ToString(), EventLogEntryType.Error);
                CommandManager.Default.AddCommand(tickByTickHistoryDataArgument.Token, new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, e));
            }
        }

        private void CreateTickByTickHistoryDatas2(object state)
        {
            TickByTickHistoryDataArgument2 tickByTickHistoryDataArgument = (TickByTickHistoryDataArgument2)state;
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
                    CommandManager.Default.AddCommand(tickByTickHistoryDataArgument.Token, new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult));
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.CreateTickByTickHistoryDatas2",
                        string.Format("Instrument {0} doesn't exists in TradingConsoleState", tickByTickHistoryDataArgument.InstrumentId), EventLogEntryType.Warning);
                    CommandManager.Default.AddCommand(tickByTickHistoryDataArgument.Token, new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, null));
                }
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("TradingConsole.CreateTickByTickHistoryDatas2", e.ToString(), EventLogEntryType.Error);
                CommandManager.Default.AddCommand(tickByTickHistoryDataArgument.Token, new AsyncCommand(0, tickByTickHistoryDataArgument.AsyncResult, true, e));
            }
        }
    }


    //used in the chart
    class DataManagerKey
    {
        Guid _instrumentId;
        string _dataCycle;

        private DataManagerKey(Guid instrumentId, string dataCycle)
        {
            this._instrumentId = instrumentId;
            this._dataCycle = dataCycle;
        }
        public static DataManagerKey create(Guid instrumentId, string dataCycle)
        {
            return new DataManagerKey(instrumentId, dataCycle);
        }

        public override bool Equals(object o)
        {
            DataManagerKey dataManagerKey = (DataManagerKey)o;
            return (dataManagerKey._instrumentId.Equals(this._instrumentId)
                && dataManagerKey._dataCycle.Equals(this._dataCycle));
        }

        public override int GetHashCode()
        {
            int hashCode = this._instrumentId.GetHashCode();
            hashCode ^= this._dataCycle.GetHashCode();

            return hashCode;
        }
    }

    static class FixBug
    {
        internal static string Fix(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            value = value.Replace("&lt;", "<");
            value = value.Replace("&gt;", ">");
            value = value.Replace("&amp;", "&");
            value = value.Replace("&apos;", "'");
            value = value.Replace("&quot;", "\"");

            return value;
        }
    }

    //This class used to avoid too many thread are created to create chart data
   public class AssistantOfCreateChartData2
    {
        private Queue<TaskOfCreateChartData2> _Tasks = new Queue<TaskOfCreateChartData2>();
        private object _TaskLock = new object();
        private int _WorkItemNumber = 0;

        public void AddTask(AsyncResult asyncResult, ChartDataArgument2 argument, WaitCallback funcToCreateChartData)
        {
            lock (this._TaskLock)
            {
                TaskOfCreateChartData2 task = new TaskOfCreateChartData2(argument, funcToCreateChartData);
                bool needQueueWorkItem = this._WorkItemNumber <= 3;//so, up to 3 thread will be created
                this._Tasks.Enqueue(task);
                if (needQueueWorkItem)
                {
                    this._WorkItemNumber++;
                    ThreadPool.QueueUserWorkItem(this.CreateChartData, null);
                }
            }
        }

        private void CreateChartData(object state)
        {
            TaskOfCreateChartData2 task = null;

            while (true)
            {
                task = null;
                lock (this._TaskLock)
                {
                    if (this._Tasks.Count > 0)
                    {
                        task = this._Tasks.Dequeue();
                    }
                    else
                    {
                        this._WorkItemNumber--;
                        break;
                    }
                }

                task.FuncToCreateChartData(task.ArgumentOfCreateChartData);
            }
        }
    }

    class TaskOfCreateChartData2
    {
        public ChartDataArgument2 ArgumentOfCreateChartData;
        public WaitCallback FuncToCreateChartData;

        public TaskOfCreateChartData2(ChartDataArgument2 argument, WaitCallback funcToCreateChartData)
        {
            this.ArgumentOfCreateChartData = argument;
            this.FuncToCreateChartData = funcToCreateChartData;
        }
    }

}
