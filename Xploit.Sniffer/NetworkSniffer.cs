using SharpPcap;
using System;
using System.Linq;
using System.Collections.Generic;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;
using System.IO;
using SharpPcap.LibPcap;
using PacketDotNet;
using System.Net;

namespace XPloit.Sniffer
{
    public class NetworkSniffer : IDisposable
    {
        const ushort BufferLength = 32 * 1024;
        //readonly Socket _socket;

        public delegate void delPacket(IPProtocolType protocolType, IpPacket packet);
        public delegate void delTcpStream(TcpStream stream);

        public event delPacket OnPacket;
        public event delTcpStream OnTcpStream;

        IIpPacketFilter[] _Filters = null;
        List<TcpStream> _TcpStreams = new List<TcpStream>();
        bool _IsDisposed, _HasFilters;
        ICaptureDevice _Device;

        /// <summary>
        /// Filter
        /// </summary>
        public IIpPacketFilter[] Filters
        {
            get { return _Filters; }
            set
            {
                _Filters = value;
                _HasFilters = _Filters != null && _Filters.Length > 0;
            }
        }
        /// <summary>
        /// Capture devices
        /// </summary>
        public static string[] CaptureDevices
        {
            get
            {
                string[] ar = CaptureDeviceList.Instance.Select(u => (u is PcapDevice) ? ((PcapDevice)u).Interface.FriendlyName : u.Name).ToArray();
                Array.Sort(ar);
                return ar;
            }
        }
        /// <summary>
        /// return IsDisposed
        /// </summary>
        public bool IsDisposed { get { return _IsDisposed; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceOrPcapfile">Device or pcap file</param>
        public NetworkSniffer(string deviceOrPcapfile)
        {
            if (File.Exists(deviceOrPcapfile)) _Device = new CaptureFileReaderDevice(deviceOrPcapfile);
            else _Device = CaptureDeviceList.Instance.Where(u => ((u is PcapDevice) ? ((PcapDevice)u).Interface.FriendlyName : u.Name) == deviceOrPcapfile).FirstOrDefault();

            if (_Device == null) throw (new Exception("Device '" + deviceOrPcapfile + "' not found!"));

            _Device.OnPacketArrival += Device_OnPacketArrival;

            // Open the device for capturing

            //_socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            //_socket.Bind(new IPEndPoint(bindTo, 0));
            //_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            //byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            //byte[] byOut = new byte[4] { 1, 0, 0, 0 };

            //_socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
        }
        void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            if (packet == null || !(packet is EthernetPacket)) return;

            EthernetPacket et = (EthernetPacket)packet;
            if (et.PayloadPacket == null || !(packet.PayloadPacket is IpPacket)) return;

            IpPacket ip = (IpPacket)et.PayloadPacket;
            if (ip == null || ip.PayloadPacket == null) return;

            OnPacket?.Invoke(ip.Protocol, ip);

            switch (ip.Protocol)
            {
                case IPProtocolType.TCP:
                    {
                        if (!(ip.PayloadPacket is TcpPacket)) return;

                        TcpPacket p = (TcpPacket)ip.PayloadPacket;
                        if (_HasFilters && !IsAllowedPacket(ip, p.SourcePort, p.DestinationPort)) return;

                        TcpStream stream = TcpStream.GetStream(_TcpStreams, ip, p);
                        if (stream != null)
                        {
                            if (stream.IsClossed) _TcpStreams.Remove(stream);
                            OnTcpStream(stream);
                        }
                        break;
                    }
                case IPProtocolType.UDP:
                    {
                        if (!(ip.PayloadPacket is UdpPacket)) return;

                        UdpPacket p = (UdpPacket)ip.PayloadPacket;
                        if (_HasFilters && !IsAllowedPacket(ip, p.SourcePort, p.DestinationPort)) return;

                        break;
                    }
            }
        }

        /// <summary>
        /// Start sniffing
        /// </summary>
        public void Start()
        {
            _Device.Open();
            _Device.StartCapture();
            //Receive();
        }
        /*
        void Receive()
        {
            //byte[] header = new byte[BufferLength];
            //_socket.BeginReceive(header, 0, header.Length, SocketFlags.None, OnReceive, header);
        }
        void OnReceive(IAsyncResult ar)
        {
            try
            {
                //int received = _socket.EndReceive(ar);
                //rec((byte[])ar.AsyncState, received);
            }
            catch
            {

            }
        }
        void rec(byte[] data, int length)
        {
            IPHeader ipHeader = new IPHeader(data, length);
            IPacket packet = ipHeader.ParseData();

            if (OnPacket != null)
                OnPacket(ipHeader.ProtocolType, packet);

            if (OnTcpStream != null && ipHeader.ProtocolType == ProtocolType.Tcp)
            {
                TcpHeader tcp = (TcpHeader)packet;

                if (AllowTcpPacket(tcp))
                {
                    TcpStream stream = TcpStream.GetStream(_TcpStreams, tcp);
                    if (stream != null)
                    {
                        if (stream.IsClossed)
                            _TcpStreams.Remove(stream);
                        OnTcpStream(stream);
                    }
                }
            }

            Receive();
        }*/
        bool IsAllowedPacket(IpPacket ip, ushort sourcePort, ushort destPort)
        {
            if (_Filters == null) return true;
            foreach (IIpPacketFilter filter in _Filters)
                if (!filter.IsAllowed(ip, sourcePort, destPort)) return false;

            return true;
        }
        /// <summary>
        /// Liberación de recursos
        /// </summary>
        public void Dispose()
        {
            if (_Device == null) return;

            _IsDisposed = true;

            try
            {
                _Device.StopCapture();
                _Device.Close();
            }
            catch { }
            _Device = null;

            //try { _socket.Close(); }
            //catch { }
            //try { _socket.Dispose(); }
            //catch { }
        }
    }
}