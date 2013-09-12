using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Security.Cryptography;
using System.Collections;
using Trader.Server._4BitCompress;
namespace Trader.Server.SessionNamespace
{
    public class TraderState:TradingConsoleState
    {
        private Dictionary<Guid, Guid> _InstrumentsEx = new Dictionary<Guid, Guid>();

        public TraderState(string sessionId) : base(sessionId) { }
        public TraderState(TradingConsoleState state)
            : base(state.SessionId)
        {
            if (state != null)
            {
                Copy(state.AccountGroups, this.AccountGroups);
                Copy(state.Accounts, this.Accounts);
                Copy(state.Instruments, this.Instruments);
                foreach(DictionaryEntry item in state.Instruments)
                {
                    _InstrumentsEx.Add((Guid)item.Key, (Guid)item.Value);
                }
                this.Language = state.Language;
                this.IsEmployee = state.IsEmployee;
            }
        }

        public Dictionary<Guid, Guid> InstrumentsEx { get { return this._InstrumentsEx; } }

        public void AddInstrumentIDToQuotePolicyMapping(Guid instrumentID,Guid quotePolicyID)
        {
            if (!this._InstrumentsEx.ContainsKey(instrumentID))
            {
                this._InstrumentsEx.Add(instrumentID, quotePolicyID);
                this.Instruments.Add(instrumentID, quotePolicyID);
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

        public long SignMapping { get; private set; }

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
            this.SignMapping = QuotationFilterSignMapping.AddSign(this.QuotationFilterSign);
        }

    }
}
