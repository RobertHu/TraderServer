using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;

namespace Trader.Server.Bll
{
    public class ChartQuotationCollection
    {
        private List<ChartQuotation> _chartQuotations = new List<ChartQuotation>();
        private decimal _high = 0;
        private decimal _low = 99999999;

        public List<ChartQuotation> chartQuotations
        {
            get { return this._chartQuotations; }
            set { this._chartQuotations = value; }
        }

        public string dataCycle { get; set; }
        public decimal high
        {
            get { return this._high; }
            set { this._high = value; }
        }

        public decimal low
        {
            get { return this._low; }
            set { this._low = value; }
        }

        internal void Add(ChartQuotation chartQuotation)
        {
            if (this.high < chartQuotation.high) this.high = chartQuotation.high;
            if (this.low > chartQuotation.low) this.low = chartQuotation.low;

            this._chartQuotations.Add(chartQuotation);
        }

        internal static ChartQuotationCollection Create(DataSet dataSet, string dataCycle, string dateFormat)
        {
            ChartQuotationCollection result = new ChartQuotationCollection();
            DataRow[] rows = dataSet.Tables[0].Select(null, "Date");
            foreach (DataRow row in rows)
            {
                ChartQuotation chartQuotation = ChartQuotation.Create(row, dateFormat);
                result.Add(chartQuotation);
            }
            result.dataCycle = dataCycle;
            return result;
        }
    }

    public class ChartQuotation
    {
        internal static string FormatDay = "{0:yyyyMMdd}";
        internal static string FormatMinute = "{0:yyyyMMddHHmm}";
        internal static string FormatSecond = "{0:yyyyMMddHHmmss}";

        public string quoteTime { get; set; }
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }

        //public string dataCycle { get; set; }
        //public double volume { get; set; }
        //public double amount { get; set; }

        //public decimal preClose { get; set; }

        internal static ChartQuotation Create(DataRow row, string dateFormat)
        {
            return new ChartQuotation()
            {
                //dataCycle = dataCycle,
                quoteTime = string.Format(dateFormat, (DateTime)row["Date"]),
                open = (decimal)row["Open"],
                high = (decimal)row["High"],
                low = (decimal)row["Low"],
                close = (decimal)row["Close"]
            };
        }
    }
}