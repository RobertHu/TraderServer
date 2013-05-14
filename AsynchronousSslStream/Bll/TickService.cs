using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using iExchange.Common;
using Trader.Server.Session;
using System.Data;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Net;
using System.Diagnostics;
using Trader.Server.Service;
namespace Trader.Server.Bll
{
    public static class TickService
    {
        public static XElement GetTickByTickHistoryData(Guid session, Guid instrumentId, DateTime from, DateTime to)
        {
            TradingConsoleState state = SessionManager.Default.GetTradingConsoleState(session);
            XElement  result = null;
            if (state.Instruments.ContainsKey(instrumentId))
            {
                Guid quotePolicyId = (Guid)state.Instruments[instrumentId];
                DataSet ds = Application.Default.TradingConsoleServer.GetTickByTickHistoryDatas2(instrumentId, quotePolicyId, from, to);
                result = XmlResultHelper.NewResult( ds.ToXml());
            }
            else
            {
                result = XmlResultHelper.NewResult(string.Empty);
            }
            return result;

        }


        public static XElement  GetChartData(Guid asyncResultId)
        {
            try
            {
                DataSet ds = (DataSet)Application.Default.AsyncResultManager.GetResult(asyncResultId);
                return XmlResultHelper.NewResult(ds.ToXml());
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.GetChartData", exception.ToString(), EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
        }


        public static XElement  AsyncGetChartData2(Guid session, Guid instrumentId, DateTime from, DateTime to, string dataCycleParameter)
        {
            try
            {
                AsyncResult asyncResult = new AsyncResult("AsyncGetChartData2", session.ToString());
                Application.Default.AssistantOfCreateChartData2.AddTask(asyncResult, new ChartDataArgument2(instrumentId, dataCycleParameter, from, to, asyncResult, session),CreateChartData2);
                return XmlResultHelper.NewResult(asyncResult.Id.ToString());
            }
            catch (System.Exception exception)
            {
                AppDebug.LogEvent("TradingConsole.AsyncGetChartData2:", exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return XmlResultHelper.ErrorResult;
            }
            
        }

        private static void CreateChartData2(object state)
        {
            ChartDataArgument2 chartDataArgument = (ChartDataArgument2)state;
            try
            {
                //DataSet dataSet = this.GetChartData2(chartDataArgument.InstrumentId, chartDataArgument.DataCycle, chartDataArgument.From, chartDataArgument.To);
                //SChart.Datas.StockData stockData = new SChart.Datas.StockData(cookieContainer);
                //DataSet dataSet = stockData.GetChartData2(chartDataArgument.InstrumentId, chartDataArgument.DataCycle, chartDataArgument.From, chartDataArgument.To);
                TradingConsoleState tradingConsoleState = chartDataArgument.TradingConsoleState;
                Guid quotePolicyId = (Guid)tradingConsoleState.Instruments[chartDataArgument.InstrumentId];
                TradingConsoleServer tradingConsoleServer = chartDataArgument.TradingConsoleServer;
                DataSet dataSet = tradingConsoleServer.GetChartData2(chartDataArgument.InstrumentId, quotePolicyId, chartDataArgument.DataCycle, chartDataArgument.From, chartDataArgument.To);
                AsyncResultManager asyncResultManager = chartDataArgument.AsyncResultManager;
                asyncResultManager.SetResult(chartDataArgument.AsyncResult, dataSet);
                CommandManager.Default.AddCommand(chartDataArgument.Token, new AsyncCommand(0, chartDataArgument.AsyncResult));
                //else
                //{
                //    string userIdString = string.Empty;
                //    if (chartDataArgument.Token != null)
                //    {
                //        Token token = chartDataArgument.Token;
                //        userIdString = token.UserID.ToString();
                //    }
                //    AppDebug.LogEvent("TradingConsole.Service.CreateChartData2", "CookieContainer Timeout" + " UserId: " + userIdString, EventLogEntryType.Warning);
                //    Commands commands = chartDataArgument.Commands;
                //    commands.Add(chartDataArgument.Token, new AsyncCommand(0, chartDataArgument.AsyncResult, true, null));
                //}
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("TradingConsole.CreateChartData2", e.ToString(), EventLogEntryType.Error);
                CommandManager.Default.AddCommand(chartDataArgument.Token, new AsyncCommand(0, chartDataArgument.AsyncResult, true, e));
            }
        }
    }
}
