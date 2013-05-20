using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server.Session;

namespace Trader.Server._4BitCompress
{
    public class Quotation4Bit
    {
        private OverridedQuotation[] _OverridedQuotations;
        private TraderState _State;
        public long Sequence { get; set; }
        public Quotation4Bit(OverridedQuotation[] overridedQuotations,TraderState state)
        {
            this._OverridedQuotations = overridedQuotations;
            this._State = state;
        }

        public byte[] GetData()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.Sequence);
            stringBuilder.Append("/");

            if (_OverridedQuotations != null && _OverridedQuotations.Length > 0)
            {
                bool addSeprator = false;
                foreach (OverridedQuotation overridedQuotation in this._OverridedQuotations)
                {
                    if (!_State.Instruments.ContainsKey(overridedQuotation.InstrumentID))
                    {
                        continue;
                    }
                    if (overridedQuotation.QuotePolicyID != (Guid)_State.Instruments[overridedQuotation.InstrumentID])
                    {
                        continue;
                    }

                    if (addSeprator)
                    {
                        stringBuilder.Append(";");
                    }
                    else
                    {
                        addSeprator = true;
                    }
                    var oq= string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", GuidMapping.Get(overridedQuotation.InstrumentID), overridedQuotation.Ask, overridedQuotation.Bid, overridedQuotation.High, overridedQuotation.Low, "", (long)(overridedQuotation.Timestamp - QuotationBase.OrginTime).TotalSeconds, overridedQuotation.Volume, overridedQuotation.TotalVolume);
                    stringBuilder.Append(oq);

                }
            }
            stringBuilder.Append("/");
            return Quotation4BitEncoder.Encode(stringBuilder.ToString());

        }

    }
}
