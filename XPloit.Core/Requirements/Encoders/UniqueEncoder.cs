using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Encoders
{
    public class UniqueEncoder : IEncoderRequirements
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
        public UniqueEncoder(params Type[] types) { _Types = types; }

        public bool IsAllowed(ModuleHeader<Encoder> nop)
        {
            Type t2 = nop.Type;

            foreach (Type t in _Types)
                if (t == t2) return true;

            return false;
        }
    }
}