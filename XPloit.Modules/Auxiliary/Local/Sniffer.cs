using System;
using System.IO;
using System.Net;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Sniffer;
using XPloit.Core.Sniffer.Filters;
using XPloit.Core.Sniffer.Interfaces;
using XPloit.Core.Sniffer.Streams;

namespace Auxiliary.Local
{
    public class Sniffer : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "TCP Sniffer to file"; } }
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
        [ConfigurableProperty(Description = "Sniff this port")]
        public ushort LocalPort { get; set; }
        [ConfigurableProperty(Description = "Sniff only the Tor Request")]
        public bool OnlyTorRequest { get; set; }
        [ConfigurableProperty(Description = "Address for binding")]
        public IPAddress LocalAddress { get; set; }
        [ConfigurableProperty(Description = "Directory for creating TcpStream files")]
        public DirectoryInfo DumpFolder { get; set; }
        #endregion

        [NonJobable()]
        public override bool Run()
        {
            if (!SystemHelper.IsAdministrator())
                WriteError("Require admin rights");

            if (!DumpFolder.Exists) return false;

            if (OnlyTorRequest) TorHelper.UpdateTorExitNodeList(true);

            NetworkSniffer s = new NetworkSniffer(LocalAddress);
            s.OnTcpStream += s_OnTcpStream;
            s.Filters = OnlyTorRequest ?
                 new ITcpStreamFilter[] { new SnifferPortFilter(LocalPort), new SnifferTorFilter() }
                :
                 new ITcpStreamFilter[] { new SnifferPortFilter(LocalPort) };
            s.Start();

            CreateJob(s);
            return true;
        }

        public override ECheck Check()
        {
            NetworkSniffer s = null;
            try
            {
                if (!SystemHelper.IsAdministrator())
                    WriteError("Require admin rights");

                if (!DumpFolder.Exists)
                {
                    WriteError("DumpFolder must exists");
                    return ECheck.Error;
                }

                if (OnlyTorRequest) TorHelper.UpdateTorExitNodeList(true);

                s = new NetworkSniffer(LocalAddress);
                s.Filters = OnlyTorRequest ?
                 new ITcpStreamFilter[] { new SnifferPortFilter(LocalPort), new SnifferTorFilter() }
                :
                 new ITcpStreamFilter[] { new SnifferPortFilter(LocalPort) };

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

            stream.DumpToFile(DumpFolder.FullName + System.IO.Path.DirectorySeparatorChar +
                stream.Source.ToString().Replace(":", ",") + " - " +
                stream.Destination.ToString().Replace(":", ",") + ".dump");
        }
    }
}