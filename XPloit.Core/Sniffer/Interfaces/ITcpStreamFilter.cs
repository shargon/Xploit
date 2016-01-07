using XPloit.Core.Sniffer.Headers;

namespace XPloit.Core.Sniffer.Interfaces
{
    public interface ITcpStreamFilter
    {
        bool AllowTcpPacket(TcpHeader packet);
    }
}