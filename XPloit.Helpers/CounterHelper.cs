using System.Threading;

namespace XPloit.Helpers
{
    public class CounterHelper
    {
        static int _NextByte = 0;
        static int _NextUShort = 0;
        static long _NextUInt = 0;
        static long _NextULong = 0;

        /// <summary>
        /// Increment and get counter
        /// </summary>
        public static long GetNextInt64()
        {
            long ret = Interlocked.Increment(ref _NextULong);
            if (ret == long.MaxValue)
            {
                _NextULong = 0;
            }
            return ret;
        }
        /// <summary>
        /// Increment and get counter
        /// </summary>
        public static uint GetNextUInt32()
        {
            long ret = Interlocked.Increment(ref _NextUInt);
            if (ret == uint.MaxValue)
            {
                _NextUInt = 0;
            }
            return (uint)ret;
        }
        /// <summary>
        /// Increment and get counter
        /// </summary>
        public static ushort GetNextUInt16()
        {
            int ret = Interlocked.Increment(ref _NextUShort);
            if (ret == ushort.MaxValue)
            {
                _NextUShort = 0;
            }
            return (ushort)ret;
        }
        /// <summary>
        /// Increment and get counter
        /// </summary>
        public static byte GetNextByte()
        {
            int ret = Interlocked.Increment(ref _NextByte);
            if (ret == byte.MaxValue)
            {
                _NextByte = 0;
            }
            return (byte)ret;
        }
    }
}