using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server.Session;

namespace Trader.Server
{
    public class ResouceManager
    {
        private ResouceManager() { }
        public static readonly ResouceManager Default = new ResouceManager();
        public void ReleaseResource(string session)
        {
            SessionManager.Default.RemoveAllItem(session);
        }
    }
}
