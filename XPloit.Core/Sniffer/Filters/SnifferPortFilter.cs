using XPloit.Core.Sniffer.Headers;
using XPloit.Core.Sniffer.Interfaces;

namespace XPloit.Core.Sniffer.Filters
{
    public class SnifferPortFilter : ITcpStreamFilter
    {
        ushort _Port = 0;

        /// <summary>
        /// Port
        /// </summary>
        public ushort Port { get { return _Port; } }

        public bool IsAllowed(TcpHeader packet) { return packet.DestinationPort == Port || packet.SourcePort == Port; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Port</param>
        public SnifferPortFilter(ushort port)
        {
            _Port = port;
        }
        public override string ToString() { return _Port.ToString(); }
    }
}