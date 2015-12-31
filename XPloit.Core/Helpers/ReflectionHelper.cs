using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using XPloit.Core.Attributes;

namespace XPloit.Core.Helpers
{
    public class ReflectionHelper
    {
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
                        // Convert to String
                        pi[0].SetValue(obj, ConvertHelper.ConvertTo(value.ToString(), pi[0].PropertyType));
                        return true;
                    }
                    else
                    {
                        pi[0].SetValue(obj, value);
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
                            pi.PropertyType != typeof(DateTime)
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