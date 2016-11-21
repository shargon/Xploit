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

        public Nop[] GetAvailables(INopRequirements req)
        {
            if (req == null) return new Nop[] { };
            List<Nop> ls = new List<Nop>();
            foreach (ModuleHeader<Nop> p in Current)
            {
                if (!req.IsAllowed(p)) continue;

                ls.Add(p.Current);
            }
            return ls.ToArray();
        }
    }
}