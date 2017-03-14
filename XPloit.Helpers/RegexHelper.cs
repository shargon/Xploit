using System.Text.RegularExpressions;

namespace XPloit.Helpers
{
    public class RegexHelper
    {
        /// <summary>
        /// Create a msdos regex style 'file.*'
        /// </summary>
        /// <param name="pattern">Pattern</param>
        public static Regex RegexAsMsdos(string pattern)
        {
            pattern = Regex.Escape(pattern);

            pattern = pattern.Replace("\\*", ".*");
            pattern = pattern.Replace("_", ".");

            return new Regex("^" + pattern + "$");
        }
    }
}