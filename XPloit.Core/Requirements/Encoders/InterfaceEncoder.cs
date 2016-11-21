using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Encoders
{
    public class InterfaceEncoder : IEncoderRequirements
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
        public InterfaceEncoder(Type type) { _Type = type; }

        public bool IsAllowed(ModuleHeader<Encoder> nop)
        {
            return _Type.IsAssignableFrom(nop.Type);
        }
    }
}