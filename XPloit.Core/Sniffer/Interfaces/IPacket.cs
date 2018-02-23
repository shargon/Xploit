using XPloit.Core.Sniffer.Headers;

namespace XPloit.Core.Sniffer.Interfaces
{
    public interface IPacket
    {
        ushort SourcePort { get; }
        ushort DestinationPort { get; }
        byte[] Data { get; }
        IPHeader IpHeader { get; }
    }
}