using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
namespace AsyncSslServer.TypeExtension
{
    public static class LuaTableExtension
    {
        public static Dictionary<string, string> ToDict(this LuaTable table)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var key in table.Keys)
            {
                dict.Add((string)key, (string)table[key]);
            }
            return dict;
        }
    }
}
