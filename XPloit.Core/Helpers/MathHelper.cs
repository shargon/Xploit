using System;
using System.Data;

namespace XPloit.Core.Helpers
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
            using (DataTable dt = new DataTable())
            {
                try { return Convert.ToDouble(dt.Compute(input, "")); }
                catch { }
            }
            return double.NaN;
        }
    }
}