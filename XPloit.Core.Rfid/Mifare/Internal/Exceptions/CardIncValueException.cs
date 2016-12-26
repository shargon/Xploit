using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardIncValueException: Exception 
    {
        public CardIncValueException(String msg)
            : base(msg)
        {
        }
    }
}