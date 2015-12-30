using System.Net;
using System;
using System.IO;

namespace XPloit.Core.Sniffer
{
    [Flags]
    public enum TcpFlags
    {
        Fin = 1,
        Syn = 2,
        Rst = 4,
        Psh = 8,
        Ack = 16,
        Urg = 32
    }

    public class TcpHeader : IPacket
    {
        private readonly uint _uiAcknowledgementNumber = 555;
        private readonly ushort _usDataOffsetAndFlags = 555;
        private readonly ushort _usUrgentPointer;

        public ushort SourcePort { get; private set; }
        public ushort DestinationPort { get; private set; }
        public ushort WindowSize { get; private set; }

        public uint SequenceNumber { get; private set; }
        public byte HeaderLength { get; private set; }
        public short Checksum { get; private set; }
        public byte[] Data { get; private set; }
        public int MessageLength { get; private set; }

        public IPHeader IpHeader { get; private set; }

        public TcpHeader(IPHeader ipHeader)
        {
            IpHeader = ipHeader;
            byte[] buffer = IpHeader.Data;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    SourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    DestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    SequenceNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                    _uiAcknowledgementNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                    _usDataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    WindowSize = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    Checksum = (short)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    _usUrgentPointer = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    HeaderLength = (byte)(_usDataOffsetAndFlags >> 12);
                    HeaderLength *= 4;
                    MessageLength = buffer.Length - HeaderLength;
                }
            }

            // Data = new ArraySegment<byte>(buffer.Array, buffer.Offset + HeaderLength, MessageLength);
            byte[] data = new byte[MessageLength];
            Array.Copy(buffer, HeaderLength, data, 0, MessageLength);
            Data = data;
        }

        public string AcknowledgementNumber { get { return Flags.HasFlag(TcpFlags.Ack) ? _uiAcknowledgementNumber.ToString() : ""; } }
        public string UrgentPointer { get { return Flags.HasFlag(TcpFlags.Urg) ? _usUrgentPointer.ToString() : ""; } }
        public TcpFlags Flags { get { return (TcpFlags)(_usDataOffsetAndFlags & 0x3F); } }
    }
}