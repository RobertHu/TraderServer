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
        private decimal _low = 99999999;

        public ChartQuotationCollection()
        {
            High = 0;
        }

        public List<ChartQuotation> ChartQuotations
        {
            get { return this._chartQuotations; }
            set { this._chartQuotations = value; }
        }

        public string DataCycle { get; set; }
        public decimal High { get; set; }

        public decimal Low
        {
            get { return this._low; }
            set { this._low = value; }
        }

        internal void Add(ChartQuotation chartQuotation)
        {
            if (this.High < chartQuotation.High) this.High = chartQuotation.High;
            if (this.Low > chartQuotation.Low) this.Low = chartQuotation.Low;

            this._chartQuotations.Add(chartQuotation);
        }

        internal static ChartQuotationCollection Create(DataSet dataSet, string dataCycle, string dateFormat)
        {
            var result = new ChartQuotationCollection();
            DataRow[] rows = dataSet.Tables[0].Select(null, "Date");
            foreach (ChartQuotation chartQuotation in rows.Select(row => ChartQuotation.Create(row, dateFormat)))
            {
                result.Add(chartQuotation);
            }
            result.DataCycle = dataCycle;
            return result;
        }
    }
}