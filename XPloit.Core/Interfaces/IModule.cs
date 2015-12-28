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
    }
}