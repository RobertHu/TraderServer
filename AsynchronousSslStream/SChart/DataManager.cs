using System;
using System.Data;
using System.Collections;
using System.Net;

using Easychart.Finance;
using Easychart.Finance.DataProvider;

using StockChart.Common;
using iExchange.Common;
using System.Diagnostics;

namespace iExchange.TradingConsole.SChart
{
	public class DataManager:DataManagerBase
	{
		private InstrumentInfo instrumentInfo;
		private DataCycle dataCycle;

		private static Scheduler scheduler;

		//private static CookieContainer cookieContainer;

		private static InitializedData initializedData;
		private static object initializedDataLockFlag=new object();

		public static Scheduler Scheduler
		{
			get { return DataManager.scheduler; }
			set { DataManager.scheduler = value; }
		}
		/*
		public static CookieContainer CookieContainer
		{
			get{return DataManager.cookieContainer;}
			set{DataManager.cookieContainer=value;}
		}
		*/
		public DataManager(InstrumentInfo instrumentInfo,DataCycle dataCycle)
		{
			this.instrumentInfo=instrumentInfo;
			this.dataCycle=dataCycle;
		}

		private void GetData()
		{
		}

		public static void CallService(CookieContainer cookieContainer)
		{
			
		}
		
		public DataSet GetData(CookieContainer cookieContainer,Guid instrumentId,DateTime lastDate,int count)
		{
            return null;
		}

		public IDataProvider GetData(CookieContainer cookieContainer,string Code,int Count)
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
		/*
		public static void Initialize()
		{
			lock(initializedDataLockFlag)
			{
				if(initializedData==null)
				{
					SChart.Datas.StockData stockData=new SChart.Datas.StockData(DataManager.cookieContainer);
					DataManager.initializedData=stockData.GetInitializedData();

					DataManager.Scheduler = new Scheduler();
					Scheduler.Action action = new Scheduler.Action(DataManager.CallService2);
					DataManager.scheduler.Add(action, null, DateTime.Now,DateTime.MaxValue,TimeSpan.FromMinutes(1));
				}
			}
		}
		
		private void CallService2(object sender, object Args)
		{
			AppDebug.LogEvent("iExchange.TradingConsole.SChart.DataManager.CallService2 -- to call DataManager.CallService()", EventLogEntryType.Information);
			DataManager.CallService();
			AppDebug.LogEvent("iExchange.TradingConsole.SChart.DataManager.CallService2 -- to call DataManager.CallService() is OK!", EventLogEntryType.Information);
		}
		*/

		public static string[] getDataCycleNames
		{
			get{return DataManager.initializedData.DataCycleNames;}
		}

	}
}