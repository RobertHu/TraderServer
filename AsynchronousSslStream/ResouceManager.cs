using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server.SessionNamespace;
using Trader.Server._4BitCompress;
using Trader.Common;

namespace Trader.Server
{
    public class ResouceManager
    {
        private ResouceManager() { }
        public static readonly ResouceManager Default = new ResouceManager();
        public void ReleaseResource(Session session)
        {
            var traderState = SessionManager.Default.GetTradingConsoleState(session);
            if (traderState != null && traderState.QuotationFilterSign != null)
            {
                QuotationFilterSignMapping.Remove(traderState.QuotationFilterSign);
            }
            SessionManager.Default.RemoveAllItem(session);
            SessionMapping.Remove(session.ToString());
        }
    }
}
