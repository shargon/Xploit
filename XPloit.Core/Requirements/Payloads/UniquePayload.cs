using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Payloads
{
    public class UniquePayload : IPayloadRequirements
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
        public UniquePayload(params Type[] types) { _Types = types; }

        public bool ItsRequired() { return true; }

        public bool IsAllowed(ModuleHeader<Payload> payload)
        {
            Type t2 = payload.Type;
            foreach (Type t in _Types)
                if (t == t2) return true;

            return false;
        }
    }
}