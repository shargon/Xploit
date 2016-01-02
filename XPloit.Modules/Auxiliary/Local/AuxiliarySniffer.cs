using System;
using System.IO;
using System.Net;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
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

        public override bool Run()
        {
            if (!SystemHelper.IsAdministrator())
                WriteError("Require admin rights");

            Sniffer s = new Sniffer(Address);
            s.OnTcpStream += s_OnTcpStream;
            s.Filter = new filter()
            {
                Port = this.Port
            };
            s.Start();

            CreateJob(s);
            return true;
        }

        public override ECheck Check()
        {
            Sniffer s = null;
            try
            {
                if (!SystemHelper.IsAdministrator())
                    WriteError("Require admin rights");

                s = new Sniffer(Address);
                s.Filter = new filter() { Port = this.Port };
                s.Start();

                return ECheck.Ok;
            }
            catch { return ECheck.Error; }
            finally
            {
                if (s != null) s.Dispose();
            }
        }

        void s_OnTcpStream(TcpStream stream)
        {
            if (stream == null) return;

            stream.DumpToFile(Folder + System.IO.Path.DirectorySeparatorChar +
                stream.Source.ToString().Replace(":", ",") + " - " +
                stream.Destination.ToString().Replace(":", ",") + ".dump");
        }

        class filter : ITcpStreamFilter
        {
            public ushort Port = 0;
            public bool AllowTcpPacket(TcpHeader packet) { return packet.DestinationPort == Port || packet.SourcePort == Port; }
        }
    }
}