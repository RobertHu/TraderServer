using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using Trader.Server.Setting;
using log4net;
using System.Data;
using System.IO;
using Trader.Server.Session;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using Trader.Server.TypeExtension;
using Trader.Server.Util;
using Trader.Helper;
using Wintellect.Threading;
using Wintellect.Threading.AsyncProgModel;
using Serialization;
using Trader.Common;
namespace Trader.Server.Bll
{
    public class LoginManager
    {
        private ILog _Logger = LogManager.GetLogger(typeof(LoginManager));
        private LoginManager() { }
        public readonly static LoginManager Default = new LoginManager();

        public IEnumerator<int> Login(SerializedObject request, string loginID, string password, string version, int appType, AsyncEnumerator ae)
        {
            long session = request.Session;
            string connectionString = SettingManager.Default.ConnectionString;
            IsFailedCountExceed(loginID, password, connectionString);
            LoginParameter loginParameter = new LoginParameter();
            loginParameter.CompanyName = string.Empty;
            if (loginID == String.Empty)
            {
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, GetLocalIP());
                SendErrorResult(request);
                yield break;
            }
            string message = string.Empty;
            Application.Default.ParticipantService.BeginLogin(loginID, password, ae.End(), null);
            yield return 1;
            try
            {
                loginParameter.UserID = Application.Default.ParticipantService.EndLogin(ae.DequeueAsyncResult());
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                SendErrorResult(request);
                yield break;
            }
            if (loginParameter.UserID == Guid.Empty)
            {
                _Logger.ErrorFormat("{0} is not a valid user", loginID);
            }
            else
            {
                Guid programID = new Guid(SettingManager.Default.GetLoginSetting("TradingConsole"));
                Guid permissionID = new Guid(SettingManager.Default.GetLoginSetting("Run"));
                Application.Default.SecurityService.BeginCheckPermission(loginParameter.UserID, programID, permissionID, "", "", loginParameter.UserID, ae.End(), null);
                yield return 1;
                bool isAuthrized=false;
                try
                {
                    isAuthrized = Application.Default.SecurityService.EndCheckPermission(ae.DequeueAsyncResult(), out message);
                }
                catch (Exception ex)
                {
                    _Logger.Error(ex);
                    SendErrorResult(request);
                    yield break;
                }
                if (!isAuthrized)
                {
                    _Logger.ErrorFormat("{0} doesn't have the right to login trader", loginID);
                    loginParameter.UserID = Guid.Empty;
                }
                else
                {
                    Token token = new Token(Guid.Empty, UserType.Customer, (AppType)appType);
                    token.UserID = loginParameter.UserID;
                    token.SessionID = session.ToString();
                    SessionManager.Default.AddToken(session, token);
                    Application.Default.StateServer.BeginLogin(token, ae.End(), null);
                    yield return 1;
                    bool isStateServerLogined=false;
                    try
                    {
                        isStateServerLogined = Application.Default.StateServer.EndLogin(ae.DequeueAsyncResult());
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex);
                        SendErrorResult(request);
                        yield break;
                    }
                    SetLoginParameter(loginParameter, session, password, version, appType, isStateServerLogined, token);
                }
            }
            if (loginParameter.UserID == Guid.Empty)
            {
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, GetLocalIP());
                SendErrorResult(request);
            }
            else
            {
                DataSet initData=null;
                if (appType == (int)AppType.TradingConsole)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    Application.Default.StateServer.BeginGetInitData(token, null, ae.End(), null);
                    yield return 1;
                    int sequence;
                    try
                    {
                       initData = Application.Default.StateServer.EndGetInitData(ae.DequeueAsyncResult(), out sequence);
                    }
                    catch (Exception ex)
                    {
                        SendErrorResult(request);
                        yield break;
                    }
                }
               var loginData = SetResult(request, loginParameter, session, loginID, password, version, appType, connectionString);
               if (initData != null && loginData != null)
               {
                   DataSet ds = InitDataService.Init(session, initData);
                   SetLoginDataToInitData(initData, loginData);
                   request.ContentInPointer = ds.ToPointer();
                   SendCenter.Default.Send(request);
               }
               else
               {
                   SendErrorResult(request);
               }
            }
        }

        private void SetLoginDataToInitData(DataSet initData,XElement loginData)
        {
            DataTable table = new DataTable("LoginTable");
            DataColumn column = new DataColumn("LoginColumn");
            column.DataType = typeof(string);
            column.AutoIncrement = false;
            table.Columns.Add(column);
            DataRow dr = table.NewRow();
            string loginString = loginData.ToString();
            dr["LoginColumn"] = loginString;
            table.Rows.Add(dr);
            initData.Tables.Add(table);
        }


        private void SendErrorResult(SerializedObject request)
        {
            request.Content = XmlResultHelper.ErrorResult;
            SendCenter.Default.Send(request);
        }


        private void IsFailedCountExceed(string loginID, string password, string connectionString)
        {
            if (LoginRetryTimeHelper.IsFailedCountExceeded(loginID, ParticipantType.Customer, connectionString))
            {
                string info = string.Format("{0} login failed: exceed max login retry times", loginID);
                _Logger.Warn(info);
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, this.GetLocalIP());
                XmlDocument document = new XmlDocument();
                document.LoadXml("<?xml version=\"1.0\" encoding=\"gb2312\" ?><Error Code=\"ExceedMaxRetryLimit\"/>");
                var result = document.DocumentElement;
                _Logger.WarnFormat("login failed: exceed max login retry times, parameter={0}", result.OuterXml);
            }
        }

        private XElement SetResult(SerializedObject request, LoginParameter loginParameter, long session, string loginID, string password, string version, int appType, string connectionString)
        {
            XElement result;
            Token token = SessionManager.Default.GetToken(session);
            if (loginParameter.UserID != Guid.Empty)
            {
                LoginRetryTimeHelper.ClearFailedCount(loginParameter.UserID, ParticipantType.Customer, connectionString);
                string language = string.IsNullOrEmpty(version) ? "ENG" : version.Substring(version.Length - 3);
                if (token == null)
                {
                    AppType tokenType = (AppType)appType;
                    token = new Token(loginParameter.UserID, UserType.Customer, tokenType);
                    SessionManager.Default.AddToken(session, token);
                }
                token.Language = language;
                var companyLogo = this.GetLogoForJava(loginParameter.CompanyName);
                var colorSettings = this.GetColorSettingsForJava(loginParameter.CompanyName);
                var systemParameter = this.GetParameterForJava(session, loginParameter.CompanyName, language);
                var settings = this.GetSettings(loginParameter.CompanyName);
                var tradingAccountData = Application.Default.TradingConsoleServer.GetTradingAccountData(loginParameter.UserID);
                var recoverPasswordData = Application.Default.TradingConsoleServer.GetRecoverPasswordData(language, loginParameter.UserID);
                var dict = new Dictionary<string, string>()
                    {
                        {"companyName", loginParameter.CompanyName},
                        {"disallowLogin", loginParameter.DisallowLogin.ToString()},
                        {"isActivateAccount", loginParameter.IsActivateAccount.ToString()},
                        {"isDisableJava30", loginParameter.IsDisableJava30.ToString()},
                        {"companyLogo",Convert.ToBase64String(companyLogo)},
                        {"colorSettings",colorSettings.OuterXml},
                        {"parameter",systemParameter.OuterXml},
                        {"settings",settings.OuterXml},
                        {"recoverPasswordData",recoverPasswordData.ToXml()},
                        {"tradingAccountData", tradingAccountData.ToXml()},
                        {"userId", loginParameter.UserID.ToString()},
                        {"session", session.ToString()}
                    };
                result = XmlResultHelper.NewResult(dict);
                Application.Default.SessionMonitor.Add(session);
            }
            else
            {
                LoginRetryTimeHelper.IncreaseFailedCount(loginID, ParticipantType.Customer, connectionString);
                result = XmlResultHelper.ErrorResult;
            }
            if (token.AppType != AppType.Mobile)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private class LoginParameter
        {
            public Guid UserID { get; set; }
            public string CompanyName { get; set; }
            public bool DisallowLogin { get; set; }
            public bool IsActivateAccount { get; set; }
            public bool IsDisableJava30 { get; set; }

        }


        private void SetLoginParameter(LoginParameter loginParameter, long session, string password, string environmentInfo, int appType, bool isStateServerLogined, Token token)
        {
            bool isPathPassed = false;
            DataSet dataSet = Application.Default.TradingConsoleServer.GetLoginParameters(loginParameter.UserID, loginParameter.CompanyName);
            DataRowCollection rows = dataSet.Tables[0].Rows;
            foreach (DataRow row in rows)
            {
                isPathPassed = (System.Boolean)row["IsPathPassed"];
                loginParameter.DisallowLogin = (System.Boolean)row["DisallowLogin"];
                loginParameter.IsActivateAccount = (System.Boolean)row["IsActivateAccount"];
                loginParameter.IsDisableJava30 = (System.Boolean)row["IsDisableJava30"];
                if (string.IsNullOrEmpty(loginParameter.CompanyName))
                {
                    string companyName2 = (System.String)row["Path"];
                    if (companyName2 == string.Empty) companyName2 = "MHL";
                    if (Directory.Exists(GetOrginazationDir(companyName2)))
                    {
                        isPathPassed = true;
                        loginParameter.CompanyName = companyName2;
                    }
                }
                break;
            }
            if (!isPathPassed || loginParameter.DisallowLogin)
            {
                loginParameter.UserID = Guid.Empty;
            }
            else
            {
                if (isStateServerLogined)
                {
                    SessionManager.Default.AddSession(loginParameter.UserID, session);
                    Application.Default.TradingConsoleServer.SaveLogonLog(token, GetLocalIP(), environmentInfo);
                }
                else
                {
                    AppDebug.LogEvent("TradingConsole.Login2", loginParameter.UserID + " StateServer.Login failed", EventLogEntryType.Error);
                    SessionManager.Default.RemoveToken(session);
                    loginParameter.UserID = Guid.Empty;
                }
            }
        }


        private byte[] GetLogoForJava(string companyCode)
        {
            string companyDir = GetOrginazationDir(companyCode);
            string filePath = Path.Combine(companyDir, SettingManager.Default.GetLoginSetting("logo"));
            return File.ReadAllBytes(filePath);
        }

        private string GetLocalIP()
        {
            return string.Empty;
        }




        private XmlNode GetColorSettingsForJava(string companyCode)
        {
            //Get xml
            try
            {
                string dir = GetOrginazationDir(companyCode);
                string xmlPath = Path.Combine(dir, SettingManager.Default.GetLoginSetting("color_setting"));
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

        private XmlNode GetSettings(string companyCode)
        {
            //Get xml
            try
            {
                string dir = GetOrginazationDir(companyCode);
                string xmlPath = Path.Combine(dir, SettingManager.Default.GetLoginSetting("setting"));
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);
                System.Xml.XmlNode node = doc.GetElementsByTagName("Settings")[0];
                return node;
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Service.GetSettings", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        private XmlNode GetParameterForJava(long session, string companyCode, string version)
        {
            SessionManager.Default.AddVersion(session, version);
            string physicalPath = Path.Combine(GetOrginazationDir(companyCode), version);

            //Get xml
            try
            {
                string xmlPath = Path.Combine(physicalPath, SettingManager.Default.GetLoginSetting("parameter"));

                System.Xml.XmlDocument parameterDocument = new System.Xml.XmlDocument();
                parameterDocument.Load(xmlPath);
                System.Xml.XmlNode parameterXmlNode = parameterDocument.GetElementsByTagName("Parameter")[0];

                xmlPath = Path.Combine(physicalPath, SettingManager.Default.GetLoginSetting("login"));
                System.Xml.XmlDocument loginDocument = new System.Xml.XmlDocument();
                loginDocument.Load(xmlPath);
                System.Xml.XmlNode loginXmlNode = loginDocument.GetElementsByTagName("Login")[0];

                string newsLanguage = loginXmlNode.SelectNodes("NewsLanguage").Item(0).InnerXml;
                TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                if (state == null)
                {
                    state = new TraderState(session.ToString());
                }
                state.Language = newsLanguage.ToLower();
                SessionManager.Default.AddTradingConsoleState(session, state);
                XmlElement newChild = parameterDocument.CreateElement("NewsLanguage");
                newChild.InnerText = loginXmlNode.SelectNodes("NewsLanguage").Item(0).InnerXml;
                parameterXmlNode.AppendChild(newChild);
                string agreementContent = "";
                string agreementFileFullPath = Path.Combine(physicalPath, SettingManager.Default.GetLoginSetting("agreement"));
                if (File.Exists(agreementFileFullPath))
                {
                    System.Xml.XmlDocument agreementDocument = new System.Xml.XmlDocument();
                    agreementDocument.Load(agreementFileFullPath);
                    System.Xml.XmlNode agreementXmlNode = agreementDocument.GetElementsByTagName("Agreement")[0];

                    string showAgreement = agreementXmlNode.SelectNodes("ShowAgreement").Item(0).InnerXml.Trim().ToLower();
                    if (showAgreement == "true")
                    {
                        agreementContent = agreementXmlNode.SelectNodes("Content").Item(0).InnerXml;
                    }
                }

                XmlElement agreementXmlNode2 = parameterDocument.CreateElement("Agreement");
                agreementXmlNode2.InnerText = agreementContent;
                parameterXmlNode.AppendChild(agreementXmlNode2);

                string columnSettings = Path.Combine(GetOrginazationDir(companyCode), SettingManager.Default.GetLoginSetting("column_setting"));
                if (File.Exists(columnSettings))
                {
                    System.Xml.XmlDocument columnSettingsDocument = new System.Xml.XmlDocument();
                    columnSettingsDocument.Load(columnSettings);
                    System.Xml.XmlNode columnSettingsXmlNode = columnSettingsDocument.GetElementsByTagName("ColumnSettings")[0];
                    columnSettingsXmlNode = parameterDocument.ImportNode(columnSettingsXmlNode, true);
                    parameterXmlNode.AppendChild(columnSettingsXmlNode);
                }
                string integralitySettings = Path.Combine(GetOrginazationDir(companyCode), SettingManager.Default.GetLoginSetting("integrality_settings"));
                if (File.Exists(columnSettings))
                {
                    System.Xml.XmlDocument integralitySettingsDocument = new System.Xml.XmlDocument();
                    integralitySettingsDocument.Load(integralitySettings);
                    System.Xml.XmlNode integralitySettingsXmlNode = integralitySettingsDocument.GetElementsByTagName("IntegralitySettings")[0];
                    integralitySettingsXmlNode = parameterDocument.ImportNode(integralitySettingsXmlNode, true);
                    parameterXmlNode.AppendChild(integralitySettingsXmlNode);
                }

                System.Xml.XmlNode node = parameterDocument.GetElementsByTagName("Parameters")[0];
                return node;
            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Service.GetParameterForJava", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return null;
        }

        private string GetOrginazationDir(string companyCode)
        {
            return Path.Combine(SettingManager.Default.PhysicPath, companyCode);
        }

        public XElement Logout(long session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                if (token != null)
                {
                    TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                    Application.Default.TradingConsoleServer.SaveLogoutLog(token, GetLocalIP(), state == null ? false : state.IsEmployee);
                    Application.Default.StateServer.Logout(token);
                    Application.Default.SessionMonitor.Remove(session);
                }

            }
            catch (System.Exception ex)
            {
                AppDebug.LogEvent("TradingConsole.Service.Logout(log)", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return XmlResultHelper.NewResult("");
        }


    }
}