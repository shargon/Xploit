using System.Text;

namespace XPloit.Core.Helpers
{
    public class PatternHelper
    {
        public const int MaxPatternUnique = 20280;

        /// <summary>
        /// Create a patter for exploit development
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="setA">Set A</param>
        /// <param name="setB">Set B</param>
        /// <param name="setC">Set C</param>
        public static string Create(int length)
        {
            if (length <= 0) return "";

            string setA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string setB = "abcdefghijklmnopqrstuvwxyz";
            string setC = "0123456789";

            int l = 0;
            StringBuilder sb = new StringBuilder();

            while (true)
                foreach (char a in setA)
                    foreach (char b in setB)
                        foreach (char c in setC)
                        {
                            sb.Append(a); l++;
                            if (l == length) return sb.ToString();

                            sb.Append(b); l++;
                            if (l == length) return sb.ToString();

                            sb.Append(c); l++;
                            if (l == length) return sb.ToString();
                        }
        }
        /// <summary>
        /// Search offset in pattern
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="txt">Text</param>
        /// <param name="start">Pattern index</param>
        public static int Search(int length, string txt, int start = 0)
        {
            string pat = Create(length);
            return pat.IndexOf(txt, start);
        }
    }
}