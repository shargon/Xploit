using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
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

        /// <summary>
        /// Return Properties
        /// </summary>
        /// <param name="excludeOnlyRead">True for exclude onlyRead</param>
        /// <param name="excludeOnlyWrite">True for exclude onlyWrite</param>
        public IEnumerable<PropertyInfo> GetProperties(bool excludeOnlyRead, bool excludeOnlyWrite, bool excludeNonEditableProperties)
        {
            foreach (PropertyInfo pi in GetType().GetProperties())
            {
                if (excludeOnlyRead && pi.CanRead && !pi.CanWrite) continue;
                if (excludeOnlyWrite && pi.CanWrite && !pi.CanRead) continue;
                if (excludeNonEditableProperties)
                {
                    if (!pi.CanWrite) continue;
                    if (pi.PropertyType.IsClass)
                    {
                        if (
                            pi.PropertyType != typeof(string) &&
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

                yield return pi;
            }
        }
    }
}