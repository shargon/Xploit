using PacketDotNet;

namespace XPloit.Sniffer.Interfaces
{
    public interface IIpPacketFilter
    {
        bool IsAllowed(IpPacket ip, ushort portSource, ushort portDest);
    }
}