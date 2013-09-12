using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using Trader.Server.SessionNamespace;
using Trader.Server.Setting;
using System.Xml;
using System.Diagnostics;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml.Linq;
using Trader.Common;

namespace Trader.Server.Bll
{
    public class AccountManager
    {
        private AccountManager() { }
        public static readonly AccountManager Default = new AccountManager();
        public XElement  GetAccountsForTradingConsole(Session  session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                var ds = Application.Default.TradingConsoleServer.GetAccountsForTradingConsole(token.UserID);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetAccountsForTradingConsole:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public DataSet GetAccountBankApplicationsNotApproved(Guid accountId)
        {
            throw new NotImplementedException("Reserved");
        }

        public static XElement  GetAccountBanksApproved(Guid accountId,string language)
        {
            try
            {
                string sql = string.Format("dbo.P_GetAccountBanksApproved @accountId='{0}', @language='{1}'", accountId, language);
                var ds = DataAccess.GetData(sql, SettingManager.Default.ConnectionString);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.GetAccountBanksApproved", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        public XElement  GetAccountBankReferenceData(string countryId, string language)
        {
            try
            {
                string sql = string.Format("dbo.P_GetAccountBankReferenceData @language='{0}'", language) + (string.IsNullOrEmpty(countryId)? "" : " , @countryId=" + countryId);
                //AppDebug.LogEvent("TradingConsole.GetAccountBankReferenceData", sql, System.Diagnostics.EventLogEntryType.Error);
                DataSet ds=DataAccess.GetData(sql,SettingManager.Default.ConnectionString);
                return XmlResultHelper.NewResult(ds.ToXml());

            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.GetAccountBankReferenceData", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public XmlNode  GetAccountForCut(Session  session,ref DateTime lastAlertTime, Guid accountId, bool includeTransactions)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);

                AppDebug.LogEvent("[TradingConsole.GetAccountForCut]", string.Format("Token={0},AccountIDs={1}", token, accountId), EventLogEntryType.Warning);

                DateTime maxAlertLevelTime = this.GetMaxAlertLevelTime(accountId, lastAlertTime);
                if (maxAlertLevelTime == DateTime.MinValue)
                {
                    return null;
                }
                else
                {
                    if (maxAlertLevelTime > lastAlertTime)
                    {
                        lastAlertTime = maxAlertLevelTime;
                        return Application.Default.TradingConsoleServer.GetAccounts(token, Application.Default.StateServer, new Guid[] { accountId }, includeTransactions);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetAccountForCut:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }


        public XmlNode  GetAccountsForCut(Session  session,Guid[] accountIDs, bool includeTransactions)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                AppDebug.LogEvent("[TradingConsole.GetAccountsForCut]", string.Format("Token={0},AccountIDs={1}", token, accountIDs), EventLogEntryType.Warning);
                return  Application.Default.TradingConsoleServer.GetAccounts(token, Application.Default.StateServer, accountIDs, includeTransactions);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetAccountsForCut:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }


        private DateTime GetMaxAlertLevelTime(Guid accountId, DateTime lastAlertTime)
        {
            string sql = string.Format("SELECT dbo.FV_GetMaxAlertLevelTime('{0}','{1}')", accountId, lastAlertTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            object o = DataAccess.ExecuteScalar(sql, SettingManager.Default.ConnectionString);
            return (o == DBNull.Value) ? DateTime.MinValue : (DateTime)o;
        }


        public XElement  UpdateAccountSetting(Session  session,Guid[] accountIds)
        {
            bool result = false;
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                result= Application.Default.TradingConsoleServer.UpdateAccountSetting(token.UserID, accountIds);
                return XmlResultHelper.NewResult(result.ToPlainBitString());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdateAccountSetting:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }

        }

        public void UpdateAccount(Session session, Guid accountID, Guid groupID, bool isDelete, bool isDeleteGroup)
        {
            try
            {
                TradingConsoleState state = SessionManager.Default.GetTradingConsoleState(session);
                Application.Default.TradingConsoleServer.UpdateAccount(accountID, groupID, isDelete, isDeleteGroup, state);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdateAccount:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }


        public XmlNode  GetAccounts(Session  session, Guid[] accountIDs, bool includeTransactions)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                AppDebug.LogEvent("[TradingConsole.GetAccounts]", string.Format("Token={0},AccountIDs={1}", token, accountIDs), EventLogEntryType.Warning);
                return Application.Default.TradingConsoleServer.GetAccounts(token, Application.Default.StateServer, accountIDs, includeTransactions);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetAccounts:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }




    }
}
