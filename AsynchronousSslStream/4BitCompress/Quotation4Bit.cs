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
        private static ConcurrentDictionary<long, Quotation4Bit> _Dict = new ConcurrentDictionary<long, Quotation4Bit>();
        private OverridedQuotation[] _OverridedQuotations;
        private TraderState _State;
        private const int _Capacity = 512;
        private const char _InnerSeparator = ':';
        private const char _StartSeparator='/';
        private const char _OutterSeparator= ';';
        public long Sequence { get; set; }
        private byte[] _Data;
        private object _Lock = new object();

        private Quotation4Bit(OverridedQuotation[] overridedQuotations,TraderState state)
        {
            this._OverridedQuotations = overridedQuotations;
            this._State = state;
           
        }


        public static Quotation4Bit TryAddQuotation(OverridedQuotation[] overridedQuotations, TraderState state, long sequence)
        {
            Quotation4Bit quotation;
            long filterSign = state.SignMapping;
            if (_Dict.TryGetValue(filterSign, out quotation))
            {
                return quotation;
            }
            quotation = new Quotation4Bit(overridedQuotations, state);
            quotation.Sequence = sequence;
            if (_Dict.TryAdd(filterSign, quotation))
            {
                return quotation;
            }
            _Dict.TryGetValue(filterSign, out quotation);
            return quotation;
        }
        public byte[] GetData()
        {
            if (this._Data != null)
            {
                return this._Data;
            }
            lock (this._Lock)
            {
                if (this._Data != null)
                {
                    return this._Data;
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder(_Capacity);
                    stringBuilder.Append(this.Sequence);
                    stringBuilder.Append(_StartSeparator);

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
                                stringBuilder.Append(_OutterSeparator);
                            }
                            else
                            {
                                addSeprator = true;
                            }

                            stringBuilder.Append(GuidMapping.Get(overridedQuotation.InstrumentID));
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(overridedQuotation.Ask);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(overridedQuotation.Bid);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(overridedQuotation.High);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(overridedQuotation.Low);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(string.Empty);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append((long)(overridedQuotation.Timestamp - QuotationBase.OrginTime).TotalSeconds);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(overridedQuotation.Volume);
                            stringBuilder.Append(_InnerSeparator);
                            stringBuilder.Append(overridedQuotation.TotalVolume);
                        }
                    }
                    stringBuilder.Append(_StartSeparator);
                    _Data = Quotation4BitEncoder.Encode(stringBuilder.ToString());
                    return _Data;
                }
            }
        }
        public static void Clear()
        {
            _Dict.Clear();
        }
        
    }
}
