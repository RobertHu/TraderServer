using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using AsyncSslServer.Session;
using System.Diagnostics;
using System.Xml;
using AsyncSslServer.Util;
using AsyncSslServer.TypeExtension;
namespace AsyncSslServer.Bll
{
    public class PasswordService
    {
        public bool RecoverPasswordDatas(string session, string[][] recoverPasswordDatas)
        {
            try
            {
                if (recoverPasswordDatas.Length > 0)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    Application.Default.TradingConsoleServer.UpdateRecoverPasswordData(token.UserID, recoverPasswordDatas);
                    return true;
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.RecoverPasswordDatas", exception.ToString(), EventLogEntryType.Error);
            }
            return false;
        }

        private static bool UpdatePassword3(string session, string loginID, string oldPassword, string newPassword, string[][] recoverPasswordDatas, out string message)
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
                        AppDebug.LogEvent("TradingConsole.UpdatePassword", string.Format("UpdatePassword({0},{1},{2},{3}) Failed", token.UserID, newPassword, token.UserID, message), EventLogEntryType.Warning);
                    }
                    return isSucceed;
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.UpdatePassword", string.Format("{0} == Login({1},{2})", userID, loginID, oldPassword), EventLogEntryType.Warning);
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdatePassword", exception.ToString(), EventLogEntryType.Error);
            }

            return false;
        }

        //Activate
        public static bool UpdatePassword2(string session, string loginID, string oldPassword, string newPassword, string[][] recoverPasswordDatas, out string message)
        {
            message = "";
            try
            {
                bool isSucceed = UpdatePassword3(session, loginID, oldPassword, newPassword, recoverPasswordDatas, out message);
                if (isSucceed)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                    bool isEmployee = state == null ? false : state.IsEmployee;
                    Application.Default.TradingConsoleServer.SaveActivateLog(token, isEmployee, "");
                }
                return isSucceed;
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdatePassword2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                message = exception.ToString();
            }
            return false;
        }

        //Change Password
        public static XmlNode UpdatePassword(string session, string loginID, string oldPassword, string newPassword)
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
                    bool isEmployee = state == null ? false : state.IsEmployee;
                    Application.Default.TradingConsoleServer.SaveChangePasswordLog(token, isEmployee, "");
                }
                Dictionary<string, string> dict = new Dictionary<string, string>() { { "message", message }, { "isSucceed", isSucceed.ToXmlResult() } };
                return XmlResultHelper.NewResult(dict);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdatePassword:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
           
        }

    }
}
