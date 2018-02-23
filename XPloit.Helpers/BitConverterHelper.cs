using System;

namespace XPloit.Helpers
{
    public class BitConverterHelper
    {
        //si el pc no es LittleEndian, se jode la bicicleta y no se puede conectar un no LittleEndian con un Si, por eso se crean estas
        //funciones paralelas

        public static byte[] GetBytesInt16(short value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 2);
            return buffer;
        }
        public static byte[] GetBytesUInt16(ushort value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 2);
            return buffer;
        }
        public static byte[] GetBytesInt24(int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 4);
            Array.Resize(ref buffer, 3);
            return buffer;
        }
        public static byte[] GetBytesUInt24(uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 4);
            Array.Resize(ref buffer, 3);
            return buffer;
        }
        public static byte[] GetBytesInt32(int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 4);
            return buffer;
        }
        public static byte[] GetBytesInt64(long value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 8);
            return buffer;
        }
        public static byte[] GetBytesUInt64(ulong value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 8);
            return buffer;
        }
        public static byte[] GetBytesDouble(double value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 8);
            return buffer;
        }
        public static byte[] GetBytesSingle(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 4);
            return buffer;
        }
        public static byte[] GetBytesUInt32(uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer, 0, 4);
            return buffer;
        }

        public static short ToInt16(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToInt16(new byte[] { value[index + 1], value[index] }, 0);
            return BitConverter.ToInt16(value, index);
        }
        public static ushort ToUInt16(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt16(new byte[] { value[index + 1], value[index] }, 0);
            return BitConverter.ToUInt16(value, index);
        }
        public static int ToInt24(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToInt32(new byte[] { 0, value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToInt32(new byte[] { value[index], value[index + 1], value[index + 2], 0 }, index);
        }
        public static uint ToUInt24(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt32(new byte[] { 0, value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToUInt32(new byte[] { value[index], value[index + 1], value[index + 2], 0 }, index);
        }
        public static int ToInt32(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToInt32(new byte[] { value[index + 3], value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToInt32(value, index);
        }
        public static long ToInt64(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToInt64(new byte[] { value[index + 7], value[index + 6], value[index + 5], value[index + 4], value[index + 3], value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToInt64(value, index);
        }
        public static ulong ToUInt64(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt64(new byte[] { value[index + 7], value[index + 6], value[index + 5], value[index + 4], value[index + 3], value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToUInt64(value, index);
        }
        public static float ToSingle(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt64(new byte[] { value[index + 3], value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToSingle(value, index);
        }
        public static double ToDouble(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToDouble(new byte[] { value[index + 7], value[index + 6], value[index + 5], value[index + 4], value[index + 3], value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToDouble(value, index);
        }
        public static uint ToUInt32(byte[] value, int index)
        {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt32(new byte[] { value[index + 3], value[index + 2], value[index + 1], value[index] }, 0);
            return BitConverter.ToUInt32(value, index);
        }
    }
}