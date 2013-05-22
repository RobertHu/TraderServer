using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server.Session;
using System.Collections.Concurrent;

namespace Trader.Server._4BitCompress
{
    public class Quotation4Bit
    {
        private OverridedQuotation[] _OverridedQuotations;
        private TraderState _State;
        private const int _Capacity = 512;
        public long Sequence { get; set; }
        private static ConcurrentDictionary<long, Quotation4Bit> _Dict = new ConcurrentDictionary<long, Quotation4Bit>();
        private Lazy<string> _PriceString;
        private Lazy<byte[]> _PriceData;

        private Quotation4Bit(OverridedQuotation[] overridedQuotations,TraderState state)
        {
            this._OverridedQuotations = overridedQuotations;
            this._State = state;
            this._PriceString = new Lazy<string>(() =>
            {
                char innerSeparator = ':';
                StringBuilder stringBuilder = new StringBuilder(_Capacity);
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

                        // var oq= string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", GuidMapping.Get(overridedQuotation.InstrumentID), overridedQuotation.Ask, overridedQuotation.Bid, overridedQuotation.High, overridedQuotation.Low, "", (long)(overridedQuotation.Timestamp - QuotationBase.OrginTime).TotalSeconds, overridedQuotation.Volume, overridedQuotation.TotalVolume);
                        stringBuilder.Append(GuidMapping.Get(overridedQuotation.InstrumentID));
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(overridedQuotation.Ask);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(overridedQuotation.Bid);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(overridedQuotation.High);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(overridedQuotation.Low);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(string.Empty);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append((long)(overridedQuotation.Timestamp - QuotationBase.OrginTime).TotalSeconds);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(overridedQuotation.Volume);
                        stringBuilder.Append(innerSeparator);
                        stringBuilder.Append(overridedQuotation.TotalVolume);
                    }
                }
                stringBuilder.Append("/");
                return stringBuilder.ToString();
            });
            this._PriceData = new Lazy<byte[]>(() => Quotation4BitEncoder.Encode(this._PriceString.Value));
        }

        public static Quotation4Bit TryAddQuotation(OverridedQuotation[] overridedQuotations, TraderState state,long sequence)
        {
            Quotation4Bit quotation;
            long filterSign = state.SignMapping;
            if (_Dict.TryGetValue(filterSign, out quotation))
            {
                quotation.Sequence = sequence;
                return quotation;
            }
            quotation = new Quotation4Bit(overridedQuotations, state);
            if (_Dict.TryAdd(filterSign, quotation))
            {
                quotation.Sequence = sequence;
                return quotation;
            }
            _Dict.TryGetValue(filterSign, out quotation);
            quotation.Sequence = sequence;
            return quotation;
        }
        public byte[] GetData()
        {
            return this._PriceData.Value;
        }
        public static void Clear()
        {
            _Dict.Clear();
        }
    }
}
