using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardWriteValueException: Exception
    {
        public CardWriteValueException(String msg)
            : base(msg)
        {
        }
    }
}