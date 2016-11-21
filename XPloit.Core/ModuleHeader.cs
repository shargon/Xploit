using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    public class ModuleHeader<T> where T : IModule
    {
        T _Cache;
        public Type Type { get; set; }

        public string FullPath { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }

        /// <summary>
        /// DisclosureDate
        /// </summary>
        public DateTime DisclosureDate { get; private set; }

        public T Current
        {
            get
            {
                if (_Cache != null) return _Cache;
                _Cache = (T)Activator.CreateInstance(Type);
                return _Cache;
            }
        }

        internal ModuleHeader(T m)
        {
            Type = m.GetType();
            FullPath = m.FullPath;
            Description = m.Description;
            Author = m.Author;
            DisclosureDate = m.DisclosureDate;
        }
    }
}