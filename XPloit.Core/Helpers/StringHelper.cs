using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace XPloit.Core.Helpers
{
    public class StringHelper
    {
        /// <summary>
        /// Split the first word, separated by whitespace, from the rest of the string and return it.
        /// </summary>
        /// <param name="from">
        /// String from which to select the first word. This parameter will be changed to exclude the split off word.
        /// </param>
        /// <returns>
        /// First whitespace-separated word in argument.
        /// </returns>
        public static string SplitFirstWord(ref string from)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }

            string[] split = from.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            from = split.Length > 1 ? split[1].TrimStart() : "";
            string word = split.Length > 0 ? split[0].Trim() : "";
            return word;
        }
        /// <summary>
        /// Split input in two string
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="sep">Separator</param>
        /// <param name="left">Left variable</param>
        /// <param name="right">Right variable</param>
        /// <returns>Returns false if sep dosen't appear</returns>
        public static bool Split(string input, char sep, out string left, out string right)
        {
            return Split(input, sep.ToString(), out left, out right);
        }
        /// <summary>
        /// Split input in two string
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="sep">Separator</param>
        /// <param name="left">Left variable</param>
        /// <param name="right">Right variable</param>
        /// <returns>Returns false if sep dosen't appear</returns>
        public static bool Split(string input, string sep, out string left, out string right)
        {
            int fi = string.IsNullOrEmpty(input) ? -1 : input.IndexOf(sep);
            if (fi == -1) { left = input; right = ""; return false; }

            left = input.Substring(0, fi);
            int sl = sep.Length;
            right = input.Substring(fi + sl, input.Length - fi - sl);
            return true;
        }
        /// <summary>
        /// Returns true if input is valid email
        /// </summary>
        /// <param name="input">Input</param>
        public static bool IsValidEmail(string input)
        {
            if (String.IsNullOrEmpty(input))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                validDomain v = new validDomain();
                input = Regex.Replace(input, @"(@)(.+)$", v.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
                if (!v.isValid) return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(input)) return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(input,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        class validDomain
        {
            public bool isValid = true;
            public string DomainMapper(Match match)
            {
                // IdnMapping class with default property values.
                IdnMapping idn = new IdnMapping();

                string domainName;
                try
                {
                    domainName = match.Groups[2].Value;
                    domainName = idn.GetAscii(domainName);
                    return match.Groups[1].Value + domainName;
                }
                catch
                {
                    isValid = false;
                    return null;
                }
            }
        }
        /// <summary>
        /// Replicate String
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="iterations">Iterations</param>
        public static string Replicate(string input, int iterations)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < iterations; x++) sb.Append(input);
            return sb.ToString();
        }
        /// <summary>
        /// Returns if the input match with the pattern (*.midomain.???)
        /// </summary>
        /// <param name="pattern">Patter</param>
        /// <param name="input">Input</param>
        public static bool Like(string pattern, string input)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            if (string.IsNullOrEmpty(input)) return false;

            return Operators.LikeString(input, pattern, Microsoft.VisualBasic.CompareMethod.Text);
        }
        /// <summary>
        /// Convert to Kb, Mb ...
        /// </summary>
        /// <param name="bytes">Bytes to convert</param>
        /// <returns></returns>
        public static string Convert2Kb(long bytes)
        {
            if (bytes < 1024) return bytes.ToString() + " b";
            if (bytes < 1024 * 1024) return (bytes / (1024.0)).ToString("0.00") + " Kb";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024.0 * 1024.0)).ToString("0.00") + " Mb";
            if (bytes < 1024 * 1024 * 1024 * 1024.0) return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.00") + " Gb";
            return (bytes / (1024.0 * 1024.0 * 1024.0 * 1024.0)).ToString("0.00") + " Tb";
        }
    }
}