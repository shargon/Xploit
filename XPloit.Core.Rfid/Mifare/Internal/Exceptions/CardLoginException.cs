using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardLoginException: Exception 
    {
        public CardLoginException(String msg)
            : base(msg)
        {
        }
    }
}