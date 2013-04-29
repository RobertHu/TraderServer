using System;
using System.Drawing;
using System.ComponentModel;
using System.Configuration;
using System.Xml;
using System.Web;

using EasyTools;
using Easychart.Finance;
using AsyncSslServer.Setting;

namespace iExchange.TradingConsole.SChart
{
	public delegate bool UpdateService (DateTime d,string Exchange);
	/// <summary>
	/// Summary description for Config.
	/// </summary>
	public class Config
	{
		private Config()
		{
		}

		//#if(!LITE)
		//		static public UpdateService AutoUpdateSource
		//		{
		//			get
		//			{
		//				string s = Read("AutoUpdateSource");
		//				if (string.Compare(s,"DataService")==0)
		//					return new UpdateService(DataService.UpdateQuote);
		//				else return new UpdateService(InternetDataToDB.UpdateQuote);
		//			}
		//		}
		//#endif

		static public bool EnableAutoUpdate = ReadBool("EnableAutoUpdate",false);
		static public bool EnableYahooStreaming = ReadBool("EnableYahooStreaming",false);
		static public bool EnableChangeToAdmin = ReadBool("EnableChangeToAdmin",false);

		static public string YAxisFormat = Read("YAxisFormat");
		static public string SymbolCase = Read("SymbolCase");
		static public string ImageFormat = Read("ImageFormat","Gif");
		static public int GifColors = ReadInt("GifColors",255);
		static public Color TransparentColor = (Color)TypeDescriptor.GetConverter(typeof(Color)).ConvertFromString(Read("TransparentColor"));
		static public string CompanyName = Read("CompanyName","Easy Chart Inc");
		static public string URL = Read("URL","http://finance.easychart.net");

		static public bool ShowXAxisInLastArea = ReadBool("ShowXAxisInLastArea",false);
		static public bool ShowMainAreaLineX= ReadBool("ShowMainAreaLineX",true);
		static public bool ShowMainAreaLineY= ReadBool("ShowMainAreaLineY",true);

		static public string WebProxy = Read("WebProxy");
		static public bool PrescanLoadToMemory = ReadBool("Prescan.LoadToMemory",false);

		static public string LatestValueType = Read("LatestValueType","StockOnly");
		static public string HistoricalDataPath = Read("HistoricalDataPath","Data\\");
		static public int FixDataTestDays = ReadInt("FixData.TestDays",300);
		static public int FixDataDifference = ReadInt("FixData.Difference",25);
		static public int FixDataGapDays = ReadInt("FixData.GapDays",10);
		static public int FixDataNoDataDays = ReadInt("FixData.NoDataDays",7);
		
		static public StickRenderType StickRenderType = (StickRenderType)ReadEnum("StickRenderType",typeof(StickRenderType),StickRenderType.Default);

		static public string DefaultFormulas = Read("DefaultFormulas","MA(20)");
		
		static public string Path 
		{
			get 
			{
                string s = SettingManager.Default.PhysicPath; //HttpRuntime.AppDomainAppVirtualPath;
				if (!s.EndsWith("/"))
					s +="/";
				return s;
			}
		}

		static public string PluginsDirectory 
		{
			get
			{
				string s = Read("PluginsDir");
				if (s==null || s=="")
					return null;
				if (!s.EndsWith("\\"))
					s +="\\";
				return SettingManager.Default.PhysicPath;//HttpRuntime.AppDomainAppPath+s;
				
				//return HttpRuntime.AppDomainAppVirtualPath+s;

			}
		}

		public static string AdminUserName = Read("AdminUserName");
		public static string AdminPassword = Read("AdminPassword");

		public static string PreScan
		{
			get 
			{
				string s = Read("PreScanFormula");
				if (s==null) return "";
				return s;
			}
			set 
			{
				Write("PreScanFormula",value);
			}
		}

		public static string DefaultSkin = Read("DefaultSkin","RedWhite");

		public static int IncludeTodayQuote = ReadInt("IncludeTodayQuote",0);
 
		public static bool RealtimeVisible()
		{
			if (IncludeTodayQuote==3)
				return true;
			int H = DateTime.UtcNow.AddHours(-4).Hour;
			if (IncludeTodayQuote!=2 && IncludeTodayQuote!=4 || H<MarketOpen || H>MarketClose)
				return false;
			else return true;
		}

		

		static public string HistoricalDataYear = Read("HistoricalDataYear","1980");

		static public string AutoPullFormulaData
		{
			get 
			{
				return Read("AutoPullFormulaData");
			}
			set 
			{
				Write("AutoPullFormulaData",value);
			}
		}

		static public int MarketOpen = ReadInt("MarketOpen",9);
		static public int MarketClose=ReadInt("MarketClose",16);
		static public string AutoUpdate = Read("AutoUpdate");
		static public string AutoUpdateFormula = Read("AutoUpdateFormula");

		public static int ReadInt(string Key,int Def)
		{
			return Tools.ToIntDef(Read(Key),Def);
		}

		static private object ReadEnum(string Key,Type T,object Def)
		{
			try
			{
				return  Enum.Parse(T,Read(Key,Def.ToString()),false);
			}
			catch
			{
				return Def;
			}
		}

		public static bool ReadBool(string Key,bool Def)
		{
			string s = Read(Key);
			if (s=="1")
				return true;
			else if (s=="0")
				return false;
			else return Def;
		}

		public static string Read(string Key,string Def) 
		{
			string s = ConfigurationSettings.AppSettings[Key];
			if (s==null)
				return Def;
			return s;
		}

		public static string Read(string Key)
		{
			return Read(Key,"");
		}

		public static void Write(string Key,string Value)
		{
			XmlDocument xd = new XmlDocument();
            //HttpRuntime.AppDomainAppPath
			string s =SettingManager.Default.PhysicPath +@"\web.config";
			xd.Load(s);
			XmlNode xns = xd.SelectSingleNode("/configuration/appSettings");
			for(int i=0; i<xns.ChildNodes.Count; i++)
				if (xns.ChildNodes[i] is XmlElement)
				{
					XmlElement xe = xns.ChildNodes[i] as XmlElement;
					if (xe.Name.ToLower() == "add") 
					{
						if (string.Compare(xe.GetAttribute("key").ToString(),Key,true)==0)
							xe.SetAttribute("value",Value);
					}
				}
			Impersonate.ChangeToAdmin();
			xd.Save(s);
		}
	}
}
