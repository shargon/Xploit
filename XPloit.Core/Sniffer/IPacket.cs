namespace XPloit.Core.Sniffer
{
    public interface IPacket
    {
        ushort SourcePort { get; }
        ushort DestinationPort { get; }
        byte[] Data { get; }
        IPHeader IpHeader { get; }
    }
}