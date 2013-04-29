using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncSslServer._4BitCompress
{
    public class OverridedQuotation : QuotationBase
    {
         public OverridedQuotation()
            : base()
        {
            this._Data = new Lazy<string>(() =>
                {
                    return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", QuotationBase.InstrumentIdToSequence(base.InstrumentId), base.Ask, base.Bid, base.High, base.Low, base.PrevClose, (long)(base.Timestamp - QuotationBase.OrginTime).TotalSeconds, base.Volume, base.TotalVolume);
                });
        }
        
        public Guid QuotePolicyId;
        private Lazy<string> _Data;

        public override string ToString()
        {
            return string.Format("QuotePolicyId={0},{1}", this.QuotePolicyId, base.ToString());
        }

        public string GetValue()
        {
            return this._Data.Value;
        }

        public static OverridedQuotation From(string data)
        {
            OverridedQuotation overridedQuotation = new OverridedQuotation();
            string[] list = data.Split(':');
            overridedQuotation.InstrumentId = QuotationBase.SequenceToInstrumentId(int.Parse(list[0]));
            overridedQuotation.Ask = list[1];
            overridedQuotation.Bid = list[2];
            overridedQuotation.High = list[3];
            overridedQuotation.Low = list[4];
            overridedQuotation.PrevClose = list[5];
            overridedQuotation.Timestamp = QuotationBase.OrginTime.AddSeconds(long.Parse(list[6]));
            overridedQuotation.Volume = list[7];
            overridedQuotation.TotalVolume = list[8];
            return overridedQuotation;
        }
    }
}
