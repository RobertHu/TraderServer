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
using AsyncSslServer;
using Trader.Server.Service;
using Trader.Server.Setting;
using Trader.Server.Bll;
using Trader.Server.Util;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Trader.Server 
{
    class ServerProgram
    {
        private static ILog _Log = LogManager.GetLogger(typeof(ServerProgram));
        private static CommandCollectorHost _CommandCollectorHost = new CommandCollectorHost();
        static void Main(string[] args)
        {
            Start();
        }

        private static void Start()
        {
            SecureTcpServer server = null;
            try
            {
                _Log.InfoFormat("{0} certificate path", SettingManager.Default.CertificatePath);
                X509Certificate serverCert = X509Certificate.CreateFromCertFile(SettingManager.Default.CertificatePath);
                server = new SecureTcpServer(SettingManager.Default.ServerPort, serverCert, ServerProgram.OnServerConnectionAvailable, null);
                _Log.Info("Server Start");
                server.StartListening();
                _CommandCollectorHost.Start();
                Application.Default.Start();
                Console.WriteLine("Press Q(q) to exit.");
                for (; ; )
                {
                    char keyChar = Console.ReadKey().KeyChar;
                    if (keyChar == 'Q' || keyChar == 'q') break;
                }
            }
            catch (Exception ex)
            {
                _Log.Error(ex);
            }
            finally
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
                _Log.Info("Server Closed");
            }
        }


        static void OnServerConnectionAvailable(object sender, SecureConnectionResults args)
        {
            if (args.AsyncException != null)
            {
                _Log.ErrorFormat("Client connection failed {0}", args.AsyncException);
                return;
            }
            try
            {
                SslStream sslStream = args.SecureStream;
                Guid session = Guid.NewGuid();
                var receiveAgent = new ReceiveManager.ReceiveAgent(ClientRequestHelper.Process);
                var client = new Communication.Client(sslStream, session);
                var iClient = client as Trader.Helper.Common.ICommunicationAgent;
                iClient.DataArrived += ReceiveCenter.Default.DataArrivedHandler;
                iClient.Closed += AgentController.Default.SenderClosedEventHandle;
                var iReceiver = receiveAgent as Trader.Helper.Common.IReceiveAgent;
                iReceiver.ResponseSent += SendCenter.Default.ResponseSentHandle;
                AgentController.Default.Add(session, receiveAgent, client);
            }
            catch (Exception ex)
            {
                _Log.Error(ex);
            }
        }
    }
}
