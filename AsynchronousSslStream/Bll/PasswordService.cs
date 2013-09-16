using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using log4net;
using Trader.Server.SessionNamespace;
using System.Diagnostics;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using Trader.Common;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;
namespace Trader.Server.Bll
{
    public class PasswordService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof (PasswordService));
        public static XElement  RecoverPasswordDatas(Session session, string[][] recoverPasswordDatas)
        {
            try
            {
                if (recoverPasswordDatas.Length > 0)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    Application.Default.TradingConsoleServer.UpdateRecoverPasswordData(token.UserID, recoverPasswordDatas);
                    return XmlResultHelper.NewResult(StringConstants.OkResult);
                }
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
            }
            return XmlResultHelper.ErrorResult;
        }


        public static XElement  ChangeMarginPin(Guid accountId, string oldPassword, string newPassword)
        {
            try
            {
                var result= Application.Default.TradingConsoleServer.ChangeMarginPin(accountId, oldPassword, newPassword);
                return XmlResultHelper.NewResult(result.ToPlainBitString());
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        public static XElement  VerifyMarginPin(Guid accountId, string password)
        {
            try
            {
                bool result=Application.Default.TradingConsoleServer.VerifyMarginPin(accountId, password);
                return XmlResultHelper.NewResult(result.ToPlainBitString());
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }



        public static XElement  ModifyTelephoneIdentificationCode(Session session, Guid accountId, string oldCode, string newCode)
        {
            bool lastResult = false;
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                string connectionString = SettingManager.Default.ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(connectionString))
                {
                    SqlCommand sqlCommand = sqlconnection.CreateCommand();
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "Account_UpdateDescription";
                    SqlCommandBuilder.DeriveParameters(sqlCommand);
                    sqlCommand.Parameters["@id"].Value = accountId;
                    sqlCommand.Parameters["@oldDescription"].Value = oldCode;
                    sqlCommand.Parameters["@newDescription"].Value = newCode;

                    sqlCommand.ExecuteNonQuery();
                    int result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                    if (result == 0)
                    {
                        sqlCommand = sqlconnection.CreateCommand();
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = string.Format("UPDATE AccountHistory SET UpdatePersonID = '{0}' WHERE ID = '{1}' AND [Description] = '{2}' AND UpdateTime = (SELECT MAX(UpdateTime) FROM AccountHistory WHERE ID='{1}' AND [Description] = '{2}')", token.UserID, accountId, newCode);
                        sqlCommand.ExecuteNonQuery();
                        lastResult = true;
                    }
                    else
                    {
                        //maybe the accountId is an employee id
                        sqlCommand = sqlconnection.CreateCommand();
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandText = "Employee_UpdateTelephonePin";
                        sqlconnection.Open();
                        SqlCommandBuilder.DeriveParameters(sqlCommand);
                        sqlCommand.Parameters["@id"].Value = accountId;
                        sqlCommand.Parameters["@oldPin"].Value = oldCode;
                        sqlCommand.Parameters["@newPin"].Value = newCode;

                        sqlCommand.ExecuteNonQuery();
                        result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                        lastResult = (result == 0);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _Logger.Error(ex);
            }
            return XmlResultHelper.NewResult(lastResult.ToPlainBitString());
        }



        private static bool UpdatePassword3(Session session, string loginID, string oldPassword, string newPassword, string[][] recoverPasswordDatas, out string message)
        {
            message = "";
            try
            {
                Guid userID = Application.Default.ParticipantService.Login(loginID, oldPassword);
                if (userID != Guid.Empty)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    bool isSucceed = Application.Default.ParticipantService.UpdatePassword(token.UserID, newPassword, token.UserID, out message);
                    if (isSucceed)
                    {
                        if (recoverPasswordDatas.Length > 0)
                        {
                            Application.Default.TradingConsoleServer.UpdateRecoverPasswordData(userID, recoverPasswordDatas);
                        }
                        Application.Default.TradingConsoleServer.ActivateAccountPass(token);
                        Application.Default.StateServer.NotifyPasswordChanged(userID, loginID, newPassword);
                    }
                    else
                    {
                        _Logger.Warn(string.Format("UpdatePassword({0},{1},{2},{3}) Failed", token.UserID, newPassword,
                            token.UserID, message));
                    }
                    return isSucceed;
                }
                _Logger.Warn(string.Format("{0} == Login({1},{2})", userID, loginID, oldPassword));
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
            }
            return false;
        }

        //Activate
        public static bool UpdatePassword2(Session session, string loginID, string oldPassword, string newPassword, string[][] recoverPasswordDatas, out string message)
        {
            message = "";
            try
            {
                bool isSucceed = UpdatePassword3(session, loginID, oldPassword, newPassword, recoverPasswordDatas, out message);
                if (isSucceed)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                    bool isEmployee = state != null && state.IsEmployee;
                    Application.Default.TradingConsoleServer.SaveActivateLog(token, isEmployee, "");
                }
                return isSucceed;
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                message = exception.ToString();
                return false;
            }
        }

        //Change Password
        public static XElement  UpdatePassword(Session session, string loginID, string oldPassword, string newPassword)
        {
            string message = "";
            bool isSucceed = false;
            try
            {
                string[][] recoverPasswordDatas = new string[][] { };
                isSucceed = UpdatePassword3(session, loginID, oldPassword, newPassword, recoverPasswordDatas, out message);
                if (isSucceed)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                    bool isEmployee = state != null && state.IsEmployee;
                    Application.Default.TradingConsoleServer.SaveChangePasswordLog(token, isEmployee, "");
                }
                var dict = new Dictionary<string, string>() { { "message", message }, { "isSucceed", isSucceed.ToPlainBitString() } };
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
           
        }

    }
}
