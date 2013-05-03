using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using log4net;

namespace Trader.Server.Service
{
    public class CommandCollectorHost
    {
        private ServiceHost _ServiceHost;
        private ILog _Logger = LogManager.GetLogger(typeof(CommandCollectorHost));
        public bool Start()
        {
            try
            {
                this._ServiceHost = new ServiceHost(typeof(CommandCollectService));
                this._ServiceHost.Open();
                return true;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                this._ServiceHost.Close();
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }
    }
}
