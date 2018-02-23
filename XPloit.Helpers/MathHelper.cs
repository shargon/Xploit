using System;
using System.Data;
using System.Text.RegularExpressions;

namespace XPloit.Helpers
{
    public class MathHelper
    {
        /// <summary>
        /// Calc from string
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Returns math calc or Nan if fails</returns>
        public static double Calc(string input)
        {
            // Replaces for byte calculation

            input = Regex.Replace(input, "tb", "*(1024*1024*1024*1024)", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "gb", "*(1024*1024*1024)", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "mb", "*(1024*1024)", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "kb", "*(1024)", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "b", "", RegexOptions.IgnoreCase);

            // Trick for doing the math calcs
            using (DataTable dt = new DataTable())
            {
                try { return Convert.ToDouble(dt.Compute(input, "")); }
                catch { }
            }
            return double.NaN;
        }
    }
}