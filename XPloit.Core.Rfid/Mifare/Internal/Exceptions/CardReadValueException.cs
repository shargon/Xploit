using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardReadValueException: Exception 
    {
        public CardReadValueException(String msg)
            : base(msg)
        {
        }
    }
}