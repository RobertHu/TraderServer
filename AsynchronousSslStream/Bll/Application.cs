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
using Trader.Helper;
namespace Trader.Server.Bll
{
    public class Application
    {
        private bool _IsRegistered = true;

        private Application()
        {
            this.ParticipantService = new Trader.Server.Security.ParticipantService.ParticipantServices();
            this.SecurityService = new Trader.Server.Security.SecurityServices.SecurityServices();
            this.StateServer = new StateServerService();
            this.StateServerReadyCheck(this.StateServer);
            this.MarketDepthManager = new MarketDepthManager();
            int tickDataReturnCount = Convert.ToInt32(ConfigurationManager.AppSettings["TickDataReturnCount"]);
            this.TradingConsoleServer = new TradingConsoleServer(SettingManager.Default.ConnectionString, tickDataReturnCount);
            this.AsyncResultManager = new AsyncResultManager(TimeSpan.FromMinutes(30));
            this.AssistantOfCreateChartData2 = new AssistantOfCreateChartData2();
            this.SessionMonitor = new SessionMonitor(SettingManager.Default.SessionExpiredTimeSpan);
            //todo: store/build mobile settings in somewhere
            Dictionary<string, string> mobileSettings = new Dictionary<string, string>();
            mobileSettings.Add("ConnectionString", SettingManager.Default.ConnectionString);
            iExchange3Promotion.Mobile.Manager.Initialize(this.StateServer, mobileSettings);
        }

        public static readonly Application Default = new Application();

        public void Start()
        {
            this.SessionMonitor.Start();
            ReceiveCenter.Default.Start();
            SendCenter.Default.Start();
            AgentController.Default.Start();
        }

        public void Stop()
        {
            this.SessionMonitor.Stop();
            ReceiveCenter.Default.Stop();
            SendCenter.Default.Stop();
            AgentController.Default.Stop();
        }



        public bool IsRegistered
        {
            get
            {
                return this._IsRegistered;
            }
        }
        

        public Trader.Server.Security.ParticipantService.ParticipantServices ParticipantService { get; private set; }
        public Trader.Server.Security.SecurityServices.SecurityServices SecurityService { get; private set; }
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


    }
}
