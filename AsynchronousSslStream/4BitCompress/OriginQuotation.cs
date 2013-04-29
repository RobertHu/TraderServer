using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncSslServer._4BitCompress
{
    public class OriginQuotation: QuotationBase
    {
        public OriginQuotation()
            : base()
        {
            this._Data = new Lazy<string>(() =>
            {
                return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}", QuotationBase.InstrumentIdToSequence(base.InstrumentId), this.Origin, base.Ask, base.Bid, base.High, base.Low, base.PrevClose, (long)(base.Timestamp - QuotationBase.OrginTime).TotalSeconds, base.Volume, base.TotalVolume);
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

        public static OriginQuotation From(string data)
        {
            OriginQuotation originQuotation = new OriginQuotation();
            string[] list = data.Split(':');
            originQuotation.InstrumentId = QuotationBase.SequenceToInstrumentId(int.Parse(list[0]));
            originQuotation.Origin = list[1];
            originQuotation.Ask = list[2];
            originQuotation.Bid = list[3];
            originQuotation.High = list[4];
            originQuotation.Low = list[5];
            originQuotation.PrevClose = list[6];
            originQuotation.Timestamp = QuotationBase.OrginTime.AddSeconds(long.Parse(list[7]));
            originQuotation.Volume = list[8];
            originQuotation.TotalVolume = list[9];
            return originQuotation;
        }
    }
}
