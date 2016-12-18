using PacketDotNet;
using XPloit.Helpers;
using XPloit.Sniffer.Interfaces;

namespace XPloit.Sniffer.Filters
{
    public class SnifferTorFilter : IIpPacketFilter
    {
        public bool IsAllowed(IpPacket ip, ushort portSource, ushort portDest)
        {
            TorHelper.UpdateTorExitNodeList(true);

            if (TorHelper.IsTorExitNode(ip.SourceAddress)) return true;
            if (TorHelper.IsTorExitNode(ip.DestinationAddress)) return true;

            return false;
        }
    }
}