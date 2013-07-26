using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common.Client;
using System.Net;
using iExchange.Common;
using System.Threading;
using System.Configuration;
using Trader.Server.Setting;
using Trader.Server.Service;
using Trader.Server.Ssl;
using Mobile = iExchange3Promotion.Mobile;

namespace Trader.Server.Bll
{
    public class Application
    {
        private bool _IsRegistered = true;
        private TradeDayChecker _TradeDayChecker;

        private Application()
        {
            this.ParticipantService = new ParticipantServices();
            this.ParticipantService.Url = SettingManager.Default.ParticipantServiceUrl;
            this.SecurityService = new SecurityServices();
            this.SecurityService.Url = SettingManager.Default.SecurityServiceUrl;
            this.StateServer = new StateServerService();
            this.StateServer.Url = SettingManager.Default.StateServerUrl;
            this.StateServerReadyCheck(this.StateServer);
            this.MarketDepthManager = new MarketDepthManager();
            int tickDataReturnCount = Convert.ToInt32(ConfigurationManager.AppSettings["TickDataReturnCount"]);
            this.TradingConsoleServer = new TradingConsoleServer(SettingManager.Default.ConnectionString, tickDataReturnCount);
            this.AsyncResultManager = new AsyncResultManager(TimeSpan.FromMinutes(30));
            this.AssistantOfCreateChartData2 = new AssistantOfCreateChartData2();
            this.SessionMonitor = new SessionMonitor(SettingManager.Default.SessionExpiredTimeSpan);
            this._TradeDayChecker = new TradeDayChecker();
            //todo: store/build mobile settings in somewhere
            Dictionary<string, string> mobileSettings = new Dictionary<string, string>();
            mobileSettings.Add("ConnectionString", SettingManager.Default.ConnectionString);
            Mobile.Manager.Initialize(this.StateServer, mobileSettings, this.MobileSendingCallback);
        }

        public static readonly Application Default = new Application();

        public void Start()
        {
            this.SessionMonitor.Start();
            ReceiveCenter.Default.Start();
            SendCenter.Default.Start();
            AgentController.Default.Start();
            CommandManager.Default.Start();
            TaskQueue.Default.Start();
            QuotationDispatcher.Default.Initialize(SettingManager.Default.PriceSendPeriodInMilisecond,SettingManager.Default.IsSendPriceImmediately);
            if (!SettingManager.Default.IsTest)
            {
                this._TradeDayChecker.Start(SettingManager.Default.ConnectionString);
            }
        }

        public void Stop()
        {
            this.SessionMonitor.Stop();
            ReceiveCenter.Default.Stop();
            SendCenter.Default.Stop();
            AgentController.Default.Stop();
            CommandManager.Default.Stop();
            TaskQueue.Default.Stop();
            QuotationDispatcher.Default.Stop();
            this._TradeDayChecker.Stop();
        }

        public bool IsRegistered
        {
            get
            {
                return this._IsRegistered;
            }
        }
        

        public ParticipantServices ParticipantService { get; private set; }
        public SecurityServices SecurityService { get; private set; }
        public StateServerService StateServer { get; private set; }
        public TradingConsoleServer TradingConsoleServer { get; private set; }
        public MarketDepthManager MarketDepthManager { get; set; }
        public AsyncResultManager AsyncResultManager { get; set; }
        public AssistantOfCreateChartData2 AssistantOfCreateChartData2 { get; set; }
        public SessionMonitor  SessionMonitor { get; private set; }

        private void StateServerReadyCheck(StateServerService stateServer)
        {
            try
            {
                Token token = new Token();
                token.AppType=AppType.TradingConsoleSilverLight;
                stateServer.Register(token, SettingManager.Default.CommandUrl);
                stateServer.HelloWorld();
            }
            catch (WebException webException)
            {
                if (webException.Status == WebExceptionStatus.Timeout)
                {
                    AppDebug.LogEvent("[TradingConsole] Application_Start", webException.ToString(), System.Diagnostics.EventLogEntryType.Error);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    this.StateServerReadyCheck(stateServer);
                }
            }
        }

        public void MobileSendingCallback(object sender, EventArgs args)
        {
            Mobile.SendCommandEventArg eventArg = args as Mobile.SendCommandEventArg;

            Trader.Server.Ssl.Client client = AgentController.Default.GetSender(long.Parse(eventArg.SessionId));
           /* client.Send(Serialization.SerializeManager.Default.Serialize(new Serialization.SerializedObject(eventArg.SessionId,eventArg.XElement, null)));*/

        }
    }
}
