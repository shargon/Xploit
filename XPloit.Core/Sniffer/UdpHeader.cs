using System.Net;
using System;
using System.IO;

namespace XPloit.Core.Sniffer
{
    public class UdpHeader : IPacket
    {
        public short Checksum { get; private set; }
        public ushort SourcePort { get; private set; }
        public ushort DestinationPort { get; private set; }
        public ushort Length { get; private set; }
        public byte[] Data { get; private set; }
        public IPHeader IpHeader { get; private set; }

        public UdpHeader(IPHeader ipHeader)
        {
            IpHeader = ipHeader;
            byte[] buffer = IpHeader.Data;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    SourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    DestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    Length = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    Checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                }
            }

            byte[] data = new byte[buffer.Length - 8];
            Array.Copy(buffer, 8, data, 0, data.Length);
            Data = data;
            //Data = new ArraySegment<byte>(buffer.Array, buffer.Offset + 8, buffer.Count - 8);
        }
    }
}