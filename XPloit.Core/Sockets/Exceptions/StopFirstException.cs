using System;

namespace XPloit.Core.Sockets.Exceptions
{
    public class StopFirstException : Exception
    {
        public StopFirstException() : base("Stop first server") { }
    }
}