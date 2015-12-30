using System;
using System.IO;
using System.Net;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.Sniffer;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliarySniffer : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "TCP Sniffer to file"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override bool IsRemote { get { return false; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Name { get { return "Sniffer"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] 
                { 
                    new Reference(EReferenceType.INFO, "For outward, requiere open Firewall for promiscuous mode") ,
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Sniff this port")]
        public ushort Port { get; set; }
        [ConfigurableProperty(Required = true, Description = "Address for binding")]
        public IPAddress Address { get; set; }
        [ConfigurableProperty(Required = true, Description = "Directory for creating TcpStream files")]
        public string Folder { get; set; }
        #endregion

        public override bool Run(ICommandLayer cmd)
        {
            Sniffer s = new Sniffer(Address);
            s.OnTcpStream += s_OnTcpStream;
            s.Filter = new filter() { Port = this.Port };
            s.Start();

            Job.Create(cmd, s);
            return true;
        }

        public override ECheck Check(ICommandLayer cmd)
        {
            Sniffer s = null;
            try
            {
                s = new Sniffer(Address);
                s.Filter = new filter() { Port = this.Port };
                s.Start();

                return ECheck.Ok;
            }
            catch
            {
                return ECheck.Error;
            }
            finally
            {
                if (s != null) s.Dispose();
            }
        }

        void s_OnTcpStream(TcpStream stream)
        {
            TcpStream.Stream l = stream.LastStream;
            if (l != null)
            {
                int ld = l.Data.Length;

                if (ld > l.Tag)
                {
                    // Copy rest
                    byte[] dd = new byte[ld - l.Tag];
                    Array.Copy(l.Data, l.Tag, dd, 0, dd.Length);

                    string path = Folder + System.IO.Path.DirectorySeparatorChar +
                        stream.Source.ToString().Replace(":", ",") + " - " +
                        stream.Destination.ToString().Replace(":", ",") + ".dump";

                    File.AppendAllText(path,
                        (l.Tag == 0 ? (l.Emisor == TcpStream.EEmisor.A ? ">" : "<") : "") +
                        l.DataHex);

                    l.Tag = ld;
                }
            }
        }

        class filter : ITcpStreamFilter
        {
            public ushort Port = 0;

            public bool AllowTcpPacket(TcpHeader packet)
            {
                return packet.DestinationPort == Port || packet.SourcePort == Port;
            }
        }
    }
}