using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace XPloit.Core.Helpers
{
    public class ConvertHelper
    {
        static Type _StringType = typeof(string);

        static Type _Int64Type = typeof(long), _UInt64Type = typeof(ulong);
        static Type _Int32Type = typeof(int), _UInt32Type = typeof(uint);
        static Type _Int16Type = typeof(short), _UInt16Type = typeof(ushort);

        static Type _DoubleType = typeof(double);
        static Type _DecimalType = typeof(decimal);
        static Type _FloatType = typeof(float);
        static Type _TimeSpanType = typeof(TimeSpan);
        static Type _DateTimeType = typeof(DateTime);
        static Type _IPAddressType = typeof(IPAddress);

        static Type _IListType = typeof(IList);

        public static object ConvertTo(string input, Type type)
        {
            if (!string.IsNullOrEmpty(input))
            {
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

                // Date/Time
                if (type == _TimeSpanType)
                {
                    TimeSpan r;
                    if (!TimeSpan.TryParse(input.Trim(), CultureInfo.InvariantCulture, out r)) return TimeSpan.Zero;
                    return r;
                }
                if (type == _DateTimeType)
                {
                    DateTime r;
                    if (!DateTime.TryParse(input.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out r)) return DateTime.MinValue;
                    return r;
                }

                // Numeric Conversion
                if (type == _Int64Type)
                {
                    long r;
                    if (!long.TryParse(input.Trim(), out r)) return (long)0;
                    return r;
                }
                if (type == _UInt64Type)
                {
                    ulong r;
                    if (!ulong.TryParse(input.Trim(), out r)) return (ulong)0;
                    return r;
                }
                if (type == _Int32Type)
                {
                    int r;
                    if (!int.TryParse(input.Trim(), out r)) return (int)0;
                    return r;
                }
                if (type == _UInt32Type)
                {
                    uint r;
                    if (!uint.TryParse(input.Trim(), out r)) return (uint)0;
                    return r;
                }

                if (type == _Int16Type)
                {
                    short r;
                    if (!short.TryParse(input.Trim(), out r)) return (short)0;
                    return r;
                }
                if (type == _UInt16Type)
                {
                    ushort r;
                    if (!ushort.TryParse(input.Trim(), out r)) return (ushort)0;
                    return r;
                }

                // Decimal (With '.' as decimal separator)
                if (type == _DoubleType)
                {
                    double r;
                    if (!double.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return (double)0;
                    return r;
                }
                if (type == _DecimalType)
                {
                    Decimal r;
                    if (!Decimal.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return Decimal.Zero;
                    return r;
                }
                if (type == _FloatType)
                {
                    float r;
                    if (!float.TryParse(input.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return (float)0;
                    return r;
                }
                if (type == _IPAddressType)
                {
                    IPAddress r;
                    if (!IPAddress.TryParse(input.Trim(), out r)) return IPAddress.Any;
                    return r;
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
        static char[] GetSplitChar(Type gt)
        {
            if (gt == _StringType)
                return new char[] { '\t', '\n', ';' };

            return new char[] { '\t', '\n', ';', ',', '|' };
        }
    }
}