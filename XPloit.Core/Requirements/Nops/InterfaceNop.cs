using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Nops
{
    public class InterfaceNop : INopRequirements
    {
        Type _Type;

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get { return _Type; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        public InterfaceNop(Type type) { _Type = type; }

        public bool IsAllowed(Nop nop)
        {
            return _Type.IsAssignableFrom(nop.GetType());
        }
    }
}