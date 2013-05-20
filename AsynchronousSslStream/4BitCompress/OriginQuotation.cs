using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server._4BitCompress
{
    public class OriginQuotation: QuotationBase
    {
        public OriginQuotation()
            : base()
        {
            this._Data = new Lazy<string>(() =>
            {
                return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}", GuidMapping.Get(base.InstrumentId), this.Origin, base.Ask, base.Bid, base.High, base.Low, base.PrevClose, (long)(base.Timestamp - QuotationBase.OrginTime).TotalSeconds, base.Volume, base.TotalVolume);
            });
        }

        public string Origin;
        private Lazy<string> _Data;

        public override string ToString()
        {
            return string.Format("Origin={0},{1}", this.Origin, base.ToString());
        }

        public string GetValue()
        {
            return this._Data.Value;
        }

    }
}
