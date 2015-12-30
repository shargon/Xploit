using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace XPloit.Core.Interfaces
{
    public class IModule
    {
        /// <summary>
        /// Author
        /// </summary>
        public virtual string Author { get { return null; } }
        /// <summary>
        /// Description
        /// </summary>
        public virtual string Description { get { return null; } }
        /// <summary>
        /// Name
        /// </summary>
        public virtual string Name { get { return null; } }
        /// <summary>
        /// Path
        /// </summary>
        public virtual string Path { get { return null; } }
        /// <summary>
        /// Return full path
        /// </summary>
        public string FullPath
        {
            get
            {
                string p = Path;
                string n = Name;

                if (!string.IsNullOrEmpty(p)) p = p.Trim('/') + "/"; else p = "";
                if (string.IsNullOrEmpty(n)) n = "";

                return p + n;
            }
        }
        ///// <summary>
        /// Type
        /// </summary>
        internal virtual EModuleType ModuleType { get { return EModuleType.Exploit; } }

        public override string ToString() { return FullPath; }

        int Sort(PropertyInfo a, PropertyInfo b) { return a.Name.CompareTo(b.Name); }
        /// <summary>
        /// Return Properties
        /// </summary>
        /// <param name="properties">Properties</param>
        public PropertyInfo[] GetProperties(params string[] properties)
        {
            List<PropertyInfo> ls = new List<PropertyInfo>();
            foreach (PropertyInfo pi in GetType().GetProperties())
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
        /// <param name="requiereRead">True for require read</param>
        /// <param name="requireWrite">True for require write</param>
        public PropertyInfo[] GetProperties(bool requiereRead, bool requireWrite, bool excludeNonEditableProperties)
        {
            List<PropertyInfo> ls = new List<PropertyInfo>();
            foreach (PropertyInfo pi in GetType().GetProperties())
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
    }
}