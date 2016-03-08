using System;
using System.Collections.Generic;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Collections
{
    public class IModuleCollection<T> : IList<T> where T : IModule
    {
        Type _TypeT;
        EModuleType _Type;
        List<string> _LoadedAsm;

        /// <summary>
        /// Type of collection
        /// </summary>
        public EModuleType Type { get { return _Type; } }

        List<T> _InternalList = new List<T>();

        protected IModuleCollection()
        {
            _TypeT = typeof(T);
            _LoadedAsm = new List<string>();

            if (_TypeT == typeof(Encoder)) _Type = EModuleType.Encoder;
            else if (_TypeT == typeof(Payload)) _Type = EModuleType.Payload;
            else if (_TypeT == typeof(Module)) _Type = EModuleType.Module;
        }

        public int IndexOf(T item) { return _InternalList.IndexOf(item); }
        public void Insert(int index, T item) { _InternalList.Insert(index, item); }
        public void RemoveAt(int index) { _InternalList.RemoveAt(index); }
        public T this[int index]
        {
            get { return _InternalList[index]; }
            set { _InternalList[index] = value; }
        }
        public void Add(T item) { _InternalList.Add(item); }
        public void Clear() { _InternalList.Clear(); }
        public bool Contains(T item) { return _InternalList.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { _InternalList.CopyTo(array, arrayIndex); }
        public int Count { get { return _InternalList.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(T item) { return _InternalList.Remove(item); }
        public IEnumerator<T> GetEnumerator()
        {
            return _InternalList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _InternalList.GetEnumerator();
        }

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
                        OnlyFor onlyFor = type.GetCustomAttribute<OnlyFor>();
                        if (onlyFor != null)
                        {
                            if (SystemHelper.IsLinux && !onlyFor.Linux) continue;
                            if (SystemHelper.IsWindows && !onlyFor.Windows) continue;
                            if (SystemHelper.IsMac && !onlyFor.Mac) continue;
                        }

                        T o = (T)Activator.CreateInstance(type);
                        Add(o);
                    }
                }
            }

            return Count - hay;
        }
        /// <summary>
        /// Search
        /// </summary>
        /// <param name="pars">Params</param>
        public IEnumerable<T> Search(string[] pars)
        {
            foreach (T m in _InternalList)
            {
                if (m.AreInThisSearch(pars))
                {
                    yield return m;
                }
            }
        }
        /// <summary>
        /// Get module by fullPath
        /// </summary>
        /// <param name="fullPath">FullPath</param>
        /// <param name="clone">Clone</param>
        public T GetByFullPath(string fullPath, bool clone)
        {
            foreach (T m in _InternalList)
            {
                if (string.Compare(m.FullPath, fullPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    if (clone) return (T)ReflectionHelper.Clone(m, true);
                    return m;
                }
            }
            return default(T);
        }
    }
}