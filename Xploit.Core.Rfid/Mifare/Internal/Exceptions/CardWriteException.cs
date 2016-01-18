using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardWriteException: Exception
    {
        public CardWriteException(String msg)
            : base(msg)
        {
        }
    }
}