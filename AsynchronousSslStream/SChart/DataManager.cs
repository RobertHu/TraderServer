using System;
using System.Data;
using System.Collections;
using System.Net;

using Easychart.Finance;
using Easychart.Finance.DataProvider;

using StockChart.Common;
using iExchange.Common;
using System.Diagnostics;
using Trader.Server.Annotations;

namespace Trader.Server.SChart
{
	public class DataManager:DataManagerBase
	{
		private static Scheduler _scheduler;
		public static Scheduler Scheduler
		{
			get { return DataManager._scheduler; }
			set { DataManager._scheduler = value; }
		}
	
		public DataManager(InstrumentInfo instrumentInfo,DataCycle dataCycle)
		{
		}

		public static void CallService(CookieContainer cookieContainer)
		{
			
		}
		
		public DataSet GetData(CookieContainer cookieContainer,Guid instrumentId,DateTime lastDate,int count)
		{
            return null;
		}

		public IDataProvider GetData(CookieContainer cookieContainer,string code,int count)
		{

            return null;
		}

		public static Hashtable GetInstrumentCodes(CookieContainer cookieContainer)
		{
            return null;
		}

		public static void ClearHistoryDataCache(CookieContainer cookieContainer,Guid instrumentId)
		{
           
		}

		public static string[] GetDataCycleNames
		{
			get { return null; }
		}

	}
}