using System;
using System.Collections.Generic;
using System.Reflection;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Collections
{
    public class IModuleCollection<T> : IList<IModule>
    {
        Type _TypeT;
        EModuleType _Type;

        /// <summary>
        /// Type of collection
        /// </summary>
        public EModuleType Type { get { return _Type; } }

        List<IModule> _InternalList = new List<IModule>();

        protected IModuleCollection()
        {
            _TypeT = typeof(T);

            if (_TypeT == typeof(Encoder)) _Type = EModuleType.Encoder;
            else if (_TypeT == typeof(Payload)) _Type = EModuleType.Payload;
            else if (_TypeT == typeof(Exploit)) _Type = EModuleType.Exploit;
        }

        public int IndexOf(IModule item) { return _InternalList.IndexOf(item); }
        public void Insert(int index, IModule item) { _InternalList.Insert(index, item); }
        public void RemoveAt(int index) { _InternalList.RemoveAt(index); }
        public IModule this[int index]
        {
            get { return _InternalList[index]; }
            set { _InternalList[index] = value; }
        }
        public void Add(IModule item) { _InternalList.Add(item); }
        public void Clear() { _InternalList.Clear(); }
        public bool Contains(IModule item) { return _InternalList.Contains(item); }
        public void CopyTo(IModule[] array, int arrayIndex) { _InternalList.CopyTo(array, arrayIndex); }
        public int Count { get { return _InternalList.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(IModule item) { return _InternalList.Remove(item); }
        public IEnumerator<IModule> GetEnumerator()
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
                    if (type.IsAssignableFrom(_TypeT))
                    {
                        IModule o = (IModule)Activator.CreateInstance(type);
                        Add(o);
                    }
                }
            }

            return Count - hay;
        }
    }
}