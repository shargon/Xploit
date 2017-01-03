using PacketDotNet;
using SharpPcap;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xploit.Sniffer.Enums;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Sniffer;
using XPloit.Sniffer.Filters;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;

namespace Auxiliary.Local
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Sniffer")]
    public class Sniffer : Module
    {
        public interface IPayloadSniffer
        {
            bool CaptureOnTcpStream { get; }
            bool CaptureOnPacket { get; }
            bool Check();

            void Dequeue(object sender, object obj);
            void OnTcpStream(object sender, TcpStream stream, bool isNew, ConcurrentQueue<object> queue);
            void OnPacket(object sender, IPProtocolType protocolType, IpPacket packet);
        }

        string _Interface;

        #region Configure
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.INFO, "For outward, requiere open Firewall for promiscuous mode"), }; }
        }
        public override IPayloadRequirements PayloadRequirements
        {
            get { return new InterfacePayload(typeof(IPayloadSniffer)); }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Sniff this port", Optional = true)]
        public ushort[] FilterPorts { get; set; }
        [ConfigurableProperty(Description = "Filter", Optional = true)]
        public string Filter { get; set; }
        [ConfigurableProperty(Description = "Filter protocols", Optional = true)]
        public IPProtocolType[] FilterProtocols { get; set; }
        [ConfigurableProperty(Description = "Filter only the Tor Request")]
        public bool FilterOnlyTorRequest { get; set; }
        [AutoFill("GetAllDevices")]
        [ConfigurableProperty(Description = "Capture device or pcap file")]
        public string Interface
        {
            get
            {
                if (string.IsNullOrEmpty(_Interface)) _Interface = NetworkSniffer.CaptureDevices.FirstOrDefault();
                return _Interface;
            }
            set { _Interface = value; }
        }
        [ConfigurableProperty(Description = "Start tcp stream only with SYNC")]
        public EStartTcpStreamMethod StartTcpStreamMethod { get; set; }
        #endregion

        public Sniffer()
        {
            //FilterProtocols = new IPProtocolType[] { IPProtocolType.TCP, IPProtocolType.UDP };
            StartTcpStreamMethod = EStartTcpStreamMethod.Sync;
        }
        public string[] GetAllDevices() { return NetworkSniffer.CaptureDevices; }
        [NonJobable()]
        public override bool Run()
        {
            IPayloadSniffer pay = (IPayloadSniffer)Payload;
            //if (!SystemHelper.IsAdministrator())
            //    WriteError("Require admin rights");
            if (!pay.Check()) return false;
            if (FilterOnlyTorRequest) TorHelper.UpdateTorExitNodeList(true);

            NetworkSniffer s = new NetworkSniffer(Interface);
            s.StartTcpStreamMethod = StartTcpStreamMethod;
            s.OnDequeue += pay.Dequeue;

            if (!string.IsNullOrEmpty(Filter)) s.Filter = Filter;
            if (pay.CaptureOnTcpStream) s.OnTcpStream += pay.OnTcpStream;
            if (pay.CaptureOnPacket) s.OnPacket += pay.OnPacket;

            List<IIpPacketFilter> filters = new List<IIpPacketFilter>();

            if (FilterOnlyTorRequest) filters.Add(new SnifferTorFilter());
            if (FilterPorts != null && FilterPorts.Length > 0) filters.Add(new SnifferPortFilter(FilterPorts));
            if (FilterProtocols != null && FilterProtocols.Length > 0) filters.Add(new SnifferProtocolFilter(FilterProtocols));

            s.Filters = filters.ToArray();
            s.OnCaptureStop += S_OnCaptureStop;
            s.Start();

            CreateJob(s, "IsDisposed");
            return true;
        }
        void S_OnCaptureStop(object sender, CaptureStoppedEventStatus status)
        {
            WriteInfo("Capture stopped");
        }
        public override ECheck Check()
        {
            NetworkSniffer s = null;
            try
            {
                //if (!SystemHelper.IsAdministrator())
                //    WriteError("Require admin rights");

                IPayloadSniffer pay = (IPayloadSniffer)Payload;
                if (!pay.Check()) return ECheck.Error;

                s = new NetworkSniffer(Interface);
                s.Start();

                return ECheck.Ok;
            }
            catch { return ECheck.Error; }
            finally
            {
                if (s != null)
                    s.Dispose();
            }
        }
    }
}