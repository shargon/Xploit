using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.PayloadRequirements
{
    public class UniquePayload : IPayloadRequirements
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
        public UniquePayload(Type type) { _Type = type; }

        public bool IsAllowedPayload(Payload payload) { return _Type == payload.GetType(); }
    }
}