﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;

namespace XPloit.Core.Helpers
{
    public class ConvertHelper
    {
        static Type _BoolType = typeof(bool);
        static Type _StringType = typeof(string);

        static Type _ByteType = typeof(byte), _SByteType = typeof(sbyte);
        static Type _Int64Type = typeof(long), _UInt64Type = typeof(ulong);
        static Type _Int32Type = typeof(int), _UInt32Type = typeof(uint);
        static Type _Int16Type = typeof(short), _UInt16Type = typeof(ushort);

        static Type _DoubleType = typeof(double);
        static Type _DecimalType = typeof(decimal);
        static Type _FloatType = typeof(float);
        static Type _TimeSpanType = typeof(TimeSpan);
        static Type _DateTimeType = typeof(DateTime);
        static Type _IPAddressType = typeof(IPAddress);
        static Type _IPEndPointType = typeof(IPEndPoint);
        static Type _FileInfoType = typeof(FileInfo);
        static Type _DirectoryInfoType = typeof(DirectoryInfo);
        static Type _ByteArrayType = typeof(byte[]);

        static Type _IListType = typeof(IList);

        public static object ConvertTo(string input, Type type)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GenericTypeArguments[0];
                }
                if (type.IsEnum)
                {
                    string[] f = Enum.GetNames(type);

                    long iret = 0;
                    object ret = null;
                    foreach (string en in input.Split(GetSplitChar(type)))
                    {
                        foreach (string v in f)
                        {
                            if (string.Compare(v, en, true) == 0)
                            {
                                object v1 = Enum.Parse(type, v);
                                if (v1 != null)
                                {
                                    if (ret == null)
                                    {
                                        ret = v1;
                                        iret = Convert.ToInt64(v1);
                                    }
                                    else
                                    {
                                        // TODO Convert multienum One|Two
                                        iret |= Convert.ToInt64(v1);
                                    }
                                }
                            }
                        }
                    }

                    return ret;
                }

                // String conversion
                if (type == _StringType) return input;
                if (input.Trim().ToLowerInvariant() == "null") return null;

                // Bool
                if (type == _BoolType)
                {
                    Boolean r;
                    if (!Boolean.TryParse(input.Trim(), out r))
                    {
                        string si = input.ToLowerInvariant();
                        return si == "y" || si == "s" || si == "yes" || si == "si" || si == "1" || si == "true";
                    }
                    return r;
                }
                // Date/Time
                if (type == _TimeSpanType)
                {
                    TimeSpan r;
                    if (!TimeSpan.TryParse(input.Trim(), CultureInfo.InvariantCulture, out r))
                    {
                        long ms = Convert.ToInt64(MathHelper.Calc(input));
                        return TimeSpan.FromMilliseconds(ms);
                    }
                    return r;
                }
                if (type == _DateTimeType)
                {
                    DateTime r;
                    if (!DateTime.TryParse(input.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out r)) return DateTime.MinValue;
                    return r;
                }

                // Numeric Conversion
                if (type == _ByteType)
                {
                    byte r;
                    if (!byte.TryParse(input.Trim(), out r)) return Convert.ToByte(MathHelper.Calc(input));
                    return r;
                }
                if (type == _SByteType)
                {
                    sbyte r;
                    if (!sbyte.TryParse(input.Trim(), out r)) return Convert.ToSByte(MathHelper.Calc(input));
                    return r;
                }
                if (type == _Int64Type)
                {
                    long r;
                    if (!long.TryParse(input.Trim(), out r)) return Convert.ToInt64(MathHelper.Calc(input));
                    return r;
                }
                if (type == _UInt64Type)
                {
                    ulong r;
                    if (!ulong.TryParse(input.Trim(), out r)) return Convert.ToUInt64(MathHelper.Calc(input));
                    return r;
                }
                if (type == _Int32Type)
                {
                    int r;
                    if (!int.TryParse(input.Trim(), out r)) return Convert.ToInt32(MathHelper.Calc(input));
                    return r;
                }
                if (type == _UInt32Type)
                {
                    uint r;
                    if (!uint.TryParse(input.Trim(), out r)) return Convert.ToUInt32(MathHelper.Calc(input));
                    return r;
                }

                if (type == _Int16Type)
                {
                    short r;
                    if (!short.TryParse(input.Trim(), out r)) return Convert.ToInt16(MathHelper.Calc(input));
                    return r;
                }
                if (type == _UInt16Type)
                {
                    ushort r;
                    if (!ushort.TryParse(input.Trim(), out r)) return Convert.ToUInt16(MathHelper.Calc(input));
                    return r;
                }

                // Decimal (With '.' as decimal separator)
                if (type == _DoubleType)
                {
                    double r;
                    if (!double.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return Convert.ToDouble(MathHelper.Calc(input));
                    return r;
                }
                if (type == _DecimalType)
                {
                    Decimal r;
                    if (!Decimal.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return Convert.ToDecimal(MathHelper.Calc(input));
                    return r;
                }
                if (type == _FloatType)
                {
                    float r;
                    if (!float.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return Convert.ToSingle(MathHelper.Calc(input));
                    return r;
                }
                if (type == _IPAddressType)
                {
                    ushort prto = 0;
                    IPAddress ip;
                    if (IPHelper.ParseIpPort(input, out ip, ref prto)) return ip;
                    return IPAddress.Any;
                }
                if (type == _FileInfoType) return new FileInfo(input);
                if (type == _DirectoryInfoType) return new DirectoryInfo(input);

                if (type == _IPEndPointType)
                {
                    IPAddress ip;
                    ushort port = 0;
                    if (!IPHelper.ParseIpPort(input, out ip, ref port)) return null;
                    if (port == 0) return null;
                    return new IPEndPoint(ip, port);
                }
                if (type == _ByteArrayType)
                {
                    return HexHelper.FromHexString(input);
                }

                // Array
                if (type.IsArray)
                {
                    List<object> l = new List<object>();
                    Type gt = type.GetElementType();
                    foreach (string ii in input.Split(GetSplitChar(gt)))
                    {
                        object ov = ConvertTo(ii, gt);
                        if (ov == null) continue;

                        l.Add(ov);
                    }

                    Array a = (Array)Activator.CreateInstance(type, l.Count);
                    Array.Copy(l.ToArray(), a, l.Count);
                    return a;
                }

                // List
                if (_IListType.IsAssignableFrom(type))
                {
                    IList l = (IList)Activator.CreateInstance(type);
                    // If dosent have T return null
                    if (type.GenericTypeArguments == null || type.GenericTypeArguments.Length == 0) return null;

                    Type gt = type.GenericTypeArguments[0];
                    foreach (string ii in input.Split(GetSplitChar(gt)))
                    {
                        object ov = ConvertTo(ii, gt);
                        if (ov == null) continue;

                        l.Add(ov);
                    }
                    return l;
                }

                // Objects
                if (type.IsClass)
                {
                    // Is Convertible (Like Password)
                    TypeConverter conv = TypeDescriptor.GetConverter(type);
                    if (conv.CanConvertFrom(_StringType))
                    {
                        return conv.ConvertFrom(input);
                    };

                    if (input.StartsWith("{") && input.EndsWith("}"))
                        input = input.Substring(1, input.Length - 2);

                    return ArgumentHelper.Parse(type, input);
                }
            }

            return null;
        }
<<<<<<< HEAD:XPloit.Core/Helpers/ConvertHelper.cs
=======

        /// <summary>
        /// Object to string
        /// </summary>
        /// <param name="value">Value</param>
        public static string ToString(object value)
        {
            if (value == null) return "NULL";

            if (value is IList)
            {
                IList l = (IList)value;
                Type t = l.GetType();
                if (t.IsArray && value is byte[])
                {
                    t = t.GetElementType();
                    if (!t.IsEnum) value = HexHelper.Buffer2Hex((byte[])l, ":");
                }
                return string.Join(",", (l).OfType<object>().Select(u => u.ToString()));
            }
            else if (value is Array)
            {
                Array l = (Array)value;
                Type t = l.GetType();
                if (t.IsArray && value is byte[])
                {
                    t = t.GetElementType();
                    if (!t.IsEnum) value = HexHelper.Buffer2Hex((byte[])l, ":");
                }
                return string.Join(",", (l).OfType<object>().Select(u => u.ToString()));
            }
            //else if (value is IEnumerable)
            //{
            //    IEnumerable l = (IEnumerable)value;
            //    return string.Join(",", (l).OfType<object>().Select(u => u.ToString()));
            //}

            return value.ToString();
        }

>>>>>>> parent of d3eaeb5... SharpPcap:Xploit.Helpers/ConvertHelper.cs
        static char[] GetSplitChar(Type gt)
        {
            if (gt == _StringType)
                return new char[] { '\t', '\n', ';' };

            return new char[] { '\t', '\n', ';', ',', '|' };
        }
    }
}