using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Nops
{
    public class UniqueNop : INopRequirements
    {
        Type[] _Types;

        /// <summary>
        /// Type
        /// </summary>
        public Type[] Types { get { return _Types; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="types">Types</param>
        public UniqueNop(params Type[] types) { _Types = types; }

        public bool IsAllowed(ModuleHeader<Nop> nop)
        {
            Type t2 = nop.Type;
            foreach (Type t in _Types)
                if (t == t2) return true;

            return false;
        }
    }
}