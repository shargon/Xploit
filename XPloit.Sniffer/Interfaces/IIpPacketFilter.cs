using PacketDotNet;
using System.Net;

namespace XPloit.Sniffer.Interfaces
{
    public interface IIpPacketFilter
    {
        bool IsAllowed(IPEndPoint source, IPEndPoint dest, ProtocolType protocol);
    }
}