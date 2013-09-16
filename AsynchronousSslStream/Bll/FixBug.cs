namespace Trader.Server.Bll
{
    static class FixBug
    {
        internal static string Fix(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            value = value.Replace("&lt;", "<");
            value = value.Replace("&gt;", ">");
            value = value.Replace("&amp;", "&");
            value = value.Replace("&apos;", "'");
            value = value.Replace("&quot;", "\"");
            return value;
        }
    }
}