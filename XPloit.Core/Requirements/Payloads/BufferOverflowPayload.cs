using XPloit.Core.Interfaces;

namespace XPloit.Core.Requirements.Payloads
{
    public class BufferOverflowPayload : IPayloadRequirements
    {
        public int Space { get; set; }
        public char[] BadChars { get; set; }
        public int StackAdjustment { get; set; }


        public bool IsAllowed(Payload payload)
        {
            return false;
        }

        public bool ItsRequired() { return true; }
    }
}