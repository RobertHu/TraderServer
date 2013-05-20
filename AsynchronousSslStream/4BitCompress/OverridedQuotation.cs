using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server._4BitCompress
{
    /*public class OverridedQuotation : QuotationBase
    {
         public OverridedQuotation()
            : base()
        {
            this._Data = new Lazy<string>(() =>
                {
                    return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", GuidMapping.Get(base.InstrumentId), base.Ask, base.Bid, base.High, base.Low, base.PrevClose, (long)(base.Timestamp - QuotationBase.OrginTime).TotalSeconds, base.Volume, base.TotalVolume);
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
    }*/
}
