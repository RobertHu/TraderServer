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
using Trader.Common;
using Trader.Server.Ssl;
namespace Trader.Server.TypeExtension
{
    public static class DataSetExtension
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(DataSetExtension));
        private const int CAPACITY = 10 * 1024;
        public static string ToXml(this DataSet dataset)
        {
            if (dataset == null)
            {
                return String.Empty;
            }
            using (var ms = new TraderMemoryStream(CAPACITY))
            {
                using (TextWriter tw = new StreamWriter(ms))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));
                    xmlSerializer.Serialize(tw, dataset);
                    string xml = Constants.ContentEncoding.GetString(ms.ToArray());
                    ms.Buffer.Dispose();
                    return xml;
                }
            }
        }

        public static UnmanagedMemory ToPointer(this DataSet dataset)
        {
            if (dataset == null)
            {
                return null;
            }
            using (var ms = new TraderMemoryStream(CAPACITY))
            {
                using (TextWriter tw = new StreamWriter(ms))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));
                    xmlSerializer.Serialize(tw, dataset);
                    return ms.Buffer;
                }
            }
        }

        public static void SetInstrumentGuidMapping(this DataSet ds)
        {
            try
            {
                if (ds == null)
                {
                    return;
                }
                DataTable table = ds.Tables[InstrumentConstants.InstrumentTableName];
                if (table == null) return;
                DataColumn column = new DataColumn();
                column.ColumnName = InstrumentConstants.IntForIntrumentIDColumnName;
                column.DataType = typeof(Int32);
                column.AutoIncrement = false;
                table.Columns.Add(column);
                DataRowCollection rowCol = table.Rows;
                for (int rowIndex = 0; rowIndex < rowCol.Count; rowIndex++)
                {
                    DataRow dr = rowCol[rowIndex];
                    Guid id = (Guid)dr[InstrumentConstants.InstrumentIDColumnName];
                    int mappingId = GuidMapping.Add(id);
                    dr[InstrumentConstants.IntForIntrumentIDColumnName] = mappingId;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }

        public static void SetCommandSequence(this DataSet ds, int commandSequence)
        {
            try
            {
                if (ds == null)
                {
                    return;
                }
                DataTable table = new DataTable(InstrumentConstants.CommandSequenceTableName);
                DataColumn column = new DataColumn(InstrumentConstants.CommandSequenceColumnName);
                column.DataType = typeof(Int32);
                column.AutoIncrement = false;
                table.Columns.Add(column);
                DataRow dr = table.NewRow();
                dr[InstrumentConstants.CommandSequenceColumnName] = commandSequence;
                table.Rows.Add(dr);
                ds.Tables.Add(table);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex);
            }
        }


    }
}
