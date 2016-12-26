using System;
using XPloit.Core.Attributes;
using XPloit.Core.Interfaces;
using System.Reflection;
using System.Diagnostics;

namespace XPloit.Core
{
    public class ModuleHeader<T> where T : IModule
    {
        T _Cache;
        string _Name, _ModulePath, _FullPath;

        public Type Type { get; set; }
        public string ModulePath { get { return _ModulePath; } }
        public string Name { get { return _Name; } }
        public string FullPath { get { return _FullPath; } }
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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="m">Module</param>
        public ModuleHeader(Type type)
        {
            Type = type;

            IModule.GetNames(type, out _ModulePath, out _Name, out _FullPath);
            ModuleInfoAttribute at = Type.GetCustomAttribute<ModuleInfoAttribute>();

            if (at != null)
            {
                Description = at.Description;
                Author = at.Author;
                DisclosureDate = at.DisclosureDate;

                if (string.IsNullOrEmpty(Author))
                    Author = GetAuthorFromLibrary(Type);
            }
        }
        /// <summary>
        /// Get author from library
        /// </summary>
        /// <param name="t">Type</param>
        public static string GetAuthorFromLibrary(Type t)
        {
            try
            {
                // Extract author from company of assembly
                // Todo: Cache this
                System.Diagnostics.FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(t).Location);
                return versionInfo.CompanyName;
            }
            catch { }
            return null;
        }
    }
}