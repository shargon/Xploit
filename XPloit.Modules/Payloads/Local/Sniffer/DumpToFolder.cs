using System.IO;
using PacketDotNet;
using XPloit.Core;
using XPloit.Helpers.Attributes;
using XPloit.Sniffer.Streams;

namespace XPloit.Modules.Payloads.Local.Sniffer
{
    public class DumpToFolder : Payload, Auxiliary.Local.Sniffer.IPayloadSniffer
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Sniffer to folder"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Directory for creating TcpStream files")]
        public DirectoryInfo DumpFolder { get; set; }
        public bool CaptureOnTcpStream { get { return true; } }
        public bool CaptureOnPacket { get { return false; } }
        #endregion

        public bool Check()
        {
            if (!DumpFolder.Exists)
            {
                WriteError("DumpFolder must exists");
                return false;
            }

            return true;
        }
        public void OnPacket(IPProtocolType protocolType, IpPacket packet) { }
        public void OnTcpStream(TcpStream stream)
        {
            if (stream == null) return;

            stream.DumpToFile(
                DumpFolder.FullName + Path.DirectorySeparatorChar +
                stream.Source.ToString().Replace(":", ",") + " - " +
                stream.Destination.ToString().Replace(":", ",") + ".dump");
        }
    }
}