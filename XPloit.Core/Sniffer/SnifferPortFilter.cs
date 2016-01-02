namespace XPloit.Core.Sniffer
{
    public class SnifferPortFilter : ITcpStreamFilter
    {
        ushort _Port = 0;

        /// <summary>
        /// Port
        /// </summary>
        public ushort Port { get { return _Port; } }

        public bool AllowTcpPacket(TcpHeader packet) { return packet.DestinationPort == Port || packet.SourcePort == Port; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Port</param>
        public SnifferPortFilter(ushort port)
        {
            _Port = port;
        }
    }
}