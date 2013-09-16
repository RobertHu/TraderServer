using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using iExchange.Common;
using iExchange.Common.External.Bursa;
using log4net;
using Trader.Server.SessionNamespace;
using System.Xml;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml.Linq;
using Trader.Common;

namespace Trader.Server.Bll
{
    public class InstrumentManager
    {
        public static readonly InstrumentManager Default = new InstrumentManager();
        private readonly ILog _Logger = LogManager.GetLogger(typeof (InstrumentManager));
        private InstrumentManager() { }

        public XElement GetInstrumentForSetting(Session session)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                XmlNode content= Application.Default.TradingConsoleServer.GetInstrumentForSetting(token, Application.Default.StateServer);
                return XmlResultHelper.NewResult(content.OuterXml);
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }


        public XElement UpdateInstrumentSetting(Session session, string[] instrumentIDs)
        {
            try
            {
                Token token = SessionManager.Default.GetToken(session);
                var ds = this.UpdateInstrumentSetting(session,token, instrumentIDs);
                ds.SetInstrumentGuidMapping();
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (Exception exception)
            {
                _Logger.Error(exception);
                return XmlResultHelper.ErrorResult;
            }
        }

        private DataSet UpdateInstrumentSetting(Session session, Token token, string[] instrumentIDs)
        {
            DataSet dataSet = Application.Default.TradingConsoleServer.GetUpdateInstrumentSetting(token, Application.Default.StateServer, instrumentIDs);
            var state = SessionManager.Default.GetTradingConsoleState(session);
            if (state == null) return null;
            Application.Default.TradingConsoleServer.UpdateInstrumentSetting(dataSet, instrumentIDs, state);
            state.CaculateQuotationFilterSign();
            return dataSet;
        }


        public void UpdateInstrumentSetting(Session session, Dictionary<Guid, Guid> quotePolicyIds)
        {
            var state = SessionManager.Default.GetTradingConsoleState(session);
            if (state == null) return;
            state.Instruments.Clear();
            foreach (var pair in quotePolicyIds)
            {
                state.Instruments.Add(pair.Key, pair.Value);
            }
            state.CaculateQuotationFilterSign();
        }
    }
}
