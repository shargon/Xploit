using System;

namespace Xploit.Core.Rfid.Mifare.Internal.Exceptions
{
    public class CardReadException: Exception 
    {
        public CardReadException(String msg)
            : base(msg)
        {
        }
    }
}