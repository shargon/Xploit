using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.PayloadRequirements
{
    public class NoPayloadRequired : IPayloadRequirements
    {
        public bool IsAllowedPayload(Payload payload) { return false; }
    }
}