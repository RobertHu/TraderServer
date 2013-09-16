using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using log4net;
using Trader.Server.TypeExtension;
using Trader.Server.Util;
namespace Trader.Server.Bll
{
    public static class NewsService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof (NewsService));
        public static XElement GetNewsList2(string newsCategoryID, string language, DateTime date)
        {
            try
            {
                DataSet ds=Application.Default.TradingConsoleServer.GetNewsList2(newsCategoryID, language, date);
                return XmlResultHelper.NewResult( ds.ToXml());
            }
            catch (System.Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
