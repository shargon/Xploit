using System.Collections.Generic;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Collections
{
    public class PayloadCollection : IModuleCollection<Payload>
    {
        static PayloadCollection _Current = null;

        /// <summary>
        /// Current Payloads
        /// </summary>
        public static PayloadCollection Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new PayloadCollection();
                    _Current.Load();
                }
                return _Current;
            }
        }

        public IEnumerable<ModuleHeader<Payload>> GetAvailables(IPayloadRequirements req)
        {
            foreach (ModuleHeader<Payload> p in Current)
            {
                if (!req.IsAllowed(p)) continue;
                yield return p;
            }
        }
    }
}