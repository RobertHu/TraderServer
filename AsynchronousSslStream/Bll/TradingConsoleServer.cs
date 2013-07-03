using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using System.Data.SqlClient;
using Trader.Server.Setting;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using iExchange.Common.Client;
using System.Net;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Configuration;
using Trader.Server.Util;
using Trader.Common;
namespace Trader.Server.Bll
{
    public class TradingConsoleServer
    {
        private static readonly string _FixLastPeriodFlag = "[FixLastPeriod]";
        private static bool AllowInstantPayment = false;
        private static TimeSpan? MaxPriceDelayForSpotOrder = null;
        private Dictionary<string, FromToDataSet> _FixLastPeriodCache = new Dictionary<string, FromToDataSet>();
        private object _FixLastPeriodCacheLock = new object();
        private string connectionString = "";
        private int tickDataReturnCount = 300;
        private Hashtable extends = new Hashtable();
        public  TradingConsoleServer() { }
        static TradingConsoleServer()
        {
            try
            {
                string connectionString = SettingManager.Default.ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand sqlCommand = connection.CreateCommand();
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "SELECT [AllowInstantPayment], [MaxPriceDelayForSpotOrder] FROM [SystemParameter]";
                    connection.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    reader.Read();
                    TradingConsoleServer.AllowInstantPayment = (bool)reader["AllowInstantPayment"];
                    object value = reader["MaxPriceDelayForSpotOrder"];
                    if (value == DBNull.Value)
                    {
                        MaxPriceDelayForSpotOrder = null;
                    }
                    else
                    {
                        MaxPriceDelayForSpotOrder = TimeSpan.FromSeconds((int)value);
                    }
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                TradingConsoleServer.AllowInstantPayment = false;
                AppDebug.LogEvent("TradingConsoleServer", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }
        public TradingConsoleServer(string connectionString, int tickDataReturnCount)
        {
            this.connectionString = connectionString;
            this.tickDataReturnCount = tickDataReturnCount;
        }

        //Use in TickByTick Chart
        public DataSet GetTickByTickHistoryDatas(Guid instrumentId, Guid quotePolicyId)
        {
            string sql = string.Format("EXEC dbo.P_GetHistoryTickDatas2 '{0}','{1}','{2:yyyy-MM-dd HH:mm:ss.fff}',{3}", instrumentId, quotePolicyId, DateTime.Now, this.tickDataReturnCount);
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                dataSet.Tables[0].TableName = "ChartDataTable";
            }
            return dataSet;
        }

        //Use in TickByTick Chart
        public DataSet GetTickByTickHistoryDatas2(Guid instrumentId, Guid quotePolicyId, DateTime from, DateTime to)
        {
            string commandTimeOutStr =ConfigurationManager.AppSettings["iExchange.TradingConsole.CommandTimeout"];
            TimeSpan commandTimeOut = TimeSpan.Parse(String.IsNullOrEmpty(commandTimeOutStr) ? "00:00:30" : commandTimeOutStr);

            string sql = String.Format("EXEC P_GetChartData2 '{0}', '{1}', '{2}', '{3:yyyy-MM-dd HH:mm:ss.fff}', '{4:yyyy-MM-dd HH:mm:ss.fff}'", instrumentId, quotePolicyId, "1 Sec", from, to);
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString, commandTimeOut);
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                dataSet.Tables[0].TableName = "ChartDataTable";
            }
            return dataSet;
        }

        public DataSet GetChartData2(Guid instrumentId, Guid quotePolicyId, string dataCycle, DateTime from, DateTime to)
        {
            string commandTimeOutStr = ConfigurationManager.AppSettings["iExchange.TradingConsole.CommandTimeout"];
            TimeSpan commandTimeOut = TimeSpan.Parse(String.IsNullOrEmpty(commandTimeOutStr) ? "00:00:30" : commandTimeOutStr);

            if (dataCycle.EndsWith(_FixLastPeriodFlag))
            {
                lock (this._FixLastPeriodCacheLock)
                {
                    dataCycle = dataCycle.Substring(0, dataCycle.Length - _FixLastPeriodFlag.Length);

                    string key = instrumentId.ToString() + quotePolicyId.ToString() + dataCycle;
                    FromToDataSet fromToDataSet = null;
                    if (this._FixLastPeriodCache.TryGetValue(key, out fromToDataSet))
                    {
                        if (fromToDataSet.From.CompareTo(from) <= 0 && fromToDataSet.To.CompareTo(to) >= 0)
                        {
                            return fromToDataSet.DataSet;
                        }
                        else
                        {
                            this._FixLastPeriodCache.Remove(key);
                        }
                    }

                    string sql = String.Format("EXEC P_GetChartData2 '{0}', '{1}', '{2}', '{3:yyyy-MM-dd HH:mm:ss.fff}', '{4:yyyy-MM-dd HH:mm:ss.fff}'", instrumentId, quotePolicyId, dataCycle, from, to);
                    DataSet dataSet = DataAccess.GetData(sql, this.connectionString, commandTimeOut);
                    if (dataSet != null && dataSet.Tables.Count > 0)
                    {
                        dataSet.Tables[0].TableName = "ChartDataTable";
                    }

                    this._FixLastPeriodCache.Add(key, new FromToDataSet(from, to, dataSet));

                    return dataSet;
                }
            }
            else
            {
                string sql = String.Format("EXEC P_GetChartData2 '{0}', '{1}', '{2}', '{3:yyyy-MM-dd HH:mm:ss.fff}', '{4:yyyy-MM-dd HH:mm:ss.fff}'", instrumentId, quotePolicyId, dataCycle, from, to);
                DataSet dataSet = DataAccess.GetData(sql, this.connectionString, commandTimeOut);
                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    dataSet.Tables[0].TableName = "ChartDataTable";
                }
                return dataSet;
            }
        }

        public DataSet GetTickDatas(Guid instrumentId, Guid quotePolicyId, DateTime dateTime, int minutes)
        {
            string sql = string.Format("SELECT [Date],[Open],[Close],[High],[Low],Volume FROM dbo.FT_CalcLastRealTimeChartData('{0}','{1}','{2:yyyy-MM-dd HH:mm:ss.fff}',{3})", instrumentId, quotePolicyId, dateTime, minutes);
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                dataSet.Tables[0].TableName = "ChartDataTable";
            }
            return dataSet;
        }

        public DataSet GetLoginParameters(Guid customerID, string companyName)
        {
            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand("dbo.P_CompanyCheckForTradingConsole", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@customerID", SqlDbType.UniqueIdentifier, 38);
            sqlParameter.Value = customerID;
            sqlParameter = sqlCommand.Parameters.Add("@companyName", SqlDbType.NVarChar, 3);
            sqlParameter.Value = companyName;
            sqlConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(dataSet);
            sqlConnection.Close();
            return dataSet;
        }

        public XmlNode GetAccounts(Token token, StateServerService stateServer, Guid[] accountIDs, bool includeTransactions)
        {
            XmlNode accountsData = stateServer.GetAccounts(token, accountIDs, includeTransactions);

            return (accountsData);
        }

        public XmlNode GetAccountsForCut(Token token, StateServerService stateServer, Guid[] accountIDs, bool includeTransactions)
        {
            XmlNode accountsData = stateServer.GetAccountsForCut(token, accountIDs, includeTransactions);

            return (accountsData);
        }

        public DataSet GetAccountsForTradingConsole(Guid userId)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT ID,Code,[Name],GroupName,IsSelected FROM [dbo].[FT_GetAccountForTradingSetting](@userId)";
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.Parameters.Add(new SqlParameter("@userId", userId));
                    sqlConnection.Open();

                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    dataAdapter.SelectCommand = sqlCommand;
                    DataSet dataSet = new DataSet();
                    dataAdapter.Fill(dataSet);
                    return dataSet;
                }
            }
        }

        public bool UpdateAccountSetting(Guid userId, Guid[] accountIds)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        sqlCommand.CommandText = "[dbo].[P_UpdateAccountSetting]";
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.Add(new SqlParameter("@userID", userId));

                        XElement rootElement = new XElement("AccountSetting");
                        for (int i = 0; i < accountIds.Length; i++)
                        {
                            XElement accountElement = new XElement("Account");
                            accountElement.SetAttributeValue("ID", accountIds[i].ToString());
                            accountElement.SetAttributeValue("Sequence", i);
                            rootElement.Add(accountElement);
                        }
                        sqlCommand.Parameters.Add(new SqlParameter("@xmlAccountSetting", rootElement.ToString()));
                        SqlParameter sqlParameter = sqlCommand.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                        sqlParameter.Direction = ParameterDirection.ReturnValue;

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();

                        return (int)sqlCommand.Parameters["@RETURN_VALUE"].Value == 0;
                    }
                }
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("UpdateAccountSetting", ex.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public DataSet GetInstrumentsForTradingConsole(Guid userId)
        {
            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = new SqlCommand("dbo.P_GetInstrumentsForTradingConsole", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@CustomerID", SqlDbType.UniqueIdentifier, 38);
            sqlParameter.Value = userId;

            sqlConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables.Count > 0)
            {
                dataSet.Tables[0].TableName = "Instrument";
            }

            sqlConnection.Close();

            return dataSet;
        }

        public void RefreshInstrumentsState(DataSet dataSet, ref TradingConsoleState state2, string sessionId)
        {
            TradingConsoleState state = state2 == null ? new TradingConsoleState(sessionId) : state2;
            if (state.Instruments.Count > 0)
            {
                state.Instruments.Clear();
            }

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                StringBuilder quotePolicyInfo = new StringBuilder();
                quotePolicyInfo.Append("SessionId = " + state.SessionId + "\t");

                DataRowCollection rows = dataSet.Tables["Instrument"].Rows;
                foreach (DataRow instrumentRow in rows)
                {
                    state.Instruments.Add(instrumentRow["ID"], instrumentRow["QuotePolicyID"]);

                    if (quotePolicyInfo.Length > 0) quotePolicyInfo.Append(";");
                    quotePolicyInfo.Append(instrumentRow["ID"]);
                    quotePolicyInfo.Append("=");
                    quotePolicyInfo.Append(instrumentRow["QuotePolicyID"]);
                }
            }
            state2 = state;
        }

        public DataSet GetQuotePolicyDetails(Guid userId)
        {
            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = new SqlCommand("dbo.P_GetQuotePolicyDetailsForTradingConsole", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@CustomerID", SqlDbType.UniqueIdentifier, 38);
            sqlParameter.Value = userId;

            sqlConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables.Count > 0)
            {
                dataSet.Tables[0].TableName = "QuotePolicyDetail";
            }

            sqlConnection.Close();
            return dataSet;
        }

        public void RefreshInstrumentsState2(DataSet dataSet, ref TradingConsoleState state2, string sessionId)
        {
            TradingConsoleState state = state2 == null ? new TradingConsoleState(sessionId) : state2;
            if (state.Instruments.Count > 0)
            {
                state.Instruments.Clear();
            }

            if (dataSet.Tables.Count > 0)
            {
                StringBuilder quotePolicyInfo = new StringBuilder();
                quotePolicyInfo.Append("SessionId = " + state.SessionId + "\t");

                DataRowCollection rows = dataSet.Tables["QuotePolicyDetail"].Rows;
                foreach (DataRow quotePolicyDetailRow in rows)
                {
                    state.Instruments.Add(quotePolicyDetailRow["InstrumentID"], quotePolicyDetailRow["QuotePolicyID"]);

                    if (quotePolicyInfo.Length > 0) quotePolicyInfo.Append(";");
                    quotePolicyInfo.Append(quotePolicyDetailRow["InstrumentID"]);
                    quotePolicyInfo.Append("=");
                    quotePolicyInfo.Append(quotePolicyDetailRow["QuotePolicyID"]);
                }
            }
            state2 = state;


        }

        public void Quote(Token token, bool isEmployee, StateServerService stateServer, string localIP, string instrumentID, double quoteLot, int BSStatus)
        {
            stateServer.Quote(token, new Guid(instrumentID), quoteLot, BSStatus);

            SaveQuote(token, isEmployee, localIP, instrumentID);
        }

        public void Quote2(Token token, bool isEmployee, StateServerService stateServer, string localIP, string instrumentID, double buyQuoteLot, double sellQuoteLot, int tick)
        {
            stateServer.Quote2(token, new Guid(instrumentID), buyQuoteLot, sellQuoteLot, tick);

            SaveQuote(token, isEmployee, localIP, instrumentID);
        }

        public void CancelQuote(Token token, StateServerService stateServer, string localIP, string instrumentID, double buyQuoteLot, double sellQuoteLot)
        {
            stateServer.CancelQuote(token, new Guid(instrumentID), buyQuoteLot, sellQuoteLot);

            //SaveCancelQuote(token, localIP, instrumentID);
        }

        public TransactionError CancelLMTOrder(Token token, StateServerService stateServer, string transactionID)
        {
            return stateServer.Cancel(token, new Guid(transactionID), CancelReason.CustomerCanceled);
        }

        public bool UpdateAccountLock(Token token, StateServerService stateServer, string agentAccountID, string[][] arrayAccountLock)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode updateAccountLock = doc.CreateNode(XmlNodeType.Element, "UpdateAccountLock", null);

            XmlAttribute attribute1 = doc.CreateAttribute("AgentAccountID");
            attribute1.Value = agentAccountID;
            updateAccountLock.Attributes.Append(attribute1);

            foreach (string[] arrayAccountLockTmp in arrayAccountLock)
            {
                XmlNode account = doc.CreateNode(XmlNodeType.Element, "Account", null);
                XmlAttribute attribute = doc.CreateAttribute("ID");
                attribute.Value = arrayAccountLockTmp[0];
                account.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("IsLocked");
                attribute.Value = arrayAccountLockTmp[1];
                account.Attributes.Append(attribute);

                updateAccountLock.AppendChild(account);
            }
            bool isSucced = stateServer.UpdateAccountLock(token, updateAccountLock);
            return (isSucced);
        }

        public DateTime GetSystemTime()
        {
            return DateTime.Now;
        }

        public TransactionError Place(Token token, StateServerService stateServer, XmlNode tran, out string tranCode)
        {
            return this.Place(token, stateServer, tran, false, out tranCode);
        }

        internal bool ChangeLeverage(Token token, StateServerService stateServer, Guid accountId, int leverage, out decimal necessary)
        {
            return stateServer.ChangeLeverage(token, accountId, leverage, out necessary);
        }

        public TransactionError Place(Token token, StateServerService stateServer, XmlNode tran, bool fromMobile, out string tranCode)
        {
            try
            {
                return stateServer.Place(token, tran, out tranCode);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public TransactionError MultipleClose(Token token, StateServerService stateServer, Guid[] orderIds, out XmlNode xmlTran, out XmlNode xmlAccount)
        {
            try
            {
                return stateServer.MultipleClose(token, orderIds, out xmlTran, out xmlAccount);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public TransactionError Assign(Token token, StateServerService stateServer, ref XmlNode xmlTransaction, out XmlNode xmlAccount, out XmlNode xmlInstrument)
        {
            try
            {
                string s = xmlTransaction.OuterXml;
                TransactionError transactionError = stateServer.Assign(token, ref xmlTransaction, out xmlAccount, out xmlInstrument);

                return transactionError;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public XmlNode GetInstrumentForSetting(Token token, StateServerService stateServer)
        {
            DataSet dataSet = stateServer.GetInstrumentForSetting(token);
            if (dataSet == null) return null;

            XmlDocument doc = new XmlDocument();
            XmlNode xmlNodeTop = doc.CreateNode(XmlNodeType.Element, "Instruments", null);
            DataTable table = dataSet.Tables[0];
            DataRowCollection rows = table.Rows;
            foreach (DataRow row in rows)
            {
                XmlNode xmlNode = doc.CreateNode(XmlNodeType.Element, "Instrument", null);

                XmlAttribute attr = doc.CreateAttribute("ID");
                attr.Value = ((Guid)row["ID"]).ToString();
                xmlNode.Attributes.Append(attr);

                attr = doc.CreateAttribute("Code");
                attr.Value = (string)row["Code"];
                xmlNode.Attributes.Append(attr);

                attr = doc.CreateAttribute("Description");
                attr.Value = (string)row["Description"];
                xmlNode.Attributes.Append(attr);

                attr = doc.CreateAttribute("GroupName");
                attr.Value = row.Table.Columns.Contains("GroupName") ? (string)row["GroupName"] : "";
                xmlNode.Attributes.Append(attr);

                xmlNodeTop.AppendChild(xmlNode);
            }
            return xmlNodeTop;
        }

        public DataSet GetCurrencyRateByAccountID(string accountID)
        {
            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = new SqlCommand("dbo.P_GetAccountRelationInfoByAccountID", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@AccountID", SqlDbType.UniqueIdentifier, 38);
            sqlParameter.Value = new Guid(accountID);

            sqlConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables.Count > 0)
                dataSet.Tables[0].TableName = "CurrencyRate";
            if (dataSet.Tables.Count > 1)
                dataSet.Tables[1].TableName = "Account";

            sqlConnection.Close();
            return (dataSet);
        }

        public DataSet GetInstruments(Token token, ArrayList instrumentIDs, TradingConsoleState state)
        {
            string xmlInstrumentIDs = string.Empty;

            System.Collections.IEnumerator instrumentIDs2 = instrumentIDs.GetEnumerator();
            while (instrumentIDs2.MoveNext())
            {
                xmlInstrumentIDs += "<Instrument ID=\"" + instrumentIDs2.Current.ToString() + "\" />";
            }

            if (xmlInstrumentIDs != string.Empty) xmlInstrumentIDs = "<Instruments>" + xmlInstrumentIDs + "</Instruments>";
            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = new SqlCommand("dbo.P_GetInstrumentForTradingConsole2", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@userID", SqlDbType.UniqueIdentifier, 38);
            sqlParameter.Value = token.UserID;
            sqlParameter = sqlCommand.Parameters.Add("@xmlInstrumentIDs", SqlDbType.NText);
            sqlParameter.Value = xmlInstrumentIDs;

            sqlConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables.Count > 0)
                dataSet.Tables[0].TableName = "Instrument";
            if (dataSet.Tables.Count > 1)
                dataSet.Tables[1].TableName = "TradingTime";
            if (dataSet.Tables.Count > 2)
                dataSet.Tables[2].TableName = "TradePolicyDetail";
            if (dataSet.Tables.Count > 3)
                dataSet.Tables[3].TableName = "Quotation";

            sqlConnection.Close();

            instrumentIDs2.Reset();
            while (instrumentIDs2.MoveNext())
            {
                Guid instrumentID = (Guid)instrumentIDs2.Current;
                if (!state.Instruments.ContainsKey(instrumentID))
                {
                    StringBuilder quotePolicyInfo = new StringBuilder();
                    quotePolicyInfo.Append("SessionId=" + state.SessionId + "\t");

                    DataRowCollection rows = dataSet.Tables["Instrument"].Rows;
                    foreach (DataRow instrumentRow in rows)
                    {
                        state.Instruments.Add(instrumentRow["ID"], instrumentRow["QuotePolicyID"]);
                        if (quotePolicyInfo.Length > 0) quotePolicyInfo.Append(";");
                        quotePolicyInfo.Append(instrumentRow["ID"]);
                        quotePolicyInfo.Append("=");
                        quotePolicyInfo.Append(instrumentRow["QuotePolicyID"]);
                    }
                }
            }

            return (dataSet);
        }

        public DataSet GetSystemParameters(Token token, string objectID)
        {
            string sql = "SELECT Parameter FROM Settings2 WHERE UserID = '" + token.UserID.ToString() + "' AND AppType = " + ((System.Int32)token.AppType).ToString() + " AND ObjectID = '" + objectID + "'";
            return DataAccess.GetData(sql, this.connectionString);
        }

        public DataSet GetMessages(Token token)
        {
            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = new SqlCommand("dbo.P_GetMessageByRecipientsID", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@RecipientsID", SqlDbType.UniqueIdentifier, 38);
            sqlParameter.Value = token.UserID;

            sqlConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables.Count > 0)
                dataSet.Tables[0].TableName = "Messages";

            sqlConnection.Close();

            return (dataSet);
        }
        public DataSet GetMessageContent(Guid id)
        {
            string sql = "SELECT Content FROM dbo.Message WHERE Id = '" + id.ToString() + "'";
            return DataAccess.GetData(sql, this.connectionString);
        }
        public bool DeleteMessage(Token token, Guid id)
        {
            bool isDeleteSucced = false;
            try
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                SqlCommand sqlCommand = new SqlCommand("dbo.P_DeleteMessageByRecipientsID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlParameter sqlParameter = sqlCommand.Parameters.Add("@RecipientsID", SqlDbType.UniqueIdentifier, 38);
                sqlParameter.Value = token.UserID;
                sqlParameter = sqlCommand.Parameters.Add("@MessageID", SqlDbType.UniqueIdentifier, 38);
                sqlParameter.Value = id;

                sqlConnection.Open();
                //SqlDataAdapter sqlDataAdapter=new SqlDataAdapter(sqlCommand);
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();

                isDeleteSucced = true;
            }
            catch
            {
                isDeleteSucced = false;
            }
            return (isDeleteSucced);
        }

        public DataSet GetUpdateInstrumentSetting(Token token, StateServerService stateServer, string[] instrumentIDs)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode instrumentSetting = doc.CreateNode(XmlNodeType.Element, "InstrumentSetting", null);
            int index = 1;
            foreach (string instrumentID in instrumentIDs)
            {
                XmlNode instrument = doc.CreateNode(XmlNodeType.Element, "Instrument", null);
                XmlAttribute attribute = doc.CreateAttribute("ID");
                attribute.Value = instrumentID;
                instrument.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("Sequence");
                attribute.Value = index.ToString();
                instrument.Attributes.Append(attribute);

                instrumentSetting.AppendChild(instrument);
                index++;
            }
            DataSet instruments = stateServer.UpdateInstrumentSetting(token, instrumentSetting);
            return instruments;
        }

        public void UpdateInstrumentSetting(DataSet instruments, string[] instrumentIDs, TradingConsoleState state)
        {
            DataRowCollection rows;

            if (instrumentIDs.Length <= 0)
            {
                state.Instruments.Clear();
            }
            else
            {
                ArrayList keys = new ArrayList();
                foreach (Guid key in state.Instruments.Keys)
                {
                    bool isExists = false;
                    foreach (string instrumentID in instrumentIDs)
                    {
                        if (key.ToString().ToUpper() == instrumentID.ToUpper())
                        {
                            isExists = true;
                            break;
                        }
                    }
                    if (!isExists)
                        keys.Add(key);
                }

                for (int i = 0; i < keys.Count; i++)
                {
                    state.Instruments.Remove(keys[i]);
                }
                if (instruments != null && instruments.Tables.Count > 0)
                {
                    StringBuilder quotePolicyInfo = new StringBuilder();
                    quotePolicyInfo.Append("SessionId = " + state.SessionId + "\t");

                    rows = instruments.Tables["Instrument"].Rows;
                    foreach (DataRow instrumentRow in rows)
                    {
                        if (!state.Instruments.ContainsKey(instrumentRow["ID"]))
                        {
                            state.Instruments.Add(instrumentRow["ID"], instrumentRow["QuotePolicyID"]);

                            if (quotePolicyInfo.Length > 0) quotePolicyInfo.Append(";");
                            quotePolicyInfo.Append(instrumentRow["ID"]);
                            quotePolicyInfo.Append("=");
                            quotePolicyInfo.Append(instrumentRow["QuotePolicyID"]);
                        }
                    }
                }
            }
        }

        public void UpdateAccount(Guid accountID, Guid groupID, bool isDelete, bool isDeleteGroup, TradingConsoleState state)
        {
            if (isDelete)
            {
                if (state.Accounts.ContainsKey(accountID))
                {
                    state.Accounts.Remove(accountID);
                    if (isDeleteGroup)
                    {
                        state.AccountGroups.Remove(groupID);
                    }
                }
            }
            else
            {
                if (state.Accounts.ContainsKey(accountID))
                {
                    state.Accounts[accountID] = null;
                    state.AccountGroups[groupID] = null;
                }
                else
                {
                    state.Accounts.Add(accountID, null);
                    state.AccountGroups.Add(groupID, null);
                }
            }
        }

        public XElement UpdateQuotePolicyDetail(Guid instrumentID, Guid quotePolicyID, TradingConsoleState state)
        {
            try
            {
                AppDebug.LogEvent("TradingConsole.UpdateQuotePolicyDetail]QuotePolicy",
                    string.Format("SessionId = {0}, InstrumentId = {1}, QuotePolicyId = {2}{3}{4}", state.SessionId, instrumentID, quotePolicyID, Environment.NewLine, Environment.StackTrace),
                    EventLogEntryType.Information);

                if (state.Instruments.ContainsKey(instrumentID))
                {
                    state.Instruments[instrumentID] = quotePolicyID;
                }
                else
                {
                    state.Instruments.Add(instrumentID, quotePolicyID);
                }

                return XmlResultHelper.NewResult(StringConstants.OK_RESULT);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return XmlResultHelper.ErrorResult;
            }
        }

        //		public bool ChangePassword(Token token,StateServerService stateServer,byte[] oldPassword,byte[] newPassword)
        //		{
        //			bool isSucced = stateServer.ChangePassword(token,oldPassword,newPassword);
        //			return(isSucced);
        //		}


        //public bool AdditionalClient(Token token, StateServerService stateServer, string email, string receive, string organizationName, string customerName,
        //    string reportDate, string accountCode, string correspondingAddress, string registratedEmailAddress, string tel, string mobile,
        //    string fax, string fillName1, string ICNo1, string fillName2, string ICNo2, string fillName3, string ICNo3, out string reference)
        //{
        //    Guid submitPerson = token.UserID;
        //    string remark = ConfigurationSettings.AppSettings["FillName1"] + fillName1 + "," + ConfigurationSettings.AppSettings["ICNo1"] + ICNo1 + ";"
        //        + ConfigurationSettings.AppSettings["FillName2"] + fillName2 + "," + ConfigurationSettings.AppSettings["ICNo2"] + ICNo2 + ";"
        //        + ConfigurationSettings.AppSettings["FillName3"] + fillName3 + "," + ConfigurationSettings.AppSettings["ICNo3"] + ICNo3 + ";"
        //        + ConfigurationSettings.AppSettings["From"] + customerName;

        //    return this.AddMargin("OwnerRegistration", DateTime.Parse(reportDate), accountCode, email, null, null, null, organizationName, correspondingAddress,
        //        registratedEmailAddress, tel, mobile, fax, null, null, null, null, remark, submitPerson, out reference);
        //}

      

        public bool CallMarginExtension(Token token, StateServerService stateServer, string email, string receive, string organizationName,
            string customerName, string reportDate, string accountCode, string currency, string currencyValue, string dueDate, out string reference)
        {
            Guid submitPerson = token.UserID;
            return this.AddMargin("CMExtension", DateTime.Parse(reportDate), accountCode, email, currency, decimal.Parse(currencyValue), null, null, null,
                null, null, null, null, null, null, null, DateTime.Parse(dueDate), null, submitPerson, out reference);
        }

        

        public bool PaymentInstruction(Token token, String type, StateServerService stateServer, string email, string receive, string organizationName, string customerName,
            string reportDate, string accountCode, string currency, string currencyValue, string beneficiaryName, string bankAccount, string bankerName,
            string bankerAddress, string swiftCode, string remarks, string thisisClient, string targetAddress, string targetEmail, out string reference)
        {
            Guid submitPerson = token.UserID;
            bool result = this.AddMargin(type, DateTime.Parse(reportDate), accountCode, email, currency, decimal.Parse(currencyValue), bankAccount, beneficiaryName,
                targetAddress, targetEmail, null, null, null, bankerName, bankerAddress, swiftCode, null, remarks, submitPerson, out reference);

            if (result && TradingConsoleServer.AllowInstantPayment)
            {
                this.ApprovePaymentInstruction(reference, type);
            }
            return result;
        }



        private void ApprovePaymentInstruction(string reference, string type)
        {
            try
            {
                string backofficeServiceUrl = SettingManager.Default.BackofficeServiceUrl;
                string validateUrl = string.Format("{0}/MarginValidation?reference={1}&marginType={2}", backofficeServiceUrl, reference, type);
                WebRequest webRequest = WebRequest.Create(validateUrl);
                WebResponse webResponse = webRequest.GetResponse();
                XmlDocument document = new XmlDocument();
                document.Load(webResponse.GetResponseStream());
                XmlElement marginElement = (XmlElement)document.DocumentElement.GetElementsByTagName("Margin")[0];
                int retrurnValue = int.Parse(marginElement.Attributes["ReturnValue"].Value);
                webResponse.Close();
                if (retrurnValue == 0)
                {
                    string approveUrl = string.Format("{0}/ApproveMargin?reference={1}&marginType={2}", backofficeServiceUrl, reference, type);
                    webRequest = WebRequest.Create(approveUrl);
                    webResponse = webRequest.GetResponse();
                    document = new XmlDocument();
                    document.Load(webResponse.GetResponseStream());
                    marginElement = (XmlElement)document.DocumentElement.ChildNodes[0];
                    retrurnValue = int.Parse(marginElement.Attributes["ReturnValue"].Value);
                    if (retrurnValue != 1)
                    {
                        AppDebug.LogEvent("TraderService.ApprovePaymentInstruction", string.Format("ApproveMargin failed: {0}, refernce={1}", retrurnValue, reference), System.Diagnostics.EventLogEntryType.Error);
                    }
                }
                else
                {
                    AppDebug.LogEvent("TraderService.ApprovePaymentInstruction", string.Format("MarginValidation failed: {0}, refernce={1}", retrurnValue, reference), System.Diagnostics.EventLogEntryType.Error);
                    string approveUrl = string.Format("{0}/CancelMargin?reference={1}&marginType={2}", backofficeServiceUrl, reference, type);
                    webRequest = WebRequest.Create(approveUrl);
                    webResponse = webRequest.GetResponse();
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TraderService.ApprovePaymentInstruction", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        internal string[] GetMerchantInfoFor99Bill(Guid[] organizationIds)
        {
            List<string> merchantInfos = new List<string>();

            StringBuilder merchantInfo = new StringBuilder();
            using (SqlConnection sqlConnection = new SqlConnection(this.connectionString))
            {
                sqlConnection.Open();
                foreach (Guid organizationId in organizationIds)
                {
                    merchantInfo.Length = 0;
                    using (SqlCommand sqlCommand = new SqlCommand("dbo.PaymentGateway_Get", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter sqlParameter = sqlCommand.Parameters.Add("@organizationId", SqlDbType.UniqueIdentifier);
                        sqlParameter.Value = organizationId;

                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                merchantInfo.Append(reader["MerchantAcctId"]);
                                merchantInfo.Append("|");
                                merchantInfo.Append(reader["MerchantKey"]);
                                merchantInfo.Append("|");
                                merchantInfo.Append(organizationId);
                            }
                            merchantInfos.Add(merchantInfo.ToString());
                        }
                    }
                }
            }

            return merchantInfos.ToArray();
        }

        internal string[] Get99BillBanks(string language)
        {
            using (SqlConnection sqlConnection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("dbo.BankFor99Bill_Get", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter sqlParameter = sqlCommand.Parameters.Add("@language", SqlDbType.NVarChar);
                    sqlParameter.Value = language;

                    sqlConnection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        List<string> banks = new List<string>();
                        while (reader.Read())
                        {
                            string bank = (string)reader["Code"];
                            bank += "|";
                            bank += (string)reader["Name"];
                            banks.Add(bank);
                        }
                        return banks.ToArray();
                    }
                }
            }
        }

        internal long GetNextOrderNoFor99Bill(string merchantAcctId)
        {
            using (SqlConnection sqlConnection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("dbo.PaymentGateway_GetNextPaySequence", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlConnection.Open();
                    SqlCommandBuilder.DeriveParameters(sqlCommand);
                    sqlCommand.Parameters["@merchantAcctId"].Value = merchantAcctId;
                    sqlCommand.ExecuteNonQuery();
                    int nextSequence = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                    return nextSequence;
                }
            }
        }

        private bool SendEmail(string from, string to, string subject, string body)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("dbo.P_AddSendEmail", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@from", SqlDbType.NVarChar, 255);
                        param.Value = from;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@To", SqlDbType.NVarChar, 4000);
                        param.Value = to;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@Subject", SqlDbType.NVarChar, 255);
                        param.Value = subject;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@Body", SqlDbType.NVarChar, 4000);
                        param.Value = body;
                        command.Parameters.Add(param);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.TradingConsoleServer.SendEmail", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return false;
            }

            return true;
        }

        private bool IsValidAgent(string newAgentCode, string newAgentICNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "Customer_IsValid";
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter param = new SqlParameter("@code", SqlDbType.NVarChar, 50);
                        param.Value = newAgentCode;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@CPIDCardNO", SqlDbType.NVarChar, 30);
                        param.Value = newAgentICNo;
                        command.Parameters.Add(param);

                        param = new SqlParameter("@isValid", SqlDbType.Bit);
                        param.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        return (bool)command.Parameters["@isValid"].Value;
                    }
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.TradingConsoleServer.AddMargin", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw;
            }
        }

        private bool AddMargin(string type, DateTime date, string account, string email, string currency, decimal? amount, string targetAccount,
            string targetName, string targetAddress, string targetEmail, string targetTel, string targetMobile, string targetFax, string bankerName,
            string bankerAddress, string swift, DateTime? targetDate, string remarks, Guid submitPerson, out string reference)
        {
            reference = "";
            try
            {
                MarginType marginType = MarginType.OwnerRegistration;
                string mailTemplateContent = string.Empty;
                switch (type)
                {
                    case "OwnerRegistration":
                        marginType = MarginType.OwnerRegistration;
                        break;
                    case "AgentRegistration":
                        marginType = MarginType.AgentRegistration;
                        break;
                    case "CMExtension":
                        marginType = MarginType.CMExtension;
                        break;
                    case "PI":
                        marginType = MarginType.PI;
                        break;
                    case "PICash":
                        marginType = MarginType.PICash;
                        break;
                    case "PIInterACTransfer":
                        marginType = MarginType.PIInterACTransfer;
                        break;
                }

                iExchange.Common.DataAccess.AddMargin(connectionString, marginType, date, account, email, currency, amount, targetAccount,
                            targetName, targetAddress, targetEmail, targetTel, targetMobile, targetFax, bankerName,
                            bankerAddress, swift, targetDate, remarks, submitPerson, TradingConsoleServer.AllowInstantPayment, out reference);
                return true;
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.TradingConsoleServer.AddMargin", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                throw;
            }
        }

        #region comment
        //private bool Email(Wakisoft.Mail.SmtpMail smtpMail1, string email, string strReceive, string mailTitle, string from, string customerName, string body,
        //    string charSet, string ESMTP_Account, string ESMTP_PassWord, string MailServer)
        //{
        //    bool result = false;

        //    smtpMail1.CharSet = charSet;
        //    smtpMail1.ESMTP_Account = ESMTP_Account;
        //    smtpMail1.ESMTP_PassWord = ESMTP_PassWord;
        //    smtpMail1.MailServer = MailServer;

        //    smtpMail1.Subject = mailTitle;
        //    smtpMail1.From = customerName;
        //    smtpMail1.FromAddr = email;
        //    string[] receive = (strReceive + ",").Split(',');
        //    smtpMail1.AddRecipient(receive, Wakisoft.Mail.SendMode.Normal);
        //    smtpMail1.Body = body;

        //    string s = "";
        //    s += "ESMTP_Account:" + ESMTP_Account + "\r\n";
        //    s += "MailServer:" + MailServer + "\r\n";
        //    s += "Subject:" + mailTitle + "\r\n";
        //    s += "From:" + customerName + "\r\n";
        //    s += "FromAddr:" + email + "\r\n";
        //    s += "Receive:" + strReceive + "\r\n";

        //    try
        //    {
        //        if (smtpMail1.Send())
        //        {
        //            result = true;
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        if (Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["IsDebug"]))
        //        {
        //            s += "Body:" + body + "\r\n";
        //            AppDebug.LogEvent("TradingConsole.Email", s + "\r\n" + ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
        //        }
        //    }
        //    if (Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["IsDebug"]))
        //    {
        //        s += (result) ? "Succeed!!" : "Failed!!" + "\r\n";
        //        AppDebug.LogEvent("TradingConsole.Email", s, System.Diagnostics.EventLogEntryType.Information);
        //    }

        //    return result;
        //}

        //public bool Email2(Token token, Wakisoft.Mail.SmtpMail smtpMail1, string typeDesc, string fromEmail, string toEmails, string subject, string body)
        //{
        //    string charSet = string.Empty;
        //    string ESMTP_Account = string.Empty;
        //    string ESMTP_PassWord = string.Empty;
        //    string MailServer = string.Empty;

        //    if (typeDesc == "Execute")
        //    {
        //        Hashtable extend = (Hashtable)this.extends[token.SessionID];
        //        charSet = extend["SmtpMail1CharSet"].ToString();
        //        ESMTP_Account = extend["SmtpAccount"].ToString();
        //        ESMTP_PassWord = extend["SmtpPassword"].ToString();
        //        MailServer = extend["SmtpServer"].ToString();
        //    }
        //    else
        //    {
        //        charSet = (string)ConfigurationSettings.AppSettings["SmtpMail1CharSet"];
        //        ESMTP_Account = (string)ConfigurationSettings.AppSettings["SmtpAccount"];
        //        ESMTP_PassWord = (string)ConfigurationSettings.AppSettings["SmtpPassword"];
        //        MailServer = (string)ConfigurationSettings.AppSettings["SmtpServer"];
        //    }

        //    //return Email(smtpMail1, fromEmail, toEmails, subject, fromEmail, fromEmail, body, charSet, ESMTP_Account, ESMTP_PassWord, MailServer);
        //    this.SendEmail(fromEmail, toEmails, subject, body);
        //}
        #endregion

        public bool SaveLog(Token token, bool isEmployee, string ip, string objectIDs, DateTime timestamp, string action)
        {
            return this.SaveLog(token, isEmployee, ip, objectIDs, timestamp, action, Guid.Empty);
        }

        public bool SaveLog(Token token, bool isEmployee, string ip, string objectIDs, DateTime timestamp, string action, Guid transactionId)
        {
            return this.SaveLog(string.Empty, token.UserID, isEmployee, ip, UserType.Customer.ToString(), objectIDs, timestamp, action, transactionId);
        }

        private bool SaveLog(string loginName, Guid userID, bool isEmployee, string ip, string role, string objectIDs, DateTime timestamp, string eventCode, Guid transactionId)
        {
            bool isSucced = false;
            try
            {//dbo.P_SaveLogForLoginFail
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                SqlCommand sqlCommand;
                SqlParameter sqlParameter;

                sqlCommand = new SqlCommand(((userID.Equals(Guid.Empty)) ? "dbo.P_SaveLogForLoginFail" : "dbo.P_SaveLog"), sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                if (userID.Equals(Guid.Empty))
                {
                    sqlParameter = sqlCommand.Parameters.Add("@LoginName", SqlDbType.VarChar, 20);
                    sqlParameter.Value = loginName;
                }
                else
                {
                    sqlParameter = sqlCommand.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier);
                    sqlParameter.Value = userID;
                }

                sqlParameter = sqlCommand.Parameters.Add("@IP", SqlDbType.NVarChar, 15);
                sqlParameter.Value = ip;
                sqlParameter = sqlCommand.Parameters.Add("@Role", SqlDbType.NVarChar, 30);
                //sqlParameter.Value = isEmployee ? "Employee" : role;
                sqlParameter.Value = role;
                sqlParameter = sqlCommand.Parameters.Add("@ObjectIDs", SqlDbType.NVarChar, 4000);
                if (string.IsNullOrEmpty(objectIDs))
                {
                    sqlParameter.Value = DBNull.Value;
                }
                else
                {
                    sqlParameter.Value = objectIDs;
                }

                sqlParameter = sqlCommand.Parameters.Add("@Timestamp", SqlDbType.DateTime);
                sqlParameter.Value = timestamp;

                int debugInfoIndex = eventCode.IndexOf("[DebugInfo]=");
                if (debugInfoIndex > 0)
                {
                    string debugInfo = eventCode.Substring(debugInfoIndex + "[DebugInfo]=".Length);
                    sqlParameter = sqlCommand.Parameters.Add("@DebugInfo", SqlDbType.NVarChar, 4000);
                    sqlParameter.Value = debugInfo;

                    eventCode = eventCode.Substring(0, debugInfoIndex);
                }
                sqlParameter = sqlCommand.Parameters.Add("@Event", SqlDbType.NVarChar, 4000);
                sqlParameter.Value = eventCode;


                if (!userID.Equals(Guid.Empty))
                {
                    sqlParameter = sqlCommand.Parameters.Add("@TransactionID", SqlDbType.UniqueIdentifier);
                    if (transactionId.Equals(Guid.Empty))
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        sqlParameter.Value = transactionId;
                    }
                }

                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();

                isSucced = true;
            }
            catch (System.Exception ex)
            {
                isSucced = false;
                AppDebug.LogEvent("TradingConsole.TradingConsoleServer.SaveLog", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return (isSucced);
        }

        public void ActivateAccountPass(Token token)
        {
            string sql = string.Format("Exec dbo.P_ActivateAccountPass '{0}'", token.UserID);
            DataAccess.UpdateDB(sql, this.connectionString);
        }

        private class EventCode
        {
            public static readonly string LoginFail = "LoginFail";
            public static readonly string Logon = "Logon";
            public static readonly string Logout = "Logout";
            public static readonly string Statement = "Statement";
            public static readonly string Ledger = "Ledger";
            public static readonly string Quote = "Quote";
            public static readonly string CancelQuote = "CancelQuote";
            public static readonly string ChangePassword = "ChangePassword";
            public static readonly string Activate = "Activate";
        }

        public bool SaveLoginFail(string loginName, string password, string localIP)
        {
            return (SaveLog(loginName, Guid.Empty, false, localIP, UserType.Customer.ToString(), null, DateTime.Now, EventCode.LoginFail, Guid.Empty));
        }

        public bool SaveLogonLog(Token token, string localIP, string environmentInfo)
        {
            return SaveLog(token, false, localIP, environmentInfo, DateTime.Now, EventCode.Logon);
        }

        public bool SaveLogoutLog(Token token, string localIP, bool isEmployee)
        {
            bool isSucceed = false;
            isSucceed = SaveLog(token, isEmployee, localIP, string.Empty, DateTime.Now, EventCode.Logout);
            return isSucceed;
        }

        public bool SaveStatement(Token token, string localIP, string accountID)
        {
            return true;
            //return (SaveLog(token, localIP, accountID, DateTime.Now, EventCode.Statement));
        }

        public bool SaveLedger(Token token, string localIP, string accountID)
        {
            return true;
            //return (SaveLog(token, localIP, accountID, DateTime.Now, EventCode.Ledger));
        }

        public bool SaveQuote(Token token, bool isEmployee, string localIP, string instrumentID)
        {
            return (SaveLog(token, isEmployee, localIP, "{" + instrumentID.ToUpper() + "}", DateTime.Now, EventCode.Quote));
        }

        public bool SaveCancelQuote(Token token, bool isEmployee, string localIP, string instrumentID)
        {
            return (SaveLog(token, isEmployee, localIP, "{" + instrumentID.ToUpper() + "}", DateTime.Now, EventCode.CancelQuote));
        }

        public bool SaveChangePasswordLog(Token token, bool isEmployee, string localIP)
        {
            return (SaveLog(token, isEmployee, localIP, string.Empty, DateTime.Now, EventCode.ChangePassword));
        }

        public bool SaveActivateLog(Token token, bool isEmployee, string localIP)
        {
            return (SaveLog(token, isEmployee, localIP, string.Empty, DateTime.Now, EventCode.Activate));
        }

        private void GetExtend(Token token, string physicalPath)
        {
            if (this.extends.ContainsKey(token.SessionID)) return;

            Hashtable extend = new Hashtable();
            string usedExtendXml = ConfigurationManager.AppSettings["UsedExtendXml"];

            //Get xml
            try
            {
                string xmlPath = physicalPath + "Setting\\" + ((usedExtendXml == null) ? "Extend.xml" : usedExtendXml);

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);
                System.Xml.XmlNode node = doc.GetElementsByTagName("Extend")[0];
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    string nodeName = node.ChildNodes.Item(i).Name.ToString();
                    string text = node.ChildNodes.Item(i).InnerXml.ToString();
                    extend[nodeName] = text;
                }
                this.extends.Add(token.SessionID, extend);
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.TradingConsoleServer.GetExtend", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        //for Java
        //		private void GetExtend(Token token,XmlNode node)
        //		{
        //			if (this.extends.ContainsKey(token.SessionID)) return;
        //			
        //			Hashtable extend = new Hashtable();
        //
        //			//Get xml
        //			try
        //			{
        //				for (int i = 0; i < node.ChildNodes.Count; i++)
        //				{						
        //					string nodeName = node.ChildNodes.Item(i).Name.ToString();
        //					string text = node.ChildNodes.Item(i).InnerText.ToString();
        //					extend[nodeName] = text;
        //				}
        //				this.extends.Add(token.SessionID,extend);
        //			}
        //			catch(System.Exception ex)
        //			{
        //				AppDebug.LogEvent("TradingConsole.TradingConsoleServer.GetExtend(for Java)",ex.ToString(),System.Diagnostics.EventLogEntryType.Error);
        //			}			
        //		}



        public void EmailExecuteOrders(Token token, StateServerService stateServer, string physicalPath, XmlElement xmlElement)
        {
            this.GetExtend(token, physicalPath);

            Hashtable extend = (Hashtable)this.extends[token.SessionID];
            try
            {
                XmlElement xmlTran = (XmlElement)xmlElement.GetElementsByTagName("Transaction")[0];
                Guid transactionID = XmlConvert.ToGuid(xmlTran.Attributes["ID"].Value);
                string accountIDString = xmlTran.Attributes["AccountID"].Value;
                Guid accountID = XmlConvert.ToGuid(accountIDString);
                string instrumentIDString = xmlTran.Attributes["InstrumentID"].Value;
                Guid instrumentID = XmlConvert.ToGuid(instrumentIDString);
                string executeTime = xmlTran.Attributes["ExecuteTime"].Value;

                //Create execute orders
                string separator = "<p>";
                string executeOrders = string.Empty;
                executeOrders += extend["accountCode"].ToString();
                executeOrders += accountIDString;
                executeOrders += ":";
                executeOrders += separator;

                string xmlOrderRelations = string.Empty;
                string orderCodes = string.Empty;
                foreach (XmlNode xmlOrder in xmlTran.ChildNodes)
                {
                    Guid closeOrderID = XmlConvert.ToGuid(xmlOrder.Attributes["ID"].Value);
                    string orderCode = xmlOrder.Attributes["Code"].Value;
                    orderCodes = (orderCodes == string.Empty) ? orderCode : orderCodes + "," + orderCode;
                    string lot = xmlOrder.Attributes["Lot"].Value;
                    string buySell = (XmlConvert.ToBoolean(xmlOrder.Attributes["IsBuy"].Value)) ? "B" : "S";
                    string executePrice = xmlOrder.Attributes["ExecutePrice"].Value;
                    bool isOpen = XmlConvert.ToBoolean(xmlOrder.Attributes["IsOpen"].Value);
                    string newClose = isOpen ? "N" : "C";

                    string openPrice = string.Empty;
                    foreach (XmlNode xmlOrderRelation in xmlOrder.ChildNodes)
                    {
                        Guid openOrderID = XmlConvert.ToGuid(xmlOrderRelation.Attributes["OpenOrderID"].Value);
                        openPrice += ((openPrice != string.Empty) ? ";" : "") + closeOrderID.ToString() + openOrderID.ToString();

                        xmlOrderRelations += "<OrderRelation ";
                        xmlOrderRelations += "CloseOrderID=\"" + closeOrderID.ToString() + "\" ";
                        xmlOrderRelations += "OpenOrderID=\"" + openOrderID.ToString() + "\" ";
                        xmlOrderRelations += "/>";
                    }

                    executeOrders += separator;
                    executeOrders += extend["orderCode"].ToString();
                    executeOrders += orderCode;
                    executeOrders += extend["instrumentCode"].ToString();
                    executeOrders += instrumentIDString;
                    executeOrders += extend["lot"].ToString();
                    executeOrders += lot;
                    executeOrders += extend["buySell"].ToString();
                    executeOrders += buySell;
                    executeOrders += extend["executePrice"].ToString();
                    executeOrders += executePrice;
                    executeOrders += extend["executeTime"].ToString();
                    executeOrders += executeTime;
                    executeOrders += extend["newClose"].ToString();
                    executeOrders += newClose;
                    executeOrders += extend["openPrice"].ToString();
                    executeOrders += openPrice;
                }

                //Get missing datas
                bool isSendOrderMail = false;
                string organizationName = string.Empty;
                string organizationEmail = string.Empty;
                string customerEmail = string.Empty;
                string agentEmail = string.Empty;
                string accountCode = string.Empty;
                string instrumentCode = string.Empty;

                string sql = string.Empty;
                if (xmlOrderRelations != string.Empty)
                {
                    xmlOrderRelations = "<OrderRelations>" + xmlOrderRelations + "</OrderRelations>";
                    sql = string.Format("Exec dbo.P_GetInfoForEmailExecuteOrder '{0}','{1}','{2}','{3}'", token.UserID, accountID, instrumentID, xmlOrderRelations);
                }
                else
                {
                    sql = string.Format("Exec dbo.P_GetInfoForEmailExecuteOrder '{0}','{1}','{2}'", token.UserID, accountID, instrumentID);
                }

                DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
                DataTable dt = dataSet.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    isSendOrderMail = (bool)dt.Rows[i]["IsSendOrderMail"];
                    if (!isSendOrderMail) return;

                    organizationName = dt.Rows[i]["OrganizationName"].ToString();
                    organizationEmail = dt.Rows[i]["OrganizationEmail"].ToString();
                    customerEmail = dt.Rows[i]["CustomerEmail"].ToString();
                    agentEmail = dt.Rows[i]["AgentEmail"].ToString();
                    accountCode = dt.Rows[i]["AccountCode"].ToString();
                    instrumentCode = dt.Rows[i]["InstrumentCode"].ToString();
                }
                if (dataSet.Tables.Count == 2)
                {
                    dt = dataSet.Tables[1];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        executeOrders = executeOrders.Replace(dt.Rows[i]["CloseOrderID"].ToString() + dt.Rows[i]["OpenOrderID"].ToString(),
                            dt.Rows[i]["ClosedLot"].ToString() + extend["RelationLot"] + "*" + dt.Rows[i]["ExecutePrice"]);
                    }
                }
                if (organizationEmail != string.Empty && (customerEmail != string.Empty || agentEmail != string.Empty))
                {
                    executeOrders = executeOrders.Replace(accountIDString, accountCode);
                    executeOrders = executeOrders.Replace(instrumentIDString, instrumentCode);

                    //Create body				
                    string body = string.Empty;
                    body = extend["appellation"].ToString();
                    body += separator;
                    body += extend["message1"].ToString();
                    body += separator;
                    body += executeOrders;
                    body += separator;
                    body += extend["message2"].ToString();
                    body += separator;
                    body += extend["message3"].ToString();
                    body += separator;
                    body += extend["message4"].ToString();
                    body += extend["message5"].ToString();
                    body += organizationName;

                    //AppDebug.LogEvent("TradingConsole.EmailExecuteOrders",body,System.Diagnostics.EventLogEntryType.Information);

                    string toEmails = string.Empty;
                    if (customerEmail != string.Empty)
                    {
                        toEmails = customerEmail;
                    }
                    if (agentEmail != string.Empty)
                    {
                        toEmails = (toEmails != string.Empty) ? toEmails + "," + agentEmail : agentEmail;
                    }
                    string subject = extend["subject1"].ToString() + " " + orderCodes + " " + extend["subject2"].ToString() + " " + accountCode;
                    //stateServer.Email(token, "Execute", transactionID, organizationEmail, toEmails, subject, body);
                    this.SendEmail(organizationEmail, toEmails, subject, body);

                }
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.EmailExecuteOrders", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        //for Java
        //		public void NotifyCustomerExecuteOrder(Token token,StateServerService stateServer,string[][] parameters,XmlNode extendXml)
        //		{
        //			this.GetExtend(token,extendXml);
        //
        //			this.NotifyCustomerExecuteOrder(token,stateServer,parameters);
        //		}

        public void NotifyCustomerExecuteOrder(Token token, StateServerService stateServer, string physicalPath, string[][] parameters)
        {
            this.GetExtend(token, physicalPath);

            string usedExtendXml = ConfigurationManager.AppSettings["UsedExtendXml"];
            if (usedExtendXml == "Extend2.xml")
            {
                this.NotifyCustomerExecuteOrder2(token, stateServer, parameters);
            }
            else
            {
                this.NotifyCustomerExecuteOrder(token, stateServer, parameters);
            }
        }

        public void NotifyCustomerExecuteOrder2(Token token, StateServerService stateServer, string[][] parameters)
        {
            Hashtable extend = (Hashtable)this.extends[token.SessionID];
            try
            {
                foreach (string[] parameters2 in parameters)
                {
                    int i = 0;
                    Guid transactionID = new Guid(parameters2[i++]);
                    Guid orderID = new Guid(parameters2[i++]);
                    string orderCode = parameters2[i++];
                    string accountCode = parameters2[i++];
                    string instrumentCode = parameters2[i++];
                    string lot = parameters2[i++];
                    string buySell = parameters2[i++];
                    bool isBuy = (buySell == "B");
                    string buySell2 = buySell;
                    buySell = buySell.Replace("B", extend["BuyPrompt"].ToString());
                    buySell = buySell.Replace("S", extend["SellPrompt"].ToString());
                    buySell2 = buySell2.Replace("B", extend["BuyPrompt2"].ToString());
                    buySell2 = buySell2.Replace("S", extend["SellPrompt2"].ToString());
                    string executePrice = parameters2[i++];
                    string executeTime = parameters2[i++];
                    string executeTradeDay = parameters2[i++];
                    string newClose = parameters2[i++];
                    string newClose2 = newClose;
                    newClose = newClose.Replace("C", extend["ClosePrompt"].ToString());
                    newClose = newClose.Replace("N", extend["NewPrompt"].ToString());
                    newClose2 = newClose2.Replace("C", extend["ClosePrompt2"].ToString());
                    newClose2 = newClose2.Replace("N", extend["NewPrompt2"].ToString());
                    string openPrice = parameters2[i++];
                    openPrice = openPrice.Replace("Lot", extend["RelationLot"].ToString());
                    string organizationName = parameters2[i++];
                    string organizationEmail = parameters2[i++];
                    string customerEmail = parameters2[i++];
                    string agentEmail = parameters2[i++];
                    string tradeDay = executeTradeDay;

                    bool isOpen = (buySell == "N");
                    string accountAlias = "&nbsp;";
                    string accountName = "&nbsp;";
                    int deliveryDay = 0;
                    string orderRelationOpenOrderExecutePrice = "&nbsp;";
                    string orderRelationOpenOrderExecuteTime = "&nbsp;";

                    string commission = "&nbsp;";
                    string orderRelationRateInOut = "&nbsp;";
                    string tradePL = "&nbsp;";
                    string accountCurrencyAlias = "&nbsp;";
                    string accountCurrencyName = "&nbsp;";
                    string contractMarginO = "&nbsp;";
                    decimal contractSize = 0;

                    string sql = string.Format("Exec dbo.P_GetEmailExecuteInfo2 '{0}'", orderID);
                    DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
                    DataTable dt = dataSet.Tables[0];
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        accountAlias = Convert.ToString(dt.Rows[j]["AccountAlias"]);
                        accountName = Convert.ToString(dt.Rows[j]["AccountName"]);
                        deliveryDay = Convert.ToInt32(dt.Rows[j]["DeliveryDay"].ToString());
                        orderRelationOpenOrderExecutePrice = Convert.ToString(dt.Rows[j]["OrderRelationOpenOrderExecutePrice"]);
                        orderRelationOpenOrderExecuteTime = Convert.ToString(dt.Rows[j]["OrderRelationOpenOrderExecuteTime"]);
                        commission = Convert.ToString(dt.Rows[j]["Commission"]);
                        orderRelationRateInOut = Convert.ToString(dt.Rows[j]["OrderRelationRateInOut"]);
                        tradePL = dt.Rows[j]["TradePL"].ToString();
                        accountCurrencyAlias = Convert.ToString(dt.Rows[j]["AccountCurrencyAlias"]);
                        accountCurrencyName = Convert.ToString(dt.Rows[j]["AccountCurrencyName"]);
                        contractMarginO = dt.Rows[j]["ContractMarginO"].ToString();
                        contractSize = Convert.ToDecimal(dt.Rows[j]["ContractSize"].ToString());
                    }
                    double contractAmount = (double)contractSize * Convert.ToDouble(lot);

                    //Create body
                    string body = string.Empty;
                    body = extend["body"].ToString();

                    body = body.Replace("@AccountCode", accountCode);
                    body = body.Replace("@AccountAlias", accountAlias + "&nbsp;");
                    body = body.Replace("@AccountName", accountName);
                    body = body.Replace("@TradeDay", tradeDay);
                    body = body.Replace("@OrderCode", orderCode);
                    body = body.Replace("@DeliveryDay", Convert.ToString(deliveryDay));
                    body = body.Replace("@InstrumentCode", instrumentCode);
                    body = body.Replace("@NewClose2", newClose2);
                    body = body.Replace("@NewClose", newClose);
                    body = body.Replace("@BuySell2", buySell2);
                    body = body.Replace("@BuySell", buySell);
                    body = body.Replace("@OrderRelationOpenOrderExecutePrice", orderRelationOpenOrderExecutePrice + "&nbsp;");
                    body = body.Replace("@OrderRelationOpenOrderExecuteTime", orderRelationOpenOrderExecuteTime + "&nbsp;");
                    body = body.Replace("@ExecuteTime", executeTime);
                    body = body.Replace("@Lot", lot);
                    body = body.Replace("@Commission", commission);
                    body = body.Replace("@ContractAmount", Convert.ToString(contractAmount));
                    body = body.Replace("@OrderRelationRateInOut", orderRelationRateInOut);
                    body = body.Replace("@ExecutePrice", executePrice);
                    body = body.Replace("@TradePL", tradePL);
                    body = body.Replace("@AccountCurrencyAlias", accountCurrencyAlias + "&nbsp;");
                    body = body.Replace("@AccountCurrencyName", accountCurrencyName);
                    body = body.Replace("@ContractMarginO", contractMarginO);

                    body = body.Replace("\n", "");
                    body = body.Replace("\t", "");
                    body = body.Replace("\r", "");
                    body = body.Replace("@Empty", "&nbsp;");
                    body = body.Replace("<BR></BR>", "<BR>");
                    body = body.Replace("\"", "'");

                    string toEmails = string.Empty;
                    if (customerEmail != string.Empty)
                    {
                        toEmails = customerEmail;
                    }
                    if (agentEmail != string.Empty)
                    {
                        toEmails = (toEmails != string.Empty) ? toEmails + "," + agentEmail : agentEmail;
                    }
                    string subject = extend["subject1"].ToString() + " " + orderCode + " " + extend["subject2"].ToString() + " " + accountCode;
                    //stateServer.Email(token, "Execute", transactionID, organizationEmail, toEmails, subject, body);
                    this.SendEmail(organizationEmail, toEmails, subject, body);
                }
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.NotifyCustomerExecuteOrder2", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public void NotifyCustomerExecuteOrder(Token token, StateServerService stateServer, string[][] parameters)
        {
            //this.GetExtend(token,physicalPath);
            try
            {
                Hashtable extend = (Hashtable)this.extends[token.SessionID];
                foreach (string[] parameters2 in parameters)
                {
                    int i = 0;
                    Guid transactionID = new Guid(parameters2[i++]);
                    Guid orderID = new Guid(parameters2[i++]);
                    string orderCode = parameters2[i++];
                    string accountCode = parameters2[i++];
                    string instrumentCode = parameters2[i++];
                    string lot = parameters2[i++];
                    string buySell = parameters2[i++];
                    bool isBuy = (buySell == "B");
                    buySell = buySell.Replace("B", extend["BuyPrompt"].ToString());
                    buySell = buySell.Replace("S", extend["SellPrompt"].ToString());
                    string executePrice = parameters2[i++];
                    string executeTime = parameters2[i++];
                    string executeTradeDay = parameters2[i++];
                    string newClose = parameters2[i++];
                    newClose = newClose.Replace("C", extend["ClosePrompt"].ToString());
                    newClose = newClose.Replace("N", extend["NewPrompt"].ToString());
                    string openPrice = parameters2[i++];
                    openPrice = openPrice.Replace("Lot", extend["RelationLot"].ToString());
                    string organizationName = parameters2[i++];
                    string organizationEmail = parameters2[i++];
                    string customerEmail = parameters2[i++];
                    string agentEmail = parameters2[i++];
                    string tradeDay = executeTradeDay;

                    string contractCurrency = "";
                    string baseCurrency = "";
                    string customerName = "";
                    string organizationAlias = "";
                    string deliveryDay = "";
                    decimal contractSize = 0;

                    string sql = string.Format("Exec dbo.P_GetEmailExecuteInfo '{0}'", orderID);
                    DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
                    DataTable dt = dataSet.Tables[0];
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        contractCurrency = dt.Rows[j]["ContractCurrency"].ToString();
                        baseCurrency = dt.Rows[j]["BaseCurrency"].ToString();
                        deliveryDay = dt.Rows[j]["DeliveryDay"].ToString();
                        organizationAlias = dt.Rows[j]["OrganizationAlias"].ToString();
                        customerName = dt.Rows[j]["CustomerName"].ToString();
                        contractSize = Convert.ToDecimal(dt.Rows[j]["ContractSize"].ToString());
                    }
                    double contractAmount = (double)contractSize * Convert.ToDouble(lot);
                    double contractValue = (double)contractSize * Convert.ToDouble(lot) * Convert.ToDouble(executePrice);

                    //Create body
                    string body = string.Empty;
                    body = extend["body"].ToString();

                    body = body.Replace("OrderCode", orderCode);
                    body = body.Replace("InstrumentCode", instrumentCode);
                    body = body.Replace("LotBalance", lot);
                    body = body.Replace("BuySell", buySell);
                    body = body.Replace("ExecuteTime", executeTime);
                    body = body.Replace("NewClose", newClose);
                    body = body.Replace("OpenPrice", openPrice);
                    body = body.Replace("OrganizationName", organizationName);

                    body = body.Replace("OrganizationEmail", organizationEmail);
                    body = body.Replace("CustomerEmail", customerEmail);
                    body = body.Replace("AgentEmail", agentEmail);

                    body = body.Replace("CustomerName", customerName);
                    body = body.Replace("OrganizationAlias", organizationAlias);
                    body = body.Replace("AccountCode", accountCode);
                    body = body.Replace("CustomerEmail", customerEmail);
                    body = body.Replace("ContractCurrencyCode", ((isBuy) ? contractCurrency : baseCurrency));
                    body = body.Replace("ContractAmount", contractAmount.ToString());
                    body = body.Replace("BaseCurrency", ((isBuy) ? baseCurrency : contractCurrency));
                    body = body.Replace("ContractValue", contractValue.ToString());
                    body = body.Replace("ExecutePrice", executePrice);
                    body = body.Replace("TradeDay", tradeDay);
                    body = body.Replace("DeliveryDay", deliveryDay);

                    body = body.Replace("\n", "");
                    body = body.Replace("\t", "");
                    body = body.Replace("\r", "");

                    string toEmails = string.Empty;
                    if (customerEmail != string.Empty)
                    {
                        toEmails = customerEmail;
                    }
                    if (agentEmail != string.Empty)
                    {
                        toEmails = (toEmails != string.Empty) ? toEmails + "," + agentEmail : agentEmail;
                    }
                    string subject = extend["subject1"].ToString() + " " + orderCode + " " + extend["subject2"].ToString() + " " + accountCode;
                    //stateServer.Email(token, "Execute", transactionID, organizationEmail, toEmails, subject, body);
                    this.SendEmail(organizationEmail, toEmails, subject, body);
                }
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.NotifyCustomerExecuteOrder", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        /*
        private bool SendEmail(string subject,string sendemailaddress,string to,string body,
            string fromEmailaddress,string smtpaccountname,string sendusername,string sendpassword,int smtpauthenticate,string smtpserver,string charset)
        {
            bool isSucceed = false;
            try
            {
                CDO.Message oMsg = new CDO.Message();
                oMsg.From = fromEmailaddress;
                oMsg.To = to;
                oMsg.Subject = subject;
                oMsg.HTMLBody = "<html><body>" + body + "</body></html>";
                CDO.IConfiguration iConfg = oMsg.Configuration;
                ADODB.Fields oFields = iConfg.Fields;
          
                oFields["http://schemas.microsoft.com/cdo/configuration/sendusing"].Value=2;
                oFields["http://schemas.microsoft.com/cdo/configuration/sendemailaddress"].Value=sendemailaddress; //sender mail
                oFields["http://schemas.microsoft.com/cdo/configuration/smtpaccountname"].Value=smtpaccountname;//email account
                oFields["http://schemas.microsoft.com/cdo/configuration/sendusername"].Value=sendusername;
                oFields["http://schemas.microsoft.com/cdo/configuration/sendpassword"].Value=sendpassword;
                oFields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"].Value=smtpauthenticate;
                //value=0 代表Anonymous验证方式（不需要验证）
                //value=1 代表Basic验证方式（使用basic (clear-text) authentication. 
                //The configuration sendusername/sendpassword or postusername/postpassword fields are used to specify credentials.）
                //Value=2 代表NTLM验证方式（Secure Password Authentication in Microsoft Outlook Express）
                //				oFields["http://schemas.microsoft.com/cdo/configuration/languagecode"].Value=0x0412;//0x0804;

                oFields["http://schemas.microsoft.com/cdo/configuration/smtpserver"].Value=smtpserver;
				
                oFields.Update();
                oMsg.BodyPart.Charset=charset;
                oMsg.HTMLBodyPart.Charset=charset; 

                oMsg.Send();
                oMsg = null;

                //				string mail="<HTML><HEAD></HEAD><BODY onload='window.frmEmail.submit();'>" +
                //					"<FORM name='frmEmail' ACTION='mailto:" + to + "?subject=" + subject + "' METHOD='POST' ENCTYPE='text/plain'>" +						
                //					"<TEXTAREA style='display=none' NAME=CONTENT COLS=40>" +
                //					body +
                //					"</TEXTAREA>" +
                //					"</FORM></BODY></HTML>";
                //				this.Context.Response.Write(mail);

                isSucceed = true;
            }
            catch(System.Exception ex)
            {}
            return isSucceed;
        }
*/
        /*
        public DataSet LoadSystemParameters(Token token)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                SqlCommand sqlCommand = new SqlCommand("P_GetSettings2",sqlConnection);
                sqlCommand.CommandType=CommandType.StoredProcedure;
                SqlParameter sqlParameter = sqlCommand.Parameters.Add("@userID",SqlDbType.UniqueIdentifier);
                sqlParameter.Value = token.UserID;
                sqlParameter = sqlCommand.Parameters.Add("@appType",SqlDbType.Int);
                sqlParameter.Value = (Int32)token.AppType;

                sqlConnection.Open();
                SqlDataAdapter sqlDataAdapter=new SqlDataAdapter(sqlCommand);
			
                sqlDataAdapter.Fill(dataSet);
                sqlConnection.Close();
            }
            catch
            {
            }
            return dataSet;
        }
        */

        public bool UpdateSystemParameters(Token token, string parameters, string objectID)
        {
            try
            {
                return this.SetSystemParameter(token, objectID, parameters);
            }
            catch
            {
                return false;
            }
        }

        /*
        private string GetSystemParameter(Token token)
        {
            string parameters = "";
            string sql=string.Format("Exec dbo.P_GetSettings2 '{0}','{1}'",token.UserID, (Int32)token.AppType);
            SqlCommand command=new SqlCommand(sql,new SqlConnection(connectionString));
            try
            { 
                command.Connection.Open();
                parameters = command.ExecuteScalar().ToString();
            }
            finally
            {
                if(command.Connection.State==ConnectionState.Open)
                {
                    command.Connection.Close();
                }
            }			
            return parameters;
        }
        */

        private bool SetSystemParameter(Token token, string objectID, string parameters)
        {
            string sql = string.Format("Exec dbo.P_SetSettings2 '{0}','{1}','{2}','{3}'", token.UserID, (Int32)token.AppType, objectID, parameters);
            return DataAccess.UpdateDB(sql, this.connectionString);
        }

        public void SaveIsCalculateFloat(Token token, bool isCalculateFloat)
        {
            bool isOK = SaveIsCalculateFloat2(token, isCalculateFloat);
        }

        private bool SaveIsCalculateFloat2(Token token, bool isCalculateFloat)
        {
            string sql = string.Format("Exec dbo.P_Customer_Upd2 '{0}','{1}'", token.UserID, (isCalculateFloat) ? 1 : 0);
            return DataAccess.UpdateDB(sql, this.connectionString);
        }

        public System.Data.DataSet GetNewsList(string newsCategoryID, string language, DateTime date)
        {
            string sql;
            if (newsCategoryID == "")
            {
                sql = "SELECT * FROM FT_GetNewsList(NULL,'" + language + "','" + date.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            }
            else
            {
                sql = "SELECT * FROM FT_GetNewsList('" + newsCategoryID + "','" + language + "','" + date.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            }
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            if (dataSet != null && dataSet.Tables.Count > 0)
                dataSet.Tables[0].TableName = "News";
            return dataSet;
        }


        public System.Data.DataSet GetNewsList2(string newsCategoryID, string newslanguage, DateTime date)
        {
            string sql;
            string initialNewsCount = null;
            try
            {
                initialNewsCount =ConfigurationManager.AppSettings["InitialNewsCount"];
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetNewsList2: GetInitialNewsCount", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            if (newsCategoryID == "")
            {
                sql = "SELECT * FROM FT_GetNewsList2(NULL,'" + newslanguage + "','" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','" + initialNewsCount + "')";
            }
            else
            {
                sql = "SELECT * FROM FT_GetNewsList2('" + newsCategoryID + "','" + newslanguage + "','" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','" + initialNewsCount + "')";
            }
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                dataSet.Tables[0].TableName = "News";
                if (newslanguage.Trim().ToLower() == "chs")
                {
                    try
                    {
                        foreach (DataRow row in dataSet.Tables["News"].Rows)
                        {
                            string newsTitle = (string)row["Title"];
                            row["Title"] = EncodingHelper.ConvertToSimpleChinese(newsTitle);
                        }
                    }
                    catch (System.Exception exception)
                    {
                        AppDebug.LogEvent("TradingConsole.GetNewsList2", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                    }
                }
            }
            return dataSet;
        }


        public DataSet GetNewsContents(string newsID)
        {
            string sql = "SELECT * FROM FT_GetNewsContents('" + newsID + "')";
            return DataAccess.GetData(sql, this.connectionString);
        }

        public DataSet GetNewsContents(string newsID, string newsLanguage)
        {
            string sql = "SELECT * FROM FT_GetNewsContents('" + newsID + "')";
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                try
                {
                    string language = ((string)dataSet.Tables[0].Rows[0]["Language"]).Trim().ToLower();
                    string newsContents = (string)dataSet.Tables[0].Rows[0]["Contents"];
                    if (newsLanguage == "chs" && language == "cht")
                    {
                        dataSet.Tables[0].Rows[0]["Contents"] = EncodingHelper.ConvertToSimpleChinese(newsContents);
                    }
                    else if (newsLanguage == "cht" && language.ToLower() == "chs")
                    {
                        dataSet.Tables[0].Rows[0]["Contents"] = EncodingHelper.ConvertToTraditionalChinese(newsContents);
                    }
                }
                catch (System.Exception exception)
                {
                    AppDebug.LogEvent("TradingConsole.GetNewsContents", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                }
            }
            return dataSet;
        }

        public DataSet GetInterestRate(Guid[] orderIds)
        {
            string xmlOrderIds = "<Orders>";
            foreach (Guid orderId in orderIds)
            {
                xmlOrderIds += "<Order ID=\"" + orderId.ToString() + "\" />";
            }
            xmlOrderIds += "</Orders>";
            string sql = string.Format("Exec dbo.P_GetInterestRate '{0}'", xmlOrderIds);
            return DataAccess.GetData(sql, this.connectionString);
        }

        public DataSet GetInterestRate2(Token token, Guid interestRateId)
        {
            string sql = string.Format("Exec dbo.P_GetInterestRate2 '{0}','{1}'", token.UserID, interestRateId);
            return DataAccess.GetData(sql, this.connectionString);
        }

       

        private DataSet GetNewsCategory()
        {
            string sql = "SELECT * FROM NewsCategory ORDER BY [Name]";
            return DataAccess.GetData(sql, this.connectionString);
        }

       

        public Decimal RefreshAgentAccountOrder(string orderID)
        {
            Decimal lotBalance = -1;

            DataSet dataSet = new DataSet();
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            string sql = "SELECT LotBalance FROM [Order] WHERE [ID] = '" + orderID + "'";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlDataAdapter.SelectCommand = sqlCommand;
            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables.Count > 0)
            {
                DataTable table = dataSet.Tables[0];
                DataRowCollection rows = table.Rows;
                foreach (DataRow row in rows)
                {
                    lotBalance = System.Convert.ToDecimal(row["LotBalance"].ToString());
                }
            }
            else
            {
                lotBalance = 0;
            }
            sqlConnection.Close();

            return lotBalance;
        }

        public Guid[] VerifyTransaction(Token token, StateServerService stateServer, Guid[] transactionIDs)
        {
            return stateServer.VerifyTransaction(token, transactionIDs);
        }

        public DataSet GetTradingAccountData(Guid userId)
        {
            string sql = string.Format("EXEC dbo.GetTradingAccountData '{0}'", userId);
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            return dataSet;
        }

        public DataSet GetRecoverPasswordData(string version, Guid userId)
        {
            string sql = string.Format("EXEC dbo.P_GetRecoverPasswordData '{0}','{1}'", version, userId);
            DataSet dataSet = DataAccess.GetData(sql, this.connectionString);
            if (dataSet != null && dataSet.Tables.Count > 1)
            {
                dataSet.Tables[0].TableName = "RecoverPasswordQuestion";
                dataSet.Tables[1].TableName = "RecoverPasswordAnswer";
            }
            return dataSet;
        }

        public void UpdateRecoverPasswordData(Guid userID, string[][] recoverPasswordDatas)
        {
            StringBuilder sql = new StringBuilder();

            string sql2 = string.Format("DELETE dbo.RecoverPasswordAnswer WHERE UserId = '{0}'\n", userID);
            sql.Append(sql2);

            foreach (string[] recoverPasswordDatas2 in recoverPasswordDatas)
            {
                int sequence = int.Parse(recoverPasswordDatas2[0]);
                string questionId = recoverPasswordDatas2[1];
                string answer = recoverPasswordDatas2[2];

                sql2 = string.Format("INSERT INTO dbo.RecoverPasswordAnswer(UserId,Sequence,QuestionId,Answer) VALUES('{0}',{1},'{2}',N'{3}')", userID, sequence, questionId, answer);
                sql.Append(sql2);
            }
            DataAccess.UpdateDB(sql.ToString(), this.connectionString);
        }

        internal void ClearDailyFixLastPeriodCache()
        {
            lock (this._FixLastPeriodCacheLock)
            {
                this._FixLastPeriodCache.Clear();
            }
        }

        internal void ClearDailyFixLastPeriodCache(Guid instrumentId)
        {
            lock (this._FixLastPeriodCacheLock)
            {
                List<string> shouldRemove = new List<string>();
                string instrumentIdKey = instrumentId.ToString();
                foreach (string key in this._FixLastPeriodCache.Keys)
                {
                    if (key.StartsWith(instrumentIdKey) && key.EndsWith("Daily"))
                    {
                        shouldRemove.Add(key);
                    }
                }

                foreach (string key in shouldRemove)
                {
                    this._FixLastPeriodCache.Remove(key);
                }
            }
        }

        internal static void SetAllowInstantPayment(bool allowInstantPayment)
        {
            TradingConsoleServer.AllowInstantPayment = allowInstantPayment;
        }

        internal bool ChangeMarginPin(Guid accountId, string oldPassword, string newPassword)
        {
            SHA1Managed sha1 = new SHA1Managed();
            byte[] oldPassword2 = sha1.ComputeHash(System.Text.Encoding.Unicode.GetBytes(oldPassword));
            byte[] newPassword2 = sha1.ComputeHash(System.Text.Encoding.Unicode.GetBytes(newPassword));

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("dbo.P_ChangeMarginPin", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlConnection.Open();
                    SqlCommandBuilder.DeriveParameters(sqlCommand);
                    sqlCommand.Parameters["@accountId"].Value = accountId;
                    sqlCommand.Parameters["@oldPassword"].Value = oldPassword2;
                    sqlCommand.Parameters["@newPassword"].Value = newPassword2;

                    sqlCommand.ExecuteNonQuery();

                    int result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                    return result == 0;
                }
            }
        }

        internal bool VerifyMarginPin(Guid accountId, string password)
        {
            SHA1Managed sha1 = new SHA1Managed();
            byte[] password2 = sha1.ComputeHash(System.Text.Encoding.Unicode.GetBytes(password));

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("dbo.P_VerifyMarginPin", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlConnection.Open();
                    SqlCommandBuilder.DeriveParameters(sqlCommand);
                    sqlCommand.Parameters["@accountId"].Value = accountId;
                    sqlCommand.Parameters["@password"].Value = password2;

                    sqlCommand.ExecuteNonQuery();

                    int result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                    return result == 0;
                }
            }
        }

        internal DataSet OrderQuery(string language, Guid customerId, string accountId, string instrumentId, int lastDays)
        {
            using (SqlConnection sqlConnection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("dbo.Order_Query", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlConnection.Open();
                    SqlCommandBuilder.DeriveParameters(sqlCommand);
                    sqlCommand.Parameters["@language"].Value = language;
                    sqlCommand.Parameters["@customerId"].Value = customerId;
                    sqlCommand.Parameters["@lastDays"].Value = lastDays;
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        sqlCommand.Parameters["@accountId"].Value = new Guid(accountId);
                    }
                    else
                    {
                        sqlCommand.Parameters["@accountId"].Value = null;
                    }

                    if (!string.IsNullOrEmpty(instrumentId))
                    {
                        sqlCommand.Parameters["@instrumentId"].Value = new Guid(instrumentId);
                    }
                    else
                    {
                        sqlCommand.Parameters["@instrumentId"].Value = null;
                    }

                    DataSet dataSet = new DataSet();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(dataSet);
                    sqlCommand.Dispose();
                    sqlConnection.Close();

                    return dataSet;
                }
            }



        }

        public bool AdditionalClient(Token token, StateServerService stateServer, string email, string receive, string organizationName, string customerName,
            string reportDate, string accountCode, string correspondingAddress, string registratedEmailAddress, string tel, string mobile,
            string fax, string fillName1, string ICNo1, string fillName2, string ICNo2, string fillName3, string ICNo3, out string reference)
        {
            Guid submitPerson = token.UserID;
            string remark = ConfigurationManager.AppSettings["FillName1"] + fillName1 + "," + ConfigurationManager.AppSettings["ICNo1"] + ICNo1 + ";"
                + ConfigurationManager.AppSettings["FillName2"] + fillName2 + "," + ConfigurationManager.AppSettings["ICNo2"] + ICNo2 + ";"
                + ConfigurationManager.AppSettings["FillName3"] + fillName3 + "," + ConfigurationManager.AppSettings["ICNo3"] + ICNo3 + ";"
                + ConfigurationManager.AppSettings["From"] + customerName;

            return this.AddMargin("OwnerRegistration", DateTime.Parse(reportDate), accountCode, email, null, null, null, organizationName, correspondingAddress,
                registratedEmailAddress, tel, mobile, fax, null, null, null, null, remark, submitPerson, out reference);
        }


        public bool Agent(Token token, StateServerService stateServer, string email, string receive, string organizationName, string customerName,
           string reportDate, string accountCode, string previousAgentCode, string previousAgentName, string newAgentCode,
           string newAgentName, string newAgentICNo, string dateReply, out string reference, out string errorMessage)
        {
            if (!this.IsValidAgent(newAgentCode, newAgentICNo))
            {
                reference = "";
                errorMessage = "New agent doesn't exist, please check the code and IC No. of new agent";
                return false;
            }

            errorMessage = "";
            Guid submitPerson = token.UserID;
            string remark = ConfigurationManager.AppSettings["PreviousAgentCode"] + previousAgentCode
                + ConfigurationManager.AppSettings["PreviousAgentName"] + previousAgentName;
            return this.AddMargin("AgentRegistration", DateTime.Parse(reportDate), accountCode, email, null, null, newAgentCode, newAgentName, null,
                null, null, null, null, null, null, newAgentICNo, DateTime.Parse(dateReply), remark, submitPerson, out reference);
        }


        public bool FundTransfer(Token token, StateServerService stateServer, string email, string receive, string organizationName, string customerName,
          string reportDate, string currency, string currencyValue, string accountCode, string bankAccount, string beneficiaryName, string replyDate)
        {
            bool isSucceed = false;
            try
            {
                string body = "<HTML><BODY>";
                body += ConfigurationManager.AppSettings["To"] + organizationName + "<BR>";
                body += ConfigurationManager.AppSettings["From"] + customerName + "(" + email + ")<BR>";
                body += ConfigurationManager.AppSettings["Subject"] + ConfigurationManager.AppSettings["FundTransfer"] + "<BR>";
                body += "<BR>";
                body += ConfigurationManager.AppSettings["Account"] + accountCode + "<BR>";
                body += ConfigurationManager.AppSettings["Currency"] + currency + "<BR>";
                body += ConfigurationManager.AppSettings["Amount"] + currencyValue + "<BR>";
                body += ConfigurationManager.AppSettings["BankAccount"] + bankAccount + "<BR>";
                body += ConfigurationManager.AppSettings["BeneficiaryName"] + beneficiaryName + "<BR>";
                body += ConfigurationManager.AppSettings["ReplyDate"] + replyDate + "<BR>";
                body += ConfigurationManager.AppSettings["From"] + customerName + "<BR>";
                body += "</BODY>";
                body += "</HTML>";

                Guid transactionID = Guid.Empty;
                string subject = (string)ConfigurationManager.AppSettings["FundTransfer"];
                //stateServer.Email(token, "FundTransfer", transactionID, email, receive, subject, body);
                isSucceed = this.SendEmail(email, receive, subject, body);
            }
            catch (Exception e)
            {
                isSucceed = false;
                throw e;
            }
            return isSucceed;
        }


    }

    class FromToDataSet
    {
        private DateTime _From;
        private DateTime _To;
        private DataSet _DataSet;

        public FromToDataSet(DateTime from, DateTime to, DataSet dataSet)
        {
            this._From = from;
            this._To = to;
            this._DataSet = dataSet;
        }

        public DateTime From
        {
            get { return this._From; }
        }

        public DateTime To
        {
            get { return this._To; }
        }

        public DataSet DataSet
        {
            get { return this._DataSet; }
        }
    }
}
