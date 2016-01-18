using System;

namespace Xploit.Core.Rfid.Mifare.Internal
{
    internal class TrailerDataBlock: DataBlock
    {
        public TrailerDataBlock(Byte[] data)
            : base(3, data, true)
        {
        }

        public AccessBits AccessBits { get; private set; }
    }
}