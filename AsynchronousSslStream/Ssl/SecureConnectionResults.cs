using System;
using System.Net.Security;

namespace Trader.Server.Ssl
{
    public delegate void SecureConnectionResultsCallback(object sender, SecureConnectionResults args);

    public class SecureConnectionResults
    {
        private SslInfo sslInfo;
        private Exception asyncException;

        internal SecureConnectionResults(SslInfo sslInfo)
        {
            this.sslInfo = sslInfo;
        }

        internal SecureConnectionResults(Exception exception)
        {
            this.asyncException = exception;
        }

        public Exception AsyncException { get { return asyncException; } }
        public SslInfo SecureInfo { get { return this.sslInfo; } }
    }
}