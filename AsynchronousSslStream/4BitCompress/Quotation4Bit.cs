using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server._4BitCompress
{
    public class Quotation4Bit
    {
        public OriginQuotation[] OriginQuotations;
        public OverridedQuotation[] OverridedQuotations;

        public byte[] QuotationInBytes;     
        public long Sequence { get; set; }
        public Lazy<string> Data;
        public Lazy<byte[]> DataInBytes;
        public Quotation4Bit()
        {
            this.Data = new Lazy<string>(() =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(this.Sequence);
                stringBuilder.Append("/");

                if (OverridedQuotations != null && OverridedQuotations.Length > 0)
                {
                    bool addSeprator = false;
                    foreach (OverridedQuotation overridedQuotation in this.OverridedQuotations)
                    {
                        if (addSeprator)
                        {
                            stringBuilder.Append(";");
                        }
                        else
                        {
                            addSeprator = true;
                        }
                        stringBuilder.Append(overridedQuotation.GetValue());

                    }
                }
                stringBuilder.Append("/");

                if (OriginQuotations != null && OriginQuotations.Length > 0)
                {
                    bool addSeprator = false;
                    foreach (OriginQuotation originQuotation in this.OriginQuotations)
                    {
                        if (addSeprator)
                        {
                            stringBuilder.Append(";");
                        }
                        else
                        {
                            addSeprator = true;
                        }
                        stringBuilder.Append(originQuotation.GetValue());
                    }
                }

                return stringBuilder.ToString();
            });

            this.DataInBytes = new Lazy<byte[]>(() =>
            {
                return Quotation4BitEncoder.Encode(this.Data.Value);
            });
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (OriginQuotations != null && OriginQuotations.Length > 0)
            {
                stringBuilder.Append("Origins=");
                foreach (OriginQuotation originQuotation in this.OriginQuotations)
                {
                    stringBuilder.Append(originQuotation);
                    stringBuilder.Append(";");
                }
                stringBuilder.Append("\t");
            }

            if (OverridedQuotations != null && OverridedQuotations.Length > 0)
            {
                stringBuilder.Append("Overrideds=");
                foreach (OverridedQuotation overridedQuotation in this.OverridedQuotations)
                {
                    stringBuilder.Append(overridedQuotation);
                }
            }

            return stringBuilder.ToString();
        }

    }
}
