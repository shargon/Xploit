using PacketDotNet;
using System.Collections.Generic;
using XPloit.Sniffer.Interfaces;
using System.Net;

namespace XPloit.Sniffer.Filters
{
    public class SnifferProtocolFilter : IIpPacketFilter
    {
        private readonly List<ProtocolType> _AvailableProtocols;

        /// <summary>
        /// Port
        /// </summary>
        public List<ProtocolType> AvailableProtocols { get { return _AvailableProtocols; } }

        public bool IsAllowed(IPEndPoint source, IPEndPoint dest, ProtocolType protocol)
        {
            return AvailableProtocols.Contains(protocol);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocols">Protocols</param>
        public SnifferProtocolFilter(params ProtocolType[] protocols)
        {
            _AvailableProtocols = new List<ProtocolType>(protocols);
        }

        public override string ToString() { return string.Join(",", _AvailableProtocols); }
    }
}