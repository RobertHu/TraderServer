using System;
using System.Data;

namespace Trader.Server.Bll
{
    public class ChartQuotation
    {
        internal static string FormatDay = "{0:yyyyMMdd}";
        internal static string FormatMinute = "{0:yyyyMMddHHmm}";
        internal static string FormatSecond = "{0:yyyyMMddHHmmss}";

        public string QuoteTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        internal static ChartQuotation Create(DataRow row, string dateFormat)
        {
            return new ChartQuotation()
            {
                QuoteTime = string.Format(dateFormat, (DateTime)row["Date"]),
                Open = (decimal)row["Open"],
                High = (decimal)row["High"],
                Low = (decimal)row["Low"],
                Close = (decimal)row["Close"]
            };
        }
    }
}