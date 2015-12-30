
namespace XPloit.Core.Sniffer
{
    public interface ITcpStreamFilter
    {
        bool AllowTcpPacket(TcpHeader packet);
    }
}