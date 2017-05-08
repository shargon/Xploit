using System;
using System.Collections.Generic;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Helpers;

namespace XPloit.Core.Collections
{
    public class IModuleCollection<T> : IEnumerable<ModuleHeader<T>>
        where T : IModule
    {
        Type _TypeT;
        EModuleType _Type;
        List<string> _LoadedAsm;

        /// <summary>
        /// Type of collection
        /// </summary>
        public EModuleType Type { get { return _Type; } }
        /// <summary>
        /// Count
        /// </summary>
        public int Count { get { return _InternalList.Count; } }

        Dictionary<string, ModuleHeader<T>> _InternalList = new Dictionary<string, ModuleHeader<T>>();

        protected IModuleCollection()
        {
            _TypeT = typeof(T);
            _LoadedAsm = new List<string>();

            if (_TypeT == typeof(Encoder)) _Type = EModuleType.Encoder;
            else if (_TypeT == typeof(Payload)) _Type = EModuleType.Payload;
            else if (_TypeT == typeof(Module)) _Type = EModuleType.Module;
        }

        public void Clear() { _InternalList.Clear(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return _InternalList.Values.GetEnumerator(); }
        IEnumerator<ModuleHeader<T>> IEnumerable<ModuleHeader<T>>.GetEnumerator() { return _InternalList.Values.GetEnumerator(); }

        /// <summary>
        /// Load IModule collection
        /// </summary>
        public int Load()
        {
            int hay = Count;

            // Load All loaded modules
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (_LoadedAsm.Contains(asm.FullName)) continue;
                _LoadedAsm.Add(asm.FullName);

                foreach (Type type in asm.GetTypes())
                {
                    if (_TypeT == type) continue;
                    if (_TypeT.IsAssignableFrom(type))
                    {
                        if (!ReflectionHelper.HavePublicConstructor(type))
                            continue;

                        AllowedPlatformsAttribute allowed = type.GetCustomAttribute<AllowedPlatformsAttribute>();
                        if (allowed != null)
                        {
                            if (SystemHelper.IsLinux && !allowed.Linux) continue;
                            if (SystemHelper.IsWindows && !allowed.Windows) continue;
                            if (SystemHelper.IsMac && !allowed.Mac) continue;
                        }

                        // Temporary creation for extract header
                        ModuleHeader<T> d = new Core.ModuleHeader<T>(type);
                        _InternalList.Add(d.FullPath.ToLowerInvariant(), d);
                    }
                }
            }

            return Count - hay;
        }
        /// <summary>
        /// Search
        /// </summary>
        /// <param name="pars">Params</param>
        public IEnumerable<ModuleHeader<T>> Search(string[] pars)
        {
            foreach (ModuleHeader<T> m in _InternalList.Values)
                if (AreInThisSearch(m, pars))
                    yield return m;
        }
        /// <summary>
        /// Return true if are in this query
        /// </summary>
        /// <param name="query">Query</param>
        bool AreInThisSearch(ModuleHeader<T> m, string[] query)
        {
            if (query == null || query.Length == 0) return true;

            string search = m.FullPath + " " + m.Description + " " + m.Author;
            foreach (string a in query)
                if (search.IndexOf(a, StringComparison.InvariantCultureIgnoreCase) < 0) return false;

            return true;
        }
        /// <summary>
        /// Get module by fullPath
        /// </summary>
        /// <param name="fullPath">FullPath</param>
        /// <param name="clone">Clone</param>
        public T GetByFullPath(string fullPath, bool clone)
        {
            ModuleHeader<T> ret;
            if (_InternalList.TryGetValue(fullPath.ToLowerInvariant(), out ret))
            {
                if (clone) return (T)ReflectionHelper.Clone(ret.Current, true);
                return ret.Current;
            }

            return default(T);
        }
    }
}