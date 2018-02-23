using System;

namespace XPloit.Core.Extensions
{
    public static class ArrayExtension
    {
        public static T[] Concat<T>(this T[] x, T y)
        {
            if (x == null || y == null) return x;

            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + 1);
            x[oldLen] = y;
            return x;
        }
        public static T[] Concat<T>(this T[] x, T[] y)
        {
            if (x == null) return y;
            if (y == null) return x;

            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
    }
}