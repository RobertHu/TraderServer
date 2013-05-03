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
namespace Trader.Server.Bll
{
    public class LoginManager
    {
        private ILog _Logger = LogManager.GetLogger(typeof(LoginManager));
        private LoginManager() {}
        public readonly static LoginManager Default = new LoginManager();

        public XmlNode Login(string session,string loginID, string password, string version,int appType)
        {
            XmlNode systemParameter;
            string companyName="";
            bool disallowLogin;
            bool isActivateAccount;
            bool isDisableJava30;
            byte[] companyLogo;
            XmlNode colorSettings;
            XmlNode settings;
            DataSet recoverPasswordData;
            DataSet tradingAccountData;
            XmlNode result = null;
            try
            {
                string connectionString = SettingManager.Default.ConnectionString;
                if (LoginRetryTimeHelper.IsFailedCountExceeded(loginID, ParticipantType.Customer, connectionString))
                {
                    string info = string.Format("{0} login failed: exceed max login retry times", loginID);
                    _Logger.Warn(info);
                    AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                    Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, this.GetLocalIP());
                    XmlDocument document = new XmlDocument();
                    document.LoadXml("<?xml version=\"1.0\" encoding=\"gb2312\" ?><Error Code=\"ExceedMaxRetryLimit\"/>");
                    systemParameter = document.DocumentElement;
                    _Logger.WarnFormat("login failed: exceed max login retry times, parameter={0}", systemParameter.OuterXml);
                }
                Guid userId = this.Login2(session, loginID, password, ref companyName, out disallowLogin, out isActivateAccount, out isDisableJava30, true, version, appType);
                if (userId != Guid.Empty)
                {
                    LoginRetryTimeHelper.ClearFailedCount(userId, ParticipantType.Customer, connectionString);
                    string language = string.IsNullOrEmpty(version) ? "ENG" : version.Substring(version.Length - 3);
                    Token token = SessionManager.Default.GetToken(session);
                    if (token == null)
                    {
                        AppType tokenType = (AppType)appType;
                        token = new Token(userId, UserType.Customer, tokenType);
                        SessionManager.Default.AddToken(session, token);
                    }
                    token.Language = language;
                    companyLogo = this.GetLogoForJava(companyName);
                    colorSettings = this.GetColorSettingsForJava(companyName);
                    systemParameter = this.GetParameterForJava(session, companyName, language);
                    settings = this.GetSettings(companyName);
                    tradingAccountData = Application.Default.TradingConsoleServer.GetTradingAccountData(userId);
                    recoverPasswordData = Application.Default.TradingConsoleServer.GetRecoverPasswordData(language, userId);
                    var dict = new Dictionary<string, string>()
                    {
                        {"companyName", companyName},
                        {"disallowLogin", disallowLogin.ToString()},
                        {"isActivateAccount", isActivateAccount.ToString()},
                        {"isDisableJava30", isDisableJava30.ToString()},
                        {"companyLogo",Convert.ToBase64String(companyLogo)},
                        {"colorSettings",colorSettings.OuterXml},
                        {"parameter",systemParameter.OuterXml},
                        {"settings",settings.OuterXml},
                        {"recoverPasswordData",recoverPasswordData.ToXml()},
                        {"tradingAccountData", tradingAccountData.ToXml()},
                        {"userId", userId.ToString()},
                        {"session", session}
                    };
                    result = XmlResultHelper.NewResult(dict);
                    Application.Default.SessionMonitor.Add(session);
                }
                else
                {
                    LoginRetryTimeHelper.IncreaseFailedCount(loginID, ParticipantType.Customer, connectionString);
                    result = XmlResultHelper.ErrorResult;
                }

            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.LoginForJava4:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, this.GetLocalIP());
                result = XmlResultHelper.ErrorResult;
            }
            return result;
        }


        private Guid Login2(string session,string loginID, string password, ref string companyName, out bool disallowLogin, out bool isActivateAccount, out bool isDisableJava30, bool isForJava, string environmentInfo,int appType)
        {
            disallowLogin = false;
            isActivateAccount = false;
            isDisableJava30 = false;
            Guid userID = Guid.Empty;

            if (loginID == String.Empty)
            {
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, GetLocalIP());
                return userID;
            }

            string message = string.Empty;
            try
            {
                userID = Application.Default.ParticipantService.Login(loginID, password);
                if (userID != Guid.Empty)
                {
                    Guid programID = new Guid(SettingManager.Default.GetLoginSetting("TradingConsole"));
                    Guid permissionID = new Guid(SettingManager.Default.GetLoginSetting("Run"));

                    bool isAuthrized = Application.Default.SecurityService.CheckPermission(userID, programID, permissionID, "", "", userID, out message);
                    if (!isAuthrized)
                    {
                        _Logger.ErrorFormat("{0} doesn't have the right to login trader", loginID);
                        userID = Guid.Empty;
                    }
                    else
                    {
                        bool isPathPassed = false;
                        DataSet dataSet = Application.Default.TradingConsoleServer.GetLoginParameters(userID, companyName);
                        DataRowCollection rows = dataSet.Tables[0].Rows;
                        foreach (DataRow row in rows)
                        {
                            isPathPassed = (System.Boolean)row["IsPathPassed"];
                            disallowLogin = (System.Boolean)row["DisallowLogin"];
                            isActivateAccount = (System.Boolean)row["IsActivateAccount"];
                            isDisableJava30 = (System.Boolean)row["IsDisableJava30"];
                            //Java used
                            if (string.IsNullOrEmpty(companyName))
                            {
                                string companyName2 = (System.String)row["Path"];
                                if (companyName2 == string.Empty) companyName2 = "MHL";
                                if (!Directory.Exists(GetOrginazationDir(companyName2)))
                                {
                                    isPathPassed = false;
                                }
                                else
                                {
                                    companyName = companyName2;
                                    isPathPassed = true;
                                }
                            }

                            break;
                        }
                        if (!isPathPassed || disallowLogin)
                        {
                            userID = Guid.Empty;
                        }
                        else
                        {
                            Token token = new Token(Guid.Empty, UserType.Customer, (AppType)appType);
                            token.UserID = userID;
                            token.SessionID = session;
                            SessionManager.Default.AddToken(token.SessionID, token);
                            if (Application.Default.StateServer.Login(token))
                            {
                                if (isForJava)
                                {
                                    //FormsAuthentication.SetAuthCookie(userID.ToString(), false);
                                }

                                //Used for kickout--Michael
                                SessionManager.Default.AddSession(userID, token.SessionID);

                                Application.Default.TradingConsoleServer.SaveLogonLog(token, GetLocalIP(), environmentInfo);
                                //try
                                //{
                                //    this.SilverlightKickoutService.KickoutPredecessor(loginID);
                                //}
                                //catch (Exception exception)
                                //{
                                //    AppDebug.LogEvent("TradingConsole", exception.ToString() + " Return message is: " + message, EventLogEntryType.Error);
                                //    this.SilverlightKickoutService = null;
                                //}
                            }
                            else
                            {
                                AppDebug.LogEvent("TradingConsole.Login2", userID + " StateServer.Login failed", EventLogEntryType.Error);
                                SessionManager.Default.RemoveToken(token.SessionID);
                                userID = Guid.Empty;
                            }
                        }
                    }
                }
                else
                {
                    _Logger.ErrorFormat("{0} is not a valid user", loginID);
                }
            }
            catch (System.Exception e)
            {
                _Logger.Error(e);
            }

            if (userID != Guid.Empty)
            {
              //  CookieContainer cookieContainer = new CookieContainer();
               // Session["CookieContainer"] = cookieContainer;
                //this.AuthenticateDailyChart(loginID, password);
                //this.AuthenticateForTickByTick(loginID, password);
            }
            else
            {
                AuditHelper.AddIllegalLogin(AppType.TradingConsole, loginID, password, this.GetLocalIP());
                Application.Default.TradingConsoleServer.SaveLoginFail(loginID, password, GetLocalIP());
            }
            return userID;
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

        private XmlNode GetParameterForJava(string session,string companyCode, string version)
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
                    state = new TraderState(session);
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

        public System.Xml.XmlNode Logout(string session)
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
