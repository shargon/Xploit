﻿using PacketDotNet;
using System.Collections.Concurrent;
using System.IO;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;
using XPloit.Sniffer.Streams;

namespace Payloads.Local.Sniffer
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Sniffer to folder")]
    public class DumpToFolder : Payload, Auxiliary.Local.Sniffer.IPayloadSniffer
    {
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
        public void OnPacket(object sender, IPProtocolType protocolType, EthernetPacket packet) { }
        public void OnTcpStream(object sender, TcpStream stream, bool isNew, ConcurrentQueue<object> queue)
        {
            if (stream == null) return;

            stream.DumpToFile(
                DumpFolder.FullName + Path.DirectorySeparatorChar +
                stream.Source.ToString().Replace(":", ",") + " - " +
                stream.Destination.ToString().Replace(":", ",") + ".dump");
        }
        public void Dequeue(object sender, object[] obj) { }
        public void Start(object sender) { }
        public void Stop(object sender) { }
    }
}