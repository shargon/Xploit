using PacketDotNet;
using System.Net;
using XPloit.Core.Sockets.Configs;
using XPloit.Sniffer.Interfaces;

namespace XPloit.Sniffer.Filters
{
    public class SnifferIPFilter : IPFilter, IIpPacketFilter
    {
        public bool IsAllowed(IPEndPoint source, IPEndPoint dest, IPProtocolType protocol)
        {
            if (IsAllowed(source.Address)) return true;
            if (IsAllowed(dest.Address)) return true;

            return false;
        } 
    }
}