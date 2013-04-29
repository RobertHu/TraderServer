using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.Time;
using iExchange.Common;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AsyncSslServer.Util;
using AsyncSslServer.TypeExtension;

namespace AsyncSslServer.Bll
{
    public static class TimeService
    {
        public static XmlNode GetTimeInfo()
        {
            try
            {
               TimeInfo info=((ITimeSyncService)Framework.Time.SystemTime.Default).GetTimeInfo();
               string xml = XmlSerializeHelper.ToXml(info.GetType(), info);
               return XmlResultHelper.NewResult(xml);
              
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetTimeInfo:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }
    }
}
