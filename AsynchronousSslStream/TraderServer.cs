using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using log4net;
using System.Configuration;
using Trader.Helper;
using Trader.Server.Service;
using Trader.Server.Setting;
using Trader.Server.Bll;
using Trader.Server.Util;
using Trader.Common;
using Trader.Server.Ssl;
using CommunicationAgent = Trader.Helper.Common.ICommunicationAgent;
using FsharpReceiveAgent = Trader.Helper.Common.IReceiveAgent;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Trader.Server 
{
   public class TraderServer
    {
        private ILog _Log = LogManager.GetLogger(typeof(TraderServer));
        private  CommandCollectorHost _CommandCollectorHost = new CommandCollectorHost();
        private SecureTcpServer server = null;
      
        public void Start()
        {
            try
            {
                _Log.InfoFormat("{0} certificate path", SettingManager.Default.CertificatePath);
                X509Certificate serverCert = X509Certificate.CreateFromCertFile(SettingManager.Default.CertificatePath);
                server = new SecureTcpServer(SettingManager.Default.ServerPort, serverCert, OnServerConnectionAvailable, null);
                _Log.Info("Server Start");
                server.StartListening();
                _CommandCollectorHost.Start();
                Application.Default.Start();
            }
            catch (Exception ex)
            {
                _Log.Error(ex);
            }
        }

        public void Stop()
        {
            try
            {
                if (server != null)
                {
                    server.StopListening();
                    server.Dispose();
                }

                if (_CommandCollectorHost != null)
                {
                    _CommandCollectorHost.Stop();
                }
                if (Application.Default != null)
                {
                    Application.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                this._Log.Error(ex);
            }
        }


        private void OnServerConnectionAvailable(object sender, SecureConnectionResults args)
        {
            if (args.AsyncException != null)
            {
                _Log.ErrorFormat("Client connection failed {0}", args.AsyncException);
                return;
            }
            try
            {
                SslStream sslStream = args.SecureStream;
                long sessionMappingID = SessionMapping.Get();
                var client = new Trader.Helper.Communication.Client(sslStream, sessionMappingID, ReceiveCenter.Default);
                var iClient = client as Trader.Helper.Common.ICommunicationAgent;
                iClient.Closed += AgentController.Default.SenderClosedEventHandle;
                iClient.BufferIndex = BufferManager.Default.SetBuffer();

                var receiveAgent = new Trader.Helper.ReceiveManager.ReceiveAgent(ClientRequestHelper.Process);
                var iReceiveAgent = receiveAgent as FsharpReceiveAgent;
                AgentController.Default.Add(sessionMappingID,iReceiveAgent,iClient);
            }
            catch (Exception ex)
            {
                _Log.Error(ex);
            }
        }
    }
}
