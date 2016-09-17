using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Helpers
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Get the selected static delegate
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type</typeparam>
        /// <param name="type">Type</param>
        /// <param name="method">Method</param>
        public static Delegate GetStaticDelegate<TDelegate>(Type type, string method)
        {
            if (type == null) return null;

            Type[] types = GetTypesOfDelegate<TDelegate>();
            MethodInfo func = type.GetMethod(method, types);
            if (func != null) return Delegate.CreateDelegate(typeof(TDelegate), func);
            return null;
        }
        /// <summary>
        /// Get the selected delegate from object
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type</typeparam>
        /// <param name="instance">Instance</param>
        /// <param name="method">Method</param>
        public static Delegate GetDelegate<TDelegate>(object instance, string method)
        {
            if (instance == null)
                return null;

            Type type = instance.GetType();

            Type[] types = GetTypesOfDelegate<TDelegate>();
            MethodInfo func = type.GetMethod(method, types);
            if (func != null) return Delegate.CreateDelegate(typeof(TDelegate), instance, func);

            return null;
        }
        /// <summary>
        /// Returns the types from delegate
        /// </summary>
        static Type[] GetTypesOfDelegate<TDelegate>()
        {
            Type type = typeof(TDelegate);
            MethodInfo mInfos = type.GetMethod("Invoke");

            ParameterInfo[] pi = mInfos.GetParameters();
            Type[] tp = new Type[pi.Length];
            for (int x = 0, m = tp.Length; x < m; x++)
                tp[x] = pi[x].ParameterType;
            return tp;
        }
        /// <summary>
        /// Return the property value of one object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property</param>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            Type type = obj.GetType();
            if (type == null) return null;

            PropertyInfo func = type.GetProperty(propertyName);
            if (func != null) return func.GetValue(obj, null);

            return null;
        }
        /// <summary>
        /// Copy properties to other object
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        /// <param name="properties">Properties</param>
        public static int CopyProperties(object from, object to, string[] properties)
        {
            PropertyInfo[] pi = GetProperties(from, properties);
            if (pi == null) return -1;

            int change = 0;
            foreach (PropertyInfo p in pi)
            {
                if (!p.CanRead) continue;
                if (!p.CanWrite) continue;

                p.SetValue(to, p.GetValue(from));
                change++;
            }

            return change;
        }

        /// <summary>
        /// Dispose object if its viable
        /// </summary>
        /// <param name="obj">Object</param>
        public static bool FreeObject(object obj)
        {
            if (obj == null) return false;
            if (obj is IDisposable)
            {
                try
                {
                    ((IDisposable)obj).Dispose();
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Set property Value
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property</param>
        /// <param name="value">Value</param>
        /// <returns>Return true if its ok</returns>
        public static bool SetProperty(object obj, string propertyName, object value)
        {
            try
            {
                PropertyInfo[] pi = GetProperties(obj, propertyName);
                if (pi != null && pi.Length == 1)
                {
                    if (value != null && value is string)
                    {
                        // PreSet
                        foreach (IPreSetVariable a in pi[0].GetCustomAttributes<IPreSetVariable>(true))
                            value = a.PreSetVariable(value.ToString());

                        // Convert to String
                        object val = ConvertHelper.ConvertTo(value.ToString(), pi[0].PropertyType);

                        //if (val == null && value != null) return false;
                        pi[0].SetValue(obj, val);
                        return true;
                    }
                    else
                    {
                        pi[0].SetValue(obj, value);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        static int Sort(PropertyInfo a, PropertyInfo b) { return a.Name.CompareTo(b.Name); }
        /// <summary>
        /// Return Properties
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="properties">Properties</param>
        public static PropertyInfo[] GetProperties(object obj, params string[] properties)
        {
            if (obj == null) return new PropertyInfo[] { };
            if (properties == null)
                return obj.GetType().GetProperties();

            switch (properties.Length)
            {
                case 0: return obj.GetType().GetProperties();
                case 1:
                    {
                        PropertyInfo pi = obj.GetType().GetProperty(properties[0]);
                        if (pi != null) return new PropertyInfo[] { pi };
                        // non-case sensitive search
                        goto default;
                    }
                default:
                    {
                        List<PropertyInfo> ls = new List<PropertyInfo>();
                        foreach (PropertyInfo pi in obj.GetType().GetProperties())
                        {
                            bool esta = false;
                            foreach (string s in properties)
                                if (string.Equals(pi.Name, s, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    esta = true;
                                    break;
                                }
                            if (!esta) continue;

                            ls.Add(pi);
                        }
                        ls.Sort(Sort);
                        return ls.ToArray();
                    }
            }
        }

        /// <summary>
        /// Return Properties
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="requiereRead">True for require read</param>
        /// <param name="requireWrite">True for require write</param>
        public static PropertyInfo[] GetProperties(object obj, bool requiereRead, bool requireWrite, bool excludeNonEditableProperties)
        {
            if (obj == null) return new PropertyInfo[] { };

            List<PropertyInfo> ls = new List<PropertyInfo>();
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
            {
                if (requiereRead && !pi.CanRead) continue;
                if (requireWrite && !pi.CanWrite) continue;

                if (excludeNonEditableProperties)
                {
                    ConfigurableProperty cfg = pi.GetCustomAttribute<ConfigurableProperty>();
                    if (cfg == null) continue;

                    if (pi.PropertyType.IsClass)
                    {
                        if (pi.PropertyType != typeof(string) &&
                            pi.PropertyType != typeof(Boolean) &&

                            pi.PropertyType != typeof(SByte) &&
                            pi.PropertyType != typeof(UInt16) &&
                            pi.PropertyType != typeof(UInt32) &&
                            pi.PropertyType != typeof(UInt64) &&

                            pi.PropertyType != typeof(Byte) &&
                            pi.PropertyType != typeof(Int16) &&
                            pi.PropertyType != typeof(Int32) &&
                            pi.PropertyType != typeof(Int64) &&

                            pi.PropertyType != typeof(Decimal) &&
                            pi.PropertyType != typeof(float) &&
                            pi.PropertyType != typeof(Double) &&

                            pi.PropertyType != typeof(IPAddress) &&
                            pi.PropertyType != typeof(IPEndPoint) &&
                            pi.PropertyType != typeof(TimeSpan) &&
                            pi.PropertyType != typeof(DateTime) &&
                            pi.PropertyType != typeof(DirectoryInfo) &&
                            pi.PropertyType != typeof(FileInfo) &&

                            pi.PropertyType != typeof(List<byte>) &&
                            pi.PropertyType != typeof(List<sbyte>) &&
                            pi.PropertyType != typeof(List<short>) &&
                            pi.PropertyType != typeof(List<ushort>) &&
                            pi.PropertyType != typeof(List<int>) &&
                            pi.PropertyType != typeof(List<uint>) &&
                            pi.PropertyType != typeof(List<long>) &&
                            pi.PropertyType != typeof(List<ulong>) &&

                            pi.PropertyType != typeof(byte[]) &&
                            pi.PropertyType != typeof(sbyte[]) &&
                            pi.PropertyType != typeof(short[]) &&
                            pi.PropertyType != typeof(ushort[]) &&
                            pi.PropertyType != typeof(int[]) &&
                            pi.PropertyType != typeof(uint[]) &&
                            pi.PropertyType != typeof(long[]) &&
                            pi.PropertyType != typeof(ulong[])
                            )
                            continue;
                    }
                }
                ls.Add(pi);
            }
            ls.Sort(Sort);
            return ls.ToArray();
        }
        /// <summary>
        /// Create a new instance of this type
        /// </summary>
        /// <param name="type">Type</param>
        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
        /// <summary>
        /// Clone the current object
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="copyProperties">Copy properties?</param>
        public static object Clone(object parent, bool copyProperties)
        {
            if (parent == null) return null;

            Type type = parent.GetType();
            object obj = CreateInstance(type);

            if (copyProperties)
            {
                foreach (PropertyInfo pi in type.GetProperties())
                {
                    if (!pi.CanRead || !pi.CanWrite) continue;

                    pi.SetValue(obj, pi.GetValue(parent));
                }
            }

            return obj;
        }
    }
}
