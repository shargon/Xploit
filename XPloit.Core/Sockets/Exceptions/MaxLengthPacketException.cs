using System;

namespace XPloit.Core.Sockets.Exceptions
{
    public class MaxLengthPacketException : Exception
    {
        public MaxLengthPacketException() : base("Max Length packet Exception") { }
    }
}