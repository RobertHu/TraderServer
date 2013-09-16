using System;
using System.Configuration;
using log4net;
namespace Trader.Server
{
    public class SettingManager
    {
        public static readonly SettingManager Default = new SettingManager();
        private readonly ILog _Logger = LogManager.GetLogger(typeof(SettingManager));
        private SettingManager()
        {
            try
            {
                this.PhysicPath = GetSettingFromAppSettingConfig("physicPath");
                this.ConnectionString = GetSettingFromAppSettingConfig("connectionString");
                this.ConnectionStringForReport = GetSettingFromAppSettingConfig("connectionStringForReport");
                this.BackofficeServiceUrl = GetSettingFromAppSettingConfig("backofficeServiceUrl");
                this.ServerPort = int.Parse(GetSettingFromAppSettingConfig("serverPort"));
                this.SessionExpiredTimeSpan = new TimeSpan(0, int.Parse(GetSettingFromAppSettingConfig("SessionExpiredTimeSpan")), 0);
                this.CommandUrl = GetSettingFromAppSettingConfig("commandUrl");
                this.CertificatePath = GetSettingFromAppSettingConfig("CertificatePath");
                this.ParticipantServiceUrl = GetSettingFromAppSettingConfig("ParticipantServiceUrl");
                this.SecurityServiceUrl = GetSettingFromAppSettingConfig("SecurityServiceUrl");
                this.StateServerUrl = GetSettingFromAppSettingConfig("iExchange.StateServer.Service");
                this.PriceSendPeriodInMilisecond = int.Parse(GetSettingFromAppSettingConfig("PriceSendPeriodInMilisecond"));
                this.IsTest = ConvertBitZeroOrOneToBoolean("IsTest");
                this.IsSendPriceImmediately = ConvertBitZeroOrOneToBoolean("IsSendPriceImmediately");
            }
            catch (Exception ex)
            {
                _Logger.ErrorFormat("Load setting failed: {0}", ex);
                Console.WriteLine(ex);

            }
        }

        public int ServerPort { get; private set; }

        public string PhysicPath { get; private set; }
        public string BackofficeServiceUrl { get; private set; }

        public string ConnectionString { get; private set; }

        public string ConnectionStringForReport { get; private set; }

        public TimeSpan SessionExpiredTimeSpan { get; set; }
        public string CommandUrl { get; private set; }
        public string CertificatePath { get; private set; }
        public int PriceSendPeriodInMilisecond { get; private set; }

        public string SecurityServiceUrl { get; private set; }
        public string ParticipantServiceUrl { get; private set; }
        public string StateServerUrl { get; private set; }
        public bool IsTest { get; private set; }
        public bool IsSendPriceImmediately { get; private set; }

        private bool ConvertBitZeroOrOneToBoolean(string configItemName)
        {
            return GetSettingFromAppSettingConfig(configItemName) == "1";
        }

        public string GetLoginSetting(string key)
        {
            try
            {
                return GetSettingFromAppSettingConfig(key);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                return string.Empty;
            }
            
        }

        private string GetSettingFromAppSettingConfig(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("{0} can't be found",key), ex);
            }
        }




    }
}
