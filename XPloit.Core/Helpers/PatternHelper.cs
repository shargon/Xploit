using System;
using System.Collections.Generic;
using System.Text;

namespace XPloit.Core.Helpers
{
    public class PatternHelper
    {
        public const int MaxPatternUnique = 20280;

        /// <summary>
        /// Generate a random byte pattern
        /// </summary>
        /// <param name="length">Size</param>
        /// <param name="badChars">Bad chars</param>
        public static byte[] CreateRandom(int length, params byte[] badChars)
        {
            byte[] ret = new byte[length];

            bool isgood = false;
            Random r = new Random();
            for (int x = ret.Length - 1; x >= 0; x--)
            {
                do
                {
                    isgood = true;
                    ret[x] = (byte)r.Next(0, 255);

                    foreach (byte b in badChars)
                        if (b == ret[x]) { isgood = false; break; }
                }
                while (!isgood);
            }
            return ret;
        }
        /// <summary>
        /// Return all bytes ordered por Test bad chars
        /// </summary>
        /// <param name="bad">Bad bytes for ommit</param>
        public static byte[] GetBadBytes(params byte[] bad)
        {
            List<byte> ret = new List<byte>();
            for (ushort x = 0; x <= 255; x++)
            {
                bool esta = false;

                foreach (byte b in bad)
                    if (b == x) { esta = true; break; }

                if (esta) continue;
                ret.Add((byte)x);
            }

            return ret.ToArray();
        }
        /// <summary>
        /// Create a patter for exploit development
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="c">Char</param>
        public static byte[] CreateRaw(int length, char c)
        {
            return CreateRaw(length, (byte)c);
        }
        /// <summary>
        /// Create a patter for exploit development
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="c">Char</param>
        public static byte[] CreateRaw(int length, byte c)
        {
            if (length <= 0) return new byte[] { };

            byte[] ret = new byte[length];
            for (int x = ret.Length - 1; x >= 0; x--) ret[x] = c;
            return ret;
        }
        /// <summary>
        /// Create a patter for exploit development
        /// </summary>
        /// <param name="length">Length</param>
        public static byte[] CreateRaw(int length)
        {
            if (length <= 0) return new byte[] { };

            char[] setA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] setB = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            char[] setC = "0123456789".ToCharArray();

            int l = 0;
            byte[] data = new byte[length];

            while (true)
                foreach (char a in setA)
                    foreach (char b in setB)
                        foreach (char c in setC)
                        {
                            data[l] = (byte)a;
                            l++;
                            if (l == length) return data;

                            data[l] = (byte)b;
                            l++;
                            if (l == length) return data;

                            data[l] = (byte)c;
                            l++;
                            if (l == length) return data;
                        }
        }
        /// <summary>
        /// Create a patter for exploit development
        /// </summary>
        /// <param name="length">Length</param>
        public static string Create(int length)
        {
            if (length <= 0) return "";

            char[] setA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] setB = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            char[] setC = "0123456789".ToCharArray();

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