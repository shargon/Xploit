using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Payloads
{
    public class InterfacePayload : IPayloadRequirements
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
        public InterfacePayload(Type type) { _Type = type; }

        public bool ItsRequired() { return true; }

        public bool IsAllowed(ModuleHeader<Payload> payload)
        {
            return _Type.IsAssignableFrom(payload.Type);
        }
    }
}