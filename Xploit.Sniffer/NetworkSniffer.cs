using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;

namespace XPloit.Sniffer
{
    public class NetworkSniffer : IDisposable
    {
        const ushort BufferLength = 32 * 1024;
        //readonly Socket _socket;

        public delegate void delPacket(IPProtocolType protocolType, IpPacket packet);
        public delegate void delTcpStream(TcpStream stream, bool isNew);

        public event CaptureStoppedEventHandler OnCaptureStop;
        public event delPacket OnPacket;
        public event delTcpStream OnTcpStream;

        IIpPacketFilter[] _Filters;
        Dictionary<string, TcpStream> _TcpStreams = new Dictionary<string, TcpStream>();
        bool _IsDisposed, _HasFilters;
        ICaptureDevice _Device;

        BackgroundWorker _Worker = new BackgroundWorker() { WorkerSupportsCancellation = true };
        BlockingCollection<cPacket> _SyncPackets = new BlockingCollection<cPacket>();

        class cPacket
        {
            public DateTime Date;
            public IpPacket Packet;
        }

        /// <summary>
        /// Filter
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// Start tcp stream only with sync
        /// </summary>
        public bool StartTcpStreamOnlyWithSync { get; set; }
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
        public bool IsDisposed { get { return _IsDisposed || _Worker == null || !_Worker.IsBusy; } }
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
            _Worker.DoWork += _Worker_DoWork;
            _Device.OnCaptureStopped += _Device_OnCaptureStopped;
            // Open the device for capturing

            //_socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            //_socket.Bind(new IPEndPoint(bindTo, 0));
            //_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            //byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            //byte[] byOut = new byte[4] { 1, 0, 0, 0 };

            //_socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
        }
        void _Device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
        {
            _SyncPackets.CompleteAdding();
            while (_Worker.IsBusy) Thread.Sleep(50);

            Stop();

            OnCaptureStop?.Invoke(sender, status);
        }
        void _Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (cPacket pc in _SyncPackets.GetConsumingEnumerable())
            {
                Packet packet = pc.Packet.PayloadPacket;
                ushort sourcePort = 0, destPort = 0;
                switch (pc.Packet.Protocol)
                {
                    case IPProtocolType.TCP:
                        {
                            TcpPacket p = (TcpPacket)packet;
                            sourcePort = p.SourcePort;
                            destPort = p.DestinationPort;
                            break;
                        }
                    case IPProtocolType.UDP:
                        {
                            UdpPacket p = (UdpPacket)packet;
                            sourcePort = p.SourcePort;
                            destPort = p.DestinationPort;
                            break;
                        }
                }

                if (_HasFilters && !IsAllowedPacket(pc.Packet, sourcePort, destPort)) continue;

                OnPacket?.Invoke(pc.Packet.Protocol, pc.Packet);

                if (pc.Packet.Protocol == IPProtocolType.TCP)
                {
                    bool isNew;

                    TcpStream stream = TcpStream.GetStream(_TcpStreams, pc.Packet, (TcpPacket)packet, StartTcpStreamOnlyWithSync, out isNew);
                    if (stream == null) continue;

                    OnTcpStream(stream, isNew);

                    if (stream.IsClossed) _TcpStreams.Remove(stream.Key);
                    else
                    {
                        if (isNew) _TcpStreams.Add(stream.Key, stream);

                        // Ver colgados
                    }
                }
            }
        }
        void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            if (packet == null || !(packet is EthernetPacket)) return;

            EthernetPacket et = (EthernetPacket)packet;
            if (et.PayloadPacket == null || !(packet.PayloadPacket is IpPacket)) return;

            IpPacket ip = (IpPacket)et.PayloadPacket;
            if (ip == null || ip.PayloadPacket == null) return;

            _SyncPackets.TryAdd(new cPacket() { Date = e.Packet.Timeval.Date, Packet = ip });
        }
        /// <summary>
        /// Start sniffing
        /// </summary>
        public void Start()
        {
            Stop();

            //if (StartTcpStreamOnlyWithSync) 
            //_ParallelPackets = new BlockingCollection<Packet>();
            //else 

            _SyncPackets = new BlockingCollection<cPacket>();
            _Device.Open();
            if (!string.IsNullOrEmpty(Filter)) _Device.Filter = Filter;
            _Device.StartCapture();
            _Worker.RunWorkerAsync();

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
        public void Stop()
        {
            try
            {
                if (_Device.Started) _Device.Close();
            }
            catch { }

            _SyncPackets.CompleteAdding();

            while (_Worker.IsBusy) Thread.Sleep(50);

            // Clean
            while (_SyncPackets.Count > 0)
            {
                cPacket item;
                _SyncPackets.TryTake(out item);
            }
        }
        /// <summary>
        /// Liberación de recursos
        /// </summary>
        public void Dispose()
        {
            if (_Device == null) return;

            Stop();

            _IsDisposed = true;
            _Device = null;

            //try { _socket.Close(); }
            //catch { }
            //try { _socket.Dispose(); }
            //catch { }
        }
    }
}