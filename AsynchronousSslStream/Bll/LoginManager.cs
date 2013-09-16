using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using log4net;
using System.Data;
using System.IO;
using Trader.Server.SessionNamespace;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using Trader.Server.TypeExtension;
using Trader.Server.Util;
using Wintellect.Threading;
using Wintellect.Threading.AsyncProgModel;
using Trader.Common;
using Trader.Server.Serialization;
namespace Trader.Server.Bll
{
    public class LoginManager
    {
        private readonly ILog _Logger = LogManager.GetLogger(typeof(LoginManager));
        private LoginManager() { }
        public readonly static LoginManager Default = new LoginManager();

        public IEnumerator<int> Login(SerializedObject request, string loginID, string password, string version, int appType, AsyncEnumerator ae)
        {
            Session session = request.Session;
            string connectionString = SettingManager.Default.ConnectionString;
            IsFailedCountExceed(loginID, password, connectionString);
            var loginParameter = new LoginParameter();
            loginParameter.CompanyName = string.Empty;
            if (loginID == String.Empty)
            {
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, GetLocalIP());
                SendErrorResult(request,appType);
                yield break;
            }
            Application.Default.ParticipantService.BeginLogin(loginID, password, ae.End(), null);
            yield return 1;
            try
            {
                loginParameter.UserID = Application.Default.ParticipantService.EndLogin(ae.DequeueAsyncResult());
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                SendErrorResult(request,appType);
                yield break;
            }
            Guid programID = new Guid(SettingManager.Default.GetLoginSetting("TradingConsole"));
            Guid permissionID = new Guid(SettingManager.Default.GetLoginSetting("Run"));
            if (loginParameter.UserID == Guid.Empty)
            {
                _Logger.ErrorFormat("{0} is not a valid user", loginID);
            }
            else
            {
                Application.Default.SecurityService.BeginCheckPermission(loginParameter.UserID, programID, permissionID, "", "", loginParameter.UserID, ae.End(), null);
                yield return 1;
                bool isAuthrized=false;
                try
                {
                    string message;
                    isAuthrized = Application.Default.SecurityService.EndCheckPermission(ae.DequeueAsyncResult(), out message);
                }
                catch (Exception ex)
                {
                    _Logger.Error(ex);
                    SendErrorResult(request,appType);
                    yield break;
                }
                if (!isAuthrized)
                {
                    _Logger.ErrorFormat("{0} doesn't have the right to login trader", loginID);
                    loginParameter.UserID = Guid.Empty;
                }
                else
                {
                    var token = new Token(Guid.Empty, UserType.Customer, (AppType)appType);
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
                        SendErrorResult(request,appType);
                        yield break;
                    }
                    SetLoginParameter(loginParameter, session, password, version, appType, isStateServerLogined, token);
                }
            }
            if (loginParameter.UserID == Guid.Empty)
            {
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, GetLocalIP());
                SendErrorResult(request,appType);
            }
            else
            {
                DataSet initData=null;
                var loginData = SetResult(request, loginParameter, session, loginID, password, version, appType, connectionString);
                if (appType == (int)AppType.TradingConsole)
                {
                    Token token = SessionManager.Default.GetToken(session);
                    Application.Default.StateServer.BeginGetInitData(token, null, ae.End(), null);
                    yield return 1;
                    try
                    {
                        int sequence;
                        initData = Application.Default.StateServer.EndGetInitData(ae.DequeueAsyncResult(), out sequence);
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex);
                        SendErrorResult(request,appType);
                        yield break;
                    }
                }
               if (initData != null && loginData != null)
               {
                   DataSet ds = InitDataService.Init(session, initData);
                   SetLoginDataToInitData(ds, loginData);
                   request.ContentInPointer = ds.ToPointer();
               }
               else
               {
                   request.Content = XmlResultHelper.ErrorResult;
               }
               if (appType != (int)AppType.Mobile)
               {
                   SendCenter.Default.Send(request);
               }
             
            }
        }

        private void SetLoginDataToInitData(DataSet initData,XElement loginData)
        {
            var table = new DataTable(LoginConstants.LoginTabalName);
            var column = new DataColumn(LoginConstants.LoginColumnName);
            column.DataType = typeof(string);
            column.AutoIncrement = false;
            table.Columns.Add(column);
            var dr = table.NewRow();
            string loginString = loginData.ToString();
            dr[LoginConstants.LoginColumnName] = loginString;
            table.Rows.Add(dr);
            initData.Tables.Add(table);
        }


        private void SendErrorResult(SerializedObject request,int appType)
        {
            if (appType != (int)AppType.Mobile)
            {
                request.Content = XmlResultHelper.ErrorResult;
                SendCenter.Default.Send(request);
            }
        }


        private void IsFailedCountExceed(string loginID, string password, string connectionString)
        {
            if (!LoginRetryTimeHelper.IsFailedCountExceeded(loginID, ParticipantType.Customer, connectionString))
                return;
            string info = string.Format("{0} login failed: exceed max login retry times", loginID);
            _Logger.Warn(info);
            AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
            Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, this.GetLocalIP());
            var document = new XmlDocument();
            document.LoadXml("<?xml version=\"1.0\" encoding=\"gb2312\" ?><Error Code=\"ExceedMaxRetryLimit\"/>");
            var result = document.DocumentElement;
            if (result != null)
                _Logger.WarnFormat("login failed: exceed max login retry times, parameter={0}", result.OuterXml);
        }

        private XElement SetResult(SerializedObject request, LoginParameter loginParameter, Session session, string loginID, string password, string version, int appType, string connectionString)
        {
            XElement result;
            var token = SessionManager.Default.GetToken(session);
            if (loginParameter.UserID != Guid.Empty)
            {
                LoginRetryTimeHelper.ClearFailedCount(loginParameter.UserID, ParticipantType.Customer, connectionString);
                var language = string.IsNullOrEmpty(version) ? "ENG" : version.Substring(version.Length - 3);
                if (token == null)
                {
                    var tokenType = (AppType)appType;
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
            return token.AppType != AppType.Mobile ? result : null;
        }

        private class LoginParameter
        {
            public Guid UserID { get; set; }
            public string CompanyName { get; set; }
            public bool DisallowLogin { get; set; }
            public bool IsActivateAccount { get; set; }
            public bool IsDisableJava30 { get; set; }

        }


        private void SetLoginParameter(LoginParameter loginParameter, Session session, string password, string environmentInfo, int appType, bool isStateServerLogined, Token token)
        {
            bool isPathPassed = false;
            var dataSet = Application.Default.TradingConsoleServer.GetLoginParameters(loginParameter.UserID, loginParameter.CompanyName);
            var rows = dataSet.Tables[0].Rows;
            foreach (DataRow row in rows)
            {
                isPathPassed = (Boolean)row["IsPathPassed"];
                loginParameter.DisallowLogin = (Boolean)row["DisallowLogin"];
                loginParameter.IsActivateAccount = (Boolean)row["IsActivateAccount"];
                loginParameter.IsDisableJava30 = (Boolean)row["IsDisableJava30"];
                if (string.IsNullOrEmpty(loginParameter.CompanyName))
                {
                    var companyName2 = (String)row["Path"];
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
                    DBLogService.SaveLog(token,environmentInfo);
                }
                else
                {
                    _Logger.Error(" StateServer.Login failed");
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
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                var node = doc.GetElementsByTagName("ColorSettings")[0];
                return node;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                return null;
            }
        }

        private XmlNode GetSettings(string companyCode)
        {
            //Get xml
            try
            {
                string dir = GetOrginazationDir(companyCode);
                string xmlPath = Path.Combine(dir, SettingManager.Default.GetLoginSetting("setting"));
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                var node = doc.GetElementsByTagName("Settings")[0];
                return node;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                return null;
            }
        }

        private XmlNode GetParameterForJava(Session session, string companyCode, string version)
        {
            SessionManager.Default.AddVersion(session, version);
            string physicalPath = Path.Combine(GetOrginazationDir(companyCode), version);

            //Get xml
            try
            {
                string xmlPath = Path.Combine(physicalPath, SettingManager.Default.GetLoginSetting("parameter"));

                var parameterDocument = new XmlDocument();
                parameterDocument.Load(xmlPath);
                XmlNode parameterXmlNode = parameterDocument.GetElementsByTagName("Parameter")[0];

                xmlPath = Path.Combine(physicalPath, SettingManager.Default.GetLoginSetting("login"));
                var loginDocument = new System.Xml.XmlDocument();
                loginDocument.Load(xmlPath);
                XmlNode loginXmlNode = loginDocument.GetElementsByTagName("Login")[0];
                string newsLanguage = loginXmlNode.SelectNodes("NewsLanguage").Item(0).InnerXml;
                TraderState state = SessionManager.Default.GetTradingConsoleState(session) ??
                                    new TraderState(session.ToString());
                state.Language = newsLanguage.ToLower();
                SessionManager.Default.AddTradingConsoleState(session, state);
                XmlElement newChild = parameterDocument.CreateElement("NewsLanguage");
                newChild.InnerText = loginXmlNode.SelectNodes("NewsLanguage").Item(0).InnerXml;
                parameterXmlNode.AppendChild(newChild);
                string agreementContent = "";
                string agreementFileFullPath = Path.Combine(physicalPath, SettingManager.Default.GetLoginSetting("agreement"));
                if (File.Exists(agreementFileFullPath))
                {
                    var agreementDocument = new System.Xml.XmlDocument();
                    agreementDocument.Load(agreementFileFullPath);
                    var agreementXmlNode = agreementDocument.GetElementsByTagName("Agreement")[0];

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
                    var columnSettingsDocument = new XmlDocument();
                    columnSettingsDocument.Load(columnSettings);
                    XmlNode columnSettingsXmlNode = columnSettingsDocument.GetElementsByTagName("ColumnSettings")[0];
                    columnSettingsXmlNode = parameterDocument.ImportNode(columnSettingsXmlNode, true);
                    parameterXmlNode.AppendChild(columnSettingsXmlNode);
                }
                string integralitySettings = Path.Combine(GetOrginazationDir(companyCode), SettingManager.Default.GetLoginSetting("integrality_settings"));
                if (File.Exists(columnSettings))
                {
                    var integralitySettingsDocument = new XmlDocument();
                    integralitySettingsDocument.Load(integralitySettings);
                    var integralitySettingsXmlNode = integralitySettingsDocument.GetElementsByTagName("IntegralitySettings")[0];
                    integralitySettingsXmlNode = parameterDocument.ImportNode(integralitySettingsXmlNode, true);
                    parameterXmlNode.AppendChild(integralitySettingsXmlNode);
                }

                var node = parameterDocument.GetElementsByTagName("Parameters")[0];
                return node;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                return null;
            }
        }

        private string GetOrginazationDir(string companyCode)
        {
            return Path.Combine(SettingManager.Default.PhysicPath, companyCode);
        }

        public XElement Logout(Session session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                if (token != null)
                {
                    TraderState state = SessionManager.Default.GetTradingConsoleState(session);
                    Application.Default.TradingConsoleServer.SaveLogoutLog(token, GetLocalIP(), state != null && state.IsEmployee);
                    Application.Default.StateServer.Logout(token);
                    Application.Default.SessionMonitor.Remove(session);
                }

            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
            return XmlResultHelper.NewResult("");
        }


    }
}