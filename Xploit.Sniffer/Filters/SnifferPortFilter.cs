using PacketDotNet;
using System.Collections.Generic;
using XPloit.Sniffer.Interfaces;

namespace XPloit.Sniffer.Filters
{
    public class SnifferPortFilter : IIpPacketFilter
    {
        List<ushort> _AvailablePorts;

        /// <summary>
        /// Port
        /// </summary>
        public List<ushort> AvailablePorts { get { return _AvailablePorts; } }

        public bool IsAllowed(IpPacket ip, ushort portSource, ushort portDest)
        {
            return AvailablePorts.Contains((ushort)portSource) || AvailablePorts.Contains((ushort)portDest);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ports">Ports</param>
        public SnifferPortFilter(params ushort[] ports)
        {
            _AvailablePorts = new List<ushort>(ports);
        }

        public override string ToString() { return string.Join(",", _AvailablePorts); }
    }
}