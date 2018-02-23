﻿using PacketDotNet;
using System.Collections.Generic;
using XPloit.Sniffer.Interfaces;
using System.Net;

namespace XPloit.Sniffer.Filters
{
    public class SnifferProtocolFilter : IIpPacketFilter
    {
        List<IPProtocolType> _AvailableProtocols;

        /// <summary>
        /// Port
        /// </summary>
        public List<IPProtocolType> AvailableProtocols { get { return _AvailableProtocols; } }

        public bool IsAllowed(IPEndPoint source, IPEndPoint dest, IPProtocolType protocol)
        {
            return AvailableProtocols.Contains(protocol);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocols">Protocols</param>
        public SnifferProtocolFilter(params IPProtocolType[] protocols)
        {
            _AvailableProtocols = new List<IPProtocolType>(protocols);
        }

        public override string ToString() { return string.Join(",", _AvailableProtocols); }
    }
}