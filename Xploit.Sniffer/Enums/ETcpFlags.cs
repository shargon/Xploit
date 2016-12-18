using System;

namespace XPloit.Sniffer.Enums
{
    [Flags]
    public enum ETcpFlags
    {
        Fin = 1,
        Syn = 2,
        Rst = 4,
        Psh = 8,
        Ack = 16,
        Urg = 32
    }
}