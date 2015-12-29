using System;
using System.Collections.Generic;
using System.Reflection;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Collections
{
    public class IModuleCollection<T> : IList<T> where T : IModule
    {
        Type _TypeT;
        EModuleType _Type;

        /// <summary>
        /// Type of collection
        /// </summary>
        public EModuleType Type { get { return _Type; } }

        List<T> _InternalList = new List<T>();

        protected IModuleCollection()
        {
            _TypeT = typeof(T);

            if (_TypeT == typeof(Encoder)) _Type = EModuleType.Encoder;
            else if (_TypeT == typeof(Payload)) _Type = EModuleType.Payload;
            else if (_TypeT == typeof(Exploit)) _Type = EModuleType.Exploit;
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
                foreach (Type type in asm.GetTypes())
                {
                    if (_TypeT == type) continue;
                    if (_TypeT.IsAssignableFrom(type))
                    {
                        T o = (T)Activator.CreateInstance(type);
                        Add(o);
                    }
                }
            }

            return Count - hay;
        }

        /// <summary>
        /// Get module by fullPath
        /// </summary>
        /// <param name="fullPath">FullPath</param>
        public T GetByFullPath(string fullPath)
        {
            foreach (T m in _InternalList)
            {
                if (string.Compare(m.FullPath, fullPath, StringComparison.InvariantCultureIgnoreCase) == 0) return m;
            }
            return default(T);
        }
    }
}