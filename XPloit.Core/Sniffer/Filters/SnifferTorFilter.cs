using XPloit.Core.Helpers;
using XPloit.Core.Sniffer.Headers;
using XPloit.Core.Sniffer.Interfaces;

namespace XPloit.Core.Sniffer.Filters
{
    public class SnifferTorFilter : ITcpStreamFilter
    {
        public bool IsAllowed(TcpHeader packet)
        {
            TorHelper.UpdateTorExitNodeList(true);

            if (TorHelper.IsTorExitNode(packet.IpHeader.DestinationAddress)) return true;
            if (TorHelper.IsTorExitNode(packet.IpHeader.SourceAddress)) return true;

            return false;
        }
    }
}