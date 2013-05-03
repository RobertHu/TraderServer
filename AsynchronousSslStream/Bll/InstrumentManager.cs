using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using Trader.Server.Session;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;

namespace Trader.Server.Bll
{
    public class InstrumentManager
    {
        private InstrumentManager() { }
        public static readonly InstrumentManager Default = new InstrumentManager();

        public XmlNode GetInstrumentForSetting(string session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                XmlNode content= Application.Default.TradingConsoleServer.GetInstrumentForSetting(token, Application.Default.StateServer);
                return XmlResultHelper.NewResult(content.OuterXml);
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetInstrumentForSetting:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public XmlNode UpdateInstrumentSetting(string session,string[] instrumentIDs)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                var ds = this.UpdateInstrumentSetting(session,token, instrumentIDs);
                ds.SetInstrumentGuidMapping();
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.UpdateInstrumentSetting:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }

        private DataSet UpdateInstrumentSetting(string session,Token token, string[] instrumentIDs)
        {
            DataSet dataSet = null;
            dataSet = Application.Default.TradingConsoleServer.GetUpdateInstrumentSetting(token, Application.Default.StateServer, instrumentIDs);
            var state = SessionManager.Default.GetTradingConsoleState(session);
            if (state == null) return null;
            Application.Default.TradingConsoleServer.UpdateInstrumentSetting(dataSet, instrumentIDs, state);
            state.CaculateQuotationFilterSign();
            return dataSet;
        }

    }
}
