using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardDecValueException: Exception 
    {
        public CardDecValueException(String msg)
            : base(msg)
        {
        }
    }
}