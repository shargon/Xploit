using System.Collections.Generic;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Collections
{
    public class NopCollection : IModuleCollection<Nop>
    {
        static NopCollection _Current = null;

        /// <summary>
        /// Current Payloads
        /// </summary>
        public static NopCollection Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new NopCollection();
                    _Current.Load();
                }
                return _Current;
            }
        }

        public IEnumerable<ModuleHeader<Nop>> GetAvailables(INopRequirements req)
        {
            foreach (ModuleHeader<Nop> p in Current)
            {
                if (!req.IsAllowed(p)) continue;
                yield return p;
            }
        }
    }
}