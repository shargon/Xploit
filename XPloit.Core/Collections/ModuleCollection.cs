namespace XPloit.Core.Collections
{
    public class ModuleCollection : IModuleCollection<Module>
    {
        static ModuleCollection _Current = null;

        /// <summary>
        /// Current Exploits
        /// </summary>
        public static ModuleCollection Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new ModuleCollection();
                    _Current.Load();
                }
                return _Current;
            }
        }
    }
}