using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using Trader.Server._4BitCompress;
using log4net;
namespace Trader.Server.TypeExtension
{
    public static class DataSetExtension
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(DataSetExtension));
        public static string ToXml(this DataSet dataset)
        {
            if (dataset == null)
            {
                return String.Empty;
            }
            using (var ms = new MemoryStream())
            {
                using (TextWriter tw = new StreamWriter(ms))
                {
                    var xmlSerializer = new XmlSerializer(typeof(DataSet));
                    xmlSerializer.Serialize(tw, dataset);
                    string xml = Encoding.UTF8.GetString(ms.ToArray());
                    return xml;
                }

            }
        }

        public static void SetInstrumentGuidMapping(this DataSet ds)
        {
            try
            {
                DataTable table = ds.Tables["Instrument"];
                if (table == null) return;
                DataColumn column = new DataColumn();
                column.ColumnName = "SequenceForQuotatoin";
                column.DataType = typeof(Int32);
                column.AutoIncrement = false;
                table.Columns.Add(column);
                DataRowCollection rowCol = table.Rows;
                for (int rowIndex = 0; rowIndex < rowCol.Count; rowIndex++)
                {
                    DataRow dr = rowCol[rowIndex];
                    Guid id = (Guid)dr["ID"];
                    int mappingId = GuidMapping.Add(id);
                    dr["SequenceForQuotatoin"] = mappingId;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }


    }
}
