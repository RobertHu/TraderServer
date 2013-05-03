using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Security.Cryptography;
using System.Collections;
namespace Trader.Server.Session
{
    public class TraderState:TradingConsoleState
    {
        public TraderState(string sessionId) : base(sessionId) { }
        public TraderState(TradingConsoleState state)
            : base(state.SessionId)
        {
            if (state != null)
            {
                Copy(state.AccountGroups, this.AccountGroups);
                Copy(state.Accounts, this.Accounts);
                Copy(state.Instruments, this.Instruments);
                this.Language = state.Language;
                this.IsEmployee = state.IsEmployee;
            }
        }

        private void Copy(Hashtable source,Hashtable destination)
        {
            foreach (DictionaryEntry item in source)
            {
                destination.Add(item.Key, item.Value);
            }
        }

        public string QuotationFilterSign { get; private set; }

        public void CaculateQuotationFilterSign()
        {
            List<Guid> instrumentIds = new List<Guid>(this.Instruments.Keys.Cast<Guid>());
            instrumentIds.Sort();
            StringBuilder sb = new StringBuilder();
            foreach (Guid instrumentId in instrumentIds)
            {
                sb.Append(instrumentId);
                sb.Append(this.Instruments[instrumentId]);
            }
            byte[] sign = MD5.Create().ComputeHash(ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
            this.QuotationFilterSign = Convert.ToBase64String(sign);
        }

    }
}
