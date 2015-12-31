using System;
using System.IO;
using System.Net;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
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
            TcpStream.Stream l = stream.LastStream;
            if (l == null) return;

            int ld = l.Data.Length;
            int va = l.Tag;
            if (ld <= va) return;

            // Copy rest
            string path = Folder + System.IO.Path.DirectorySeparatorChar +
                stream.Source.ToString().Replace(":", ",") + " - " +
                stream.Destination.ToString().Replace(":", ",") + ".dump";

            StringBuilder sb = new StringBuilder();
            for (int x = va; x < ld; x++)
            {
                if (va + x % 16 == 0)
                {
                    // comienzo
                    if (stream.Count != 1 || x != 0)
                    {
                        // No esta empezando
                        sb.AppendLine();
                    }

                    sb.Append((l.Emisor == TcpStream.EEmisor.A ? "    " : ""));
                    sb.Append(x.ToString("x2").PadLeft(8, '0') + "  ");
                }
                else
                {
                    sb.Append(" ");
                }

                sb.Append(l.Data[l.Tag + x].ToString("x2"));
            }

            File.AppendAllText(path, sb.ToString());
            l.Tag = ld;
        }

        class filter : ITcpStreamFilter
        {
            public ushort Port = 0;
            public bool AllowTcpPacket(TcpHeader packet) { return packet.DestinationPort == Port || packet.SourcePort == Port; }
        }
    }
}