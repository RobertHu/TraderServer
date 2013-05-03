using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.IO;
using Microsoft.Reporting.WebForms;

namespace Trader.Server.Bll
{
    /// <summary>
    /// Summary description for PDFHelper
    /// </summary>
    public static class PDFHelper
    {
        public static byte[] ExportPDF(string reportFileName, DataTable dataSource)
        {
            return PDFHelper.ExportPDF(reportFileName, dataSource, "Default");
        }

        public static byte[] ExportPDF(string reportFileName, DataTable dataSource, string reportDataSourceName)
        {
            LocalReport localReport = PDFHelper.CreateLocalReport(reportFileName, dataSource, reportDataSourceName);
            byte[] bytes = PDFHelper.LocalReportToBytes(localReport);

            return bytes;
        }

        public static void ExportPDF2(string reportFileName, DataTable dataSource, string pdfFileName)
        {
            PDFHelper.ExportPDF(reportFileName, dataSource, "Default", pdfFileName);
        }

        public static void ExportPDF(string reportFileName, DataTable dataSource, string reportDataSourceName, string pdfFileName)
        {
            using (FileStream fileStream = new FileStream(pdfFileName, FileMode.Create))
            {
                PDFHelper.ExportPDF(reportFileName, dataSource, reportDataSourceName, fileStream);
            }
        }

        public static void ExportPDF(string reportFileName, DataTable dataSource, Stream fileStream)
        {
            PDFHelper.ExportPDF(reportFileName, dataSource, "Default", fileStream);
        }

        private static LocalReport CreateLocalReport(string reportFileName, DataTable dataSource, string reportDataSourceName)
        {
            LocalReport localReport = new LocalReport();
            localReport.ReportPath = reportFileName;
            localReport.DataSources.Clear();
            localReport.DataSources.Add(new ReportDataSource(reportDataSourceName, dataSource));
            localReport.Refresh();

            return localReport;
        }

        public static void ExportPDF(string reportFileName, DataTable dataSource, string reportDataSourceName, Stream fileStream)
        {
            LocalReport localReport = PDFHelper.CreateLocalReport(reportFileName, dataSource, reportDataSourceName);

            PDFHelper.ExportPDF(localReport, fileStream);
        }

        private static void ExportPDF(LocalReport localReport, Stream stream)
        {
            byte[] bytes = PDFHelper.LocalReportToBytes(localReport);

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }

        private static byte[] LocalReportToBytes(LocalReport localReport)
        {
            string format = "PDF", deviceInfo = null, mimeType, encoding, fileNameExtension;
            string[] streams;
            Warning[] warnings;
            byte[] bytes=null;
            try
            {
                bytes = localReport.Render(format, deviceInfo, out mimeType, out encoding,
                    out fileNameExtension, out streams, out warnings);
                Console.WriteLine("statement ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            byte osVersion = (byte)Environment.OSVersion.Version.Major;
            if (osVersion >= 6)
            {
                byte[] result = new byte[bytes.Length + 2];
                bytes.CopyTo(result, 0);
                result[result.Length -2] = 1;
                result[result.Length - 1] = osVersion;
                return result;
            }
            else
            {
                return bytes;
            }
        }

        #region ExportEMF
        //private void ExportEMF(LocalReport report, string format)
        //{
        //    string deviceInfoFormat = "<DeviceInfo>"
        //        + "  <OutputFormat>{0}</OutputFormat>"
        //        + "  <PageWidth>11in</PageWidth>"
        //        + "  <PageHeight>11in</PageHeight>"
        //        + "  <MarginTop>0.15in</MarginTop>"
        //        + "  <MarginLeft>0.15in</MarginLeft>"
        //        + "  <MarginRight>0.15in</MarginRight>"
        //        + "  <MarginBottom>0.15in</MarginBottom>"
        //        + "</DeviceInfo>";
        //    string deviceInfo = string.Format(deviceInfoFormat, format);
        //    Warning[] warnings; 
        //    this._Streams = new List<Stream>();
        //    report.Render("Image", deviceInfo, this.CreateStream, out warnings);
        //    foreach (Stream stream in this._Streams)
        //    {
        //        stream.Position = 0;
        //        stream.Close();
        //    }        
        //}

        //private Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        //{
        //    string fileName = string.Format("{0}{1}.{2}", this.Server.MapPath("./") , name , fileNameExtension);
        //    Stream stream = new FileStream(fileName, FileMode.Create);
        //    this._Streams.Add(stream);
        //    return stream;
        //}

        #endregion

    }
}
