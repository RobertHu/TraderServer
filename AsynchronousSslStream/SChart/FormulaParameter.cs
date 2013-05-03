using System;
using System.Reflection;
using System.ComponentModel;

namespace Trader.Server.SChart
{
	//	[Serializable]
	public class FormulaParameter
	{
		private string name;
		public string Name
		{
			get{return this.name;}
			//set{this.name=value;}
		}

		private string defauleValue;
		public string DefaultValue
		{
			get{return this.defauleValue;}
			set{this.defauleValue=value;}
		}

		private string minValue;
		public string MinValue
		{
			get{return this.minValue;}
			set{this.minValue=value;}
		}

		private string maxValue;
		public string MaxValue
		{
			get{return this.maxValue;}
			set{this.maxValue=value;}
		}

		public FormulaParameter(string name){ this.name=name;}

	}
}
