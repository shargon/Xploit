using XPloit.Core.Interfaces;

namespace XPloit.Core.PayloadRequirements
{
    public class BufferOverflowPayload : IPayloadRequirements
    {
        public int Space { get; set; }
        public char[] BadChars { get; set; }
        public int StackAdjustment { get; set; }

        public bool IsAllowedPayload(Payload payload)
        {
            return false;
        }
    }
}