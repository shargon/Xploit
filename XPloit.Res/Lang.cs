using System.Globalization;
using System.Resources;

namespace XPloit.Res
{
    public static class Lang
    {
        static CultureInfo Current = CultureInfo.CurrentCulture;
        static ResourceSet rs;
        static ResourceManager rm = LoadLanguage(Current, ref rs);

        /// <summary>
        /// Load Language
        /// </summary>
        /// <param name="culture">CultureInfo</param>
        /// <returns></returns>
        public static void LoadLanguage(CultureInfo culture) { rm = LoadLanguage(culture, ref rs); }
        static ResourceManager LoadLanguage(CultureInfo culture, ref ResourceSet rs)
        {
            Current = culture;
            if (rs != null) { rs.Dispose(); rs = null; }

            try
            {
                ResourceManager rManager = new ResourceManager("XPloit.Res.Res", typeof(Lang).Assembly) { IgnoreCase = true };
                rs = rManager.GetResourceSet(culture, true, true);
                return rManager;
            }
            catch { }

            return null;
        }
        /// <summary>
        /// Get String from language
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pars">Params (%0 ,%1 ... )</param>
        public static string Get(string name, params string[] pars)
        {
            string ret = rs == null ? null : rs.GetString(name, true);
            if (ret == null) return "?";

            //string ret = rm.GetString(name, Current);

            for (int x = 0, m = pars.Length; x < m; x++)
                ret = ret.Replace("%" + x.ToString(), pars[x]);

            return ret;
        }
    }
}