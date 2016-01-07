using System.Diagnostics;
using System.Net;
using System;
using System.IO;
using System.Net.Sockets;
using XPloit.Core.Sniffer.Interfaces;

namespace XPloit.Core.Sniffer.Headers
{
    public enum IPVersion
    {
        None = 0,
        IPv4 = 1,
        IPv6 = 2
    }
    public enum IPFlag
    {
        Reserved = 0,
        DontFragment = 1,
        MoreFragments = 2
    }

    public class IPHeader
    {
        //IP Header fields
        readonly ushort _flagsAndOffset;
        readonly byte _protocol;

        public byte[] Raw { get; protected set; }
        public byte[] Data { get; protected set; }

        public int HeaderLength { get; private set; }
        public byte Ttl { get; private set; }
        public byte DifferentiatedServices { get; private set; }
        public byte CongestionNotification { get; private set; }

        public short Checksum { get; private set; }
        public ushort TotalLength { get; private set; }
        public ushort Identification { get; private set; }
        public IPAddress SourceAddress { get; private set; }
        public IPAddress DestinationAddress { get; private set; }

        public IPHeader(byte[] buffer, int length)
        {
            using (MemoryStream memoryStream = new MemoryStream(buffer, 0, length))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    byte versionAndHeaderLength = binaryReader.ReadByte();
                    byte differentiatedServices = binaryReader.ReadByte();

                    DifferentiatedServices = (byte)(differentiatedServices >> 2);
                    CongestionNotification = (byte)(differentiatedServices & 0x03);

                    TotalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    if (TotalLength != length)
                    {

                    }
                    //Debug.Assert(TotalLength >= 20, "Invalid IP packet Total Lenght");
                    Identification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    _flagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    Ttl = binaryReader.ReadByte();
                    _protocol = binaryReader.ReadByte();

                    Checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    SourceAddress = new IPAddress(binaryReader.ReadUInt32());
                    DestinationAddress = new IPAddress(binaryReader.ReadUInt32());

                    HeaderLength = (versionAndHeaderLength & 0x0f) * 4;
                }
                Raw = memoryStream.ToArray();
            }

            byte[] data = new byte[MessageLength];
            Array.Copy(buffer, HeaderLength, data, 0, MessageLength);
            Data = data;
        }


        public IPVersion Version
        {
            get
            {
                var ver = HeaderLength >> 4;
                if (ver == 4) return IPVersion.IPv4;
                if (ver == 6) return IPVersion.IPv6;
                return IPVersion.None;
            }
        }

        public ProtocolType ProtocolType
        {
            get
            {
                if (_protocol == 6) return ProtocolType.Tcp;
                if (_protocol == 17) return ProtocolType.Udp;
                return ProtocolType.Unknown;
            }
        }

        public int MessageLength
        {
            get { return TotalLength - HeaderLength; }
        }

        public IPFlag Flags
        {
            get { return (IPFlag)(_flagsAndOffset >> 13); }
        }

        public int FragmentationOffset
        {
            get { return (_flagsAndOffset << 3) >> 3; }
        }

        public bool IsFragment
        {
            get { return Flags == IPFlag.MoreFragments || FragmentationOffset > 0; }
        }

        /// <summary>
        /// Parse packet
        /// </summary>
        public IPacket ParseData()
        {
            switch (ProtocolType)
            {
                case ProtocolType.Tcp: return new TcpHeader(this);
                case ProtocolType.Udp: return new UdpHeader(this);
                case ProtocolType.Unknown: return null;
            }

            return null;
        }
    }
}
