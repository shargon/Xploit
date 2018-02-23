using System;
using System.Collections.Generic;
using System.Globalization;

namespace XPloit.Core.Extensions
{
    /// <summary>
    /// Extension methods for StringComparison.
    /// </summary>
    public static class StringComparisonExtensions
    {
        /// <summary>
        /// Loose string comparison. Returns the best match using increasingly inaccurate comparisons.
        /// Also makes sure there is a sole match at that level of accuracy.
        ///
        /// Spaces in the select string are ignored.
        ///
        /// The levels are:
        /// <list>
        /// <item>Perfect match (abcd in abcd)</item>
        /// <item>Prefix match (ab in abcd)</item>
        /// <item>Containing match (bc in abcd)</item>
        /// <item>Matching ordered sequence of characters (bd in abcd)</item>
        /// </list>
        /// </summary>
        public static string LooseSelect(this string select, IEnumerable<string> source, StringComparison sc)
        {
            select = select.Replace(" ", "");
            StringComparer ec = sc.GetCorrespondingComparer();
            List<string> matches = new List<string>();
            int bestQuality = 0;

            foreach (var s in source)
            {
                int quality = -1;

                if (s.Equals(select, sc)) { quality = 10; }
                else if (s.StartsWith(select, sc)) { quality = 8; }
                else if (s.Contains(select, sc)) { quality = 6; }
                else if (StringContainsSequence(s, select)) { quality = 3; }

                if (quality >= bestQuality)
                {
                    if (quality > bestQuality)
                    {
                        bestQuality = quality;
                        matches.Clear();
                    }
                    matches.Add(s);
                }
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            //if (matches.Count > 1)
            //{
            //    Console.WriteLine("Identifier not unique: " + select);
            //}
            //else
            //{
            //    Console.WriteLine("Could not find identifier: " + select);
            //}
            return null;
        }
        static bool StringContainsSequence(string str, string sequence)
        {
            int i = 0;
            foreach (var c in sequence)
            {
                i = str.IndexOf(c, i) + 1;
                if (i == 0) return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if a StringComparison value is case sensitive.
        /// </summary>
        public static bool IsCaseSensitive(this StringComparison sc)
        {
            return false
                || sc == StringComparison.CurrentCulture
                || sc == StringComparison.InvariantCulture
                || sc == StringComparison.Ordinal;
        }

        /// <summary>
        /// Returns a culture which is appropriate for usage with the specified StringComparison.
        /// </summary>
        public static CultureInfo RelatedCulture(this StringComparison sc)
        {
            if (false
                || sc == StringComparison.InvariantCulture
                || sc == StringComparison.InvariantCultureIgnoreCase
                || sc == StringComparison.Ordinal
                || sc == StringComparison.OrdinalIgnoreCase)
            {
                return CultureInfo.InvariantCulture;
            }
            return CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Returns a StringComparer with the same comparison as the given StringComparison.
        /// </summary>
        public static StringComparer GetCorrespondingComparer(this StringComparison sc)
        {
            switch (sc)
            {
                case StringComparison.CurrentCulture: return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase: return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture: return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase: return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.Ordinal: return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase: return StringComparer.OrdinalIgnoreCase;
                default: throw new InvalidOperationException("Unknown string comparison value.");
            }
        }
        /// <summary>
        /// Returns true if a string contains a substring, using the specified culture and comparison options.
        /// </summary>
        public static bool Contains(this string s, string value, CultureInfo culture, CompareOptions options = CompareOptions.None)
        {
            return 0 <= culture.CompareInfo.IndexOf(s, value, options);
        }
        /// <summary>
        /// Returns true if a string contains a substring, using the specified StringComparison.
        /// </summary>
        public static bool Contains(this string s, string value, StringComparison sc)
        {
            CompareOptions co;
            switch (sc)
            {
                case StringComparison.CurrentCulture: co = CompareOptions.None; break;
                case StringComparison.CurrentCultureIgnoreCase: co = CompareOptions.IgnoreCase; break;
                case StringComparison.InvariantCulture: co = CompareOptions.None; break;
                case StringComparison.InvariantCultureIgnoreCase: co = CompareOptions.IgnoreCase; break;
                case StringComparison.Ordinal: co = CompareOptions.Ordinal; break;
                case StringComparison.OrdinalIgnoreCase: co = CompareOptions.OrdinalIgnoreCase; break;
                default: throw new InvalidOperationException("Unknown string comparison value.");
            }

            return s.Contains(value, sc.RelatedCulture(), co);
        }
    }
}