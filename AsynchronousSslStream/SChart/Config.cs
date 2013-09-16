using System;
using System.Drawing;
using System.ComponentModel;
using System.Xml;
using System.Configuration;
using EasyTools;
using Easychart.Finance;

namespace Trader.Server.SChart
{
	public delegate bool UpdateService (DateTime d,string exchange);
	/// <summary>
	/// Summary description for Config.
	/// </summary>
	public class Config
	{
		private Config()
		{
		}
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
				if (string.IsNullOrEmpty(s))
					return null;
				if (!s.EndsWith("\\"))
					s +="\\";
				return SettingManager.Default.PhysicPath;//HttpRuntime.AppDomainAppPath+s;
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

		public static int ReadInt(string key,int def)
		{
			return Tools.ToIntDef(Read(key),def);
		}

		static private object ReadEnum(string key,Type T,object def)
		{
			try
			{
				return  Enum.Parse(T,Read(key,def.ToString()),false);
			}
			catch
			{
				return def;
			}
		}

		public static bool ReadBool(string key,bool def)
		{
		    string s = Read(key);
		    switch (s)
		    {
		        case "1":
		            return true;
		        case "0":
		            return false;
		        default:
		            return def;
		    }
		}

	    public static string Read(string key,string def) 
		{
            string s = ConfigurationManager.AppSettings[key];
			if (s==null)
				return def;
			return s;
		}

		public static string Read(string key)
		{
			return Read(key,"");
		}

		public static void Write(string key,string value)
		{
			XmlDocument xd = new XmlDocument();
			string s =SettingManager.Default.PhysicPath +@"\web.config";
			xd.Load(s);
			XmlNode xns = xd.SelectSingleNode("/configuration/appSettings");
			for(int i=0; i<xns.ChildNodes.Count; i++)
				if (xns.ChildNodes[i] is XmlElement)
				{
					XmlElement xe = xns.ChildNodes[i] as XmlElement;
					if (xe.Name.ToLower() == "add") 
					{
						if (System.String.Compare(xe.GetAttribute("key"), key, System.StringComparison.OrdinalIgnoreCase)==0)
							xe.SetAttribute("value",value);
					}
				}
			Impersonate.ChangeToAdmin();
			xd.Save(s);
		}
	}
}
