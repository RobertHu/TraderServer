using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Diagnostics;
using log4net;
using Trader.Common;

namespace Trader.Server.Ssl
{
    public class SecureTcpServer : IDisposable
    {
        private ILog _Logger = LogManager.GetLogger(typeof(SecureTcpServer));
        private X509Certificate _ServerCertificate;
        private RemoteCertificateValidationCallback _CertValidationCallback;
        private SecureConnectionResultsCallback _ConnectionCallback;
        private bool _Started = false;
        private int _ListenPort;
        private TcpListener _ListenerV4;
        private TcpListener _ListenerV6;
        private int _Disposed = 0;
        private bool _ClientCertificateRequired;
        private bool _CheckCertifcateRevocation;
        private SslProtocols _SslProtocols;

        public SecureTcpServer(int listenPort, X509Certificate serverCertificate, SecureConnectionResultsCallback callback)
            : this(listenPort, serverCertificate, callback, null)
        {
        }

        public SecureTcpServer(int listenPort, X509Certificate serverCertificate, SecureConnectionResultsCallback callback, RemoteCertificateValidationCallback certValidationCallback)
        {
            if (listenPort < 0 || listenPort > UInt16.MaxValue) throw new ArgumentOutOfRangeException("listenPort");
            if (serverCertificate == null) throw new ArgumentNullException("serverCertificate");
            if (callback == null) throw new ArgumentNullException("callback");

            this._ServerCertificate = serverCertificate;
            this._CertValidationCallback = certValidationCallback;
            this._ConnectionCallback = callback;
            this._ListenPort = listenPort;
            this._CheckCertifcateRevocation = false;
            this._ClientCertificateRequired = false;
            this._SslProtocols = SslProtocols.Default;
        }

        ~SecureTcpServer()
        {
            Dispose();
        }

        public SslProtocols SslProtocols
        {
            get { return _SslProtocols; }
            set { _SslProtocols = value; }
        }

        public bool CheckCertifcateRevocation
        {
            get { return _CheckCertifcateRevocation; }
            set { _CheckCertifcateRevocation = value; }
        }


        public bool ClientCertificateRequired
        {
            get { return _ClientCertificateRequired; }
            set { _ClientCertificateRequired = value; }
        }

        public void StartListening()
        {
            if (this._Started) throw new InvalidOperationException("Already started...");
            IPEndPoint localIP;
            if (Socket.OSSupportsIPv4 && this._ListenerV4 == null)
            {
                localIP = new IPEndPoint(IPAddress.Any, this._ListenPort);
                Console.WriteLine("SecureTcpServer: Started listening on {0}", localIP);
                this._ListenerV4 = new TcpListener(localIP);
            }

            if (Socket.OSSupportsIPv6 && this._ListenerV6 == null)
            {
                localIP = new IPEndPoint(IPAddress.IPv6Any, this._ListenPort);
                Console.WriteLine("SecureTcpServer: Started listening on {0}", localIP);
                this._ListenerV6 = new TcpListener(localIP);
            }

            if (this._ListenerV4 != null)
            {
                this._ListenerV4.Start();
                this._ListenerV4.BeginAcceptTcpClient(this.OnAcceptConnection, this._ListenerV4);
            }

            if (this._ListenerV6 != null)
            {
                this._ListenerV6.Start();
                this._ListenerV6.BeginAcceptTcpClient(this.OnAcceptConnection, this._ListenerV6);
            }

            this._Started = true;
        }

        public void StopListening()
        {
            if (!this._Started) return;
            this._Started = false;
            if (_ListenerV4 != null) _ListenerV4.Stop();
            if (_ListenerV6 != null) _ListenerV6.Stop();
        }

        private void OnAcceptConnection(IAsyncResult result)
        {
            if (!this._Started) return;
            TcpListener listener = result.AsyncState as TcpListener;
            SslStream sslStream = null;
            try
            {
                listener.BeginAcceptTcpClient(this.OnAcceptConnection, listener);
                //complete the last operation...
                TcpClient client = listener.EndAcceptTcpClient(result);
                bool leaveStreamOpen = false; //close the socket when done
                TraderNetworkStream networkStream = new TraderNetworkStream(client.Client,BufferManager.Default.SetBuffer());
                if (this._CertValidationCallback != null)
                {
                    sslStream = new SslStream(networkStream, leaveStreamOpen, this._CertValidationCallback);
                }
                else
                {
                    sslStream = new SslStream(networkStream, leaveStreamOpen);
                }
                SslInfo sslInfo = new SslInfo(networkStream, sslStream);
                sslStream.BeginAuthenticateAsServer(this._ServerCertificate,
                    this._ClientCertificateRequired,
                    this._SslProtocols,
                    this._CheckCertifcateRevocation, //checkCertifcateRevocation
                    this.OnAuthenticateAsServer,
                    sslInfo);

            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                if (sslStream != null)
                {
                    sslStream.Dispose();
                    sslStream = null;
                }
                this._ConnectionCallback(this, new SecureConnectionResults(ex));
            }
        }

        private void OnAuthenticateAsServer(IAsyncResult result)
        {
            SslInfo sslInfo = null;
            try
            {
                sslInfo = result.AsyncState as SslInfo;
                sslInfo.SslStream.EndAuthenticateAsServer(result);
                this._ConnectionCallback(this, new SecureConnectionResults(sslInfo));
            }
            catch (Exception ex)
            {
                if (sslInfo.SslStream != null)
                {
                    sslInfo.SslStream.Dispose();
                }
                this._ConnectionCallback(this, new SecureConnectionResults(ex));
            }
        }

        public void Dispose()
        {
            if (System.Threading.Interlocked.Increment(ref this._Disposed) == 1)
            {
                if (this._ListenerV4 != null) this._ListenerV4.Stop();
                if (this._ListenerV6 != null) this._ListenerV6.Stop();
                this._ListenerV4 = null;
                this._ListenerV6 = null;
                GC.SuppressFinalize(this);
            }
        }
    }

    public class SslInfo
    {
        public SslInfo(TraderNetworkStream stream1, SslStream stream2)
        {
            this.NetworkStream = stream1;
            this.SslStream = stream2;
        }
        public TraderNetworkStream NetworkStream { get; private set; }
        public SslStream SslStream { get; private set; }
    }
}