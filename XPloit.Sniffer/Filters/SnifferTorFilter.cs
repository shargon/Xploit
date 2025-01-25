using System.Net;
using PacketDotNet;
using XPloit.Helpers;
using XPloit.Sniffer.Interfaces;

namespace XPloit.Sniffer.Filters
{
    public class SnifferTorFilter : IIpPacketFilter
    {
        public bool IsAllowed(IPEndPoint source, IPEndPoint dest, ProtocolType protocol)
        {
            TorHelper.UpdateTorExitNodeList(true);

            if (TorHelper.IsTorExitNode(source.Address)) return true;
            if (TorHelper.IsTorExitNode(dest.Address)) return true;

            return false;
        }
    }
}