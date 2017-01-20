using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Xploit.Helpers.Geolocate;
using Xploit.Sniffer.Enums;
using Xploit.Sniffer.Streams;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;

namespace XPloit.Sniffer
{
    public class NetworkSniffer : IDisposable
    {
        const ushort BufferLength = 32 * 1024;
        //readonly Socket _socket;

        public delegate void delOnQueueObject(object sender, object enqueue);
        public delegate void delPacket(object sender, IPProtocolType protocolType, EthernetPacket packet);
        public delegate void delTcpStream(object sender, TcpStream stream, bool isNew, ConcurrentQueue<object> queue);

        public event CaptureStoppedEventHandler OnCaptureStop;
        public event delPacket OnPacket;
        public event delTcpStream OnTcpStream;

        IIpPacketFilter[] _Filters;
        TcpStreamStack _TcpStack = new TcpStreamStack();
        bool _IsDisposed, _HasFilters;
        ICaptureDevice _Device;

        BackgroundWorker _WorkerClean = new BackgroundWorker() { WorkerSupportsCancellation = true };
        BackgroundWorker _WorkerQueue = new BackgroundWorker() { WorkerSupportsCancellation = true };
        BlockingCollection<cPacket> _SyncPackets = new BlockingCollection<cPacket>();
        ConcurrentQueue<object> _Queue = new ConcurrentQueue<object>();

        public event delOnQueueObject OnDequeue;
        class cPacket
        {
            public DateTime Date;
            public PhysicalAddress HwSource, HwDest;
            public IPEndPoint IpSource, IpDest;
            public TcpPacket Tcp;
        }

        /// <summary>
        /// Read all pcap file
        /// </summary>
        /// <param name="pcapfile">PcapFile</param>
        public static IEnumerable<Packet> ReadAllPacketsFromPcap(string pcapfile)
        {
            CaptureFileReaderDevice device = new CaptureFileReaderDevice(pcapfile);
            device.Open();

            RawCapture r;
            while ((r = device.GetNextPacket()) != null)
            {
                Packet packet = Packet.ParsePacket(r.LinkLayerType, r.Data);
                if (packet != null) yield return packet;
            }
            device.Close();
        }

        /// <summary>
        /// Filter
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// Start tcp stream only with sync
        /// </summary>
        public EStartTcpStreamMethod StartTcpStreamMethod { get; set; }
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
        public bool IsDisposed { get { return _IsDisposed || _WorkerQueue == null || !_WorkerQueue.IsBusy || _WorkerClean == null || !_WorkerClean.IsBusy; } }
        /// <summary>
        /// Send packet
        /// </summary>
        /// <param name="p">Packet</param>
        public void Send(Packet p)
        {
            _Device.SendPacket(p);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceOrPcapfile">Device or pcap file</param>
        public NetworkSniffer(string deviceOrPcapfile)
        {
            if (File.Exists(deviceOrPcapfile)) _Device = new CaptureFileReaderDevice(deviceOrPcapfile);
            else _Device = CaptureDeviceList.Instance.Where(u => ((u is PcapDevice) ? ((PcapDevice)u).Interface.FriendlyName : u.Name) == deviceOrPcapfile).FirstOrDefault();

            if (_Device == null) throw (new Exception("Device '" + deviceOrPcapfile + "' not found!"));

            _WorkerQueue.DoWork += _Worker_DoWork;
            _WorkerClean.DoWork += _WorkerClean_DoWork;
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
            if (_WorkerQueue != null) while (_WorkerQueue.IsBusy) Thread.Sleep(50);
            if (_WorkerClean != null) while (_WorkerClean.IsBusy) Thread.Sleep(50);

            Stop();

            OnCaptureStop?.Invoke(sender, status);
        }
        DateTime _ProcessDate = DateTime.MaxValue;
        void _Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (cPacket pc in _SyncPackets.GetConsumingEnumerable())
            {
                TcpStream stream;
                bool isNew = _TcpStack.GetStream(pc.HwSource, pc.HwDest, pc.IpSource, pc.IpDest, pc.Tcp, pc.Date, StartTcpStreamMethod, out stream);

                _ProcessDate = pc.Date;
                if (stream != null && OnTcpStream != null) OnTcpStream(this, stream, isNew, _Queue);
            }

            _WorkerClean.CancelAsync();
        }
        void _WorkerClean_DoWork(object sender, DoWorkEventArgs e)
        {
            Task t = null;

            while (_WorkerClean != null && !_WorkerClean.CancellationPending && !e.Cancel)
            {
                if ((t == null || t.Exception != null || t.IsCompleted) && _Queue.Count > 0)
                {
                    if (t != null) t.Dispose();
                    t = new Task(OnProcessQueue);
                    t.Start();
                }

                Thread.Sleep(500);
                if (!((_WorkerClean != null && !_WorkerClean.CancellationPending && !e.Cancel))) break;

                if (OnTcpStream != null)
                    foreach (TcpStream stream in _TcpStack.CleanByTimeout(_ProcessDate))
                        OnTcpStream(this, stream, false, _Queue);

                Thread.Sleep(500);
            }

            if (OnTcpStream != null)
                foreach (TcpStream stream in _TcpStack.CleanByTimeout(DateTime.MaxValue))
                    OnTcpStream(this, stream, false, _Queue);

            if (t != null)
            {
                t.Wait();
                t.Dispose();
            }
            OnProcessQueue();
        }
        void OnProcessQueue()
        {
            if (GeoLite2LocationProvider.Current == null)
            {
#if DEBUG
                ///TODO: Config the default GeoIp
                GeoLite2LocationProvider.LoadCurrent(
                    @"D:\Fuentes\Xploit\Resources\GeoLite2\Small\GeoLite2-Blocks-IP.csv.gz",
                    @"D:\Fuentes\Xploit\Resources\GeoLite2\Small\GeoLite2-City-Locations-es.csv.gz");
#endif
            }

            Parallel.For(0, _Queue.Count, (i) =>
            {
                object o;
                if (_Queue.TryDequeue(out o))
                    try { OnDequeue?.Invoke(this, o); }
                    catch (Exception ex)
                    {
                        _Queue.Enqueue(o);
                    }
            });
        }
        void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            if (packet == null || !(packet is EthernetPacket)) return;

            EthernetPacket et = (EthernetPacket)packet;
            if (et.PayloadPacket == null || !(packet.PayloadPacket is IpPacket)) return;

            IpPacket ip = (IpPacket)et.PayloadPacket;
            if (ip == null || ip.PayloadPacket == null) return;

            packet = ip.PayloadPacket;
            switch (ip.Protocol)
            {
                case IPProtocolType.TCP:
                    {
                        TcpPacket p = (TcpPacket)packet;
                        IPEndPoint source = new IPEndPoint(ip.SourceAddress, p.SourcePort);
                        IPEndPoint dest = new IPEndPoint(ip.DestinationAddress, p.DestinationPort);

                        if (_HasFilters && !IsAllowedPacket(source, dest, IPProtocolType.TCP)) return;
                        OnPacket?.Invoke(this, ip.Protocol, et);

                        if (_SyncPackets.Count > 50000) Thread.Sleep(1);

                        _SyncPackets.TryAdd(new cPacket()
                        {
                            Date = e.Packet.Timeval.Date,

                            HwSource = et.SourceHwAddress,
                            IpSource = source,
                            HwDest = et.DestinationHwAddress,
                            IpDest = dest,

                            Tcp = p
                        });
                        break;
                    }
                case IPProtocolType.UDP:
                    {
                        UdpPacket p = (UdpPacket)packet;
                        IPEndPoint source = new IPEndPoint(ip.SourceAddress, p.SourcePort);
                        IPEndPoint dest = new IPEndPoint(ip.DestinationAddress, p.DestinationPort);

                        if (_HasFilters && !IsAllowedPacket(source, dest, IPProtocolType.UDP)) return;
                        OnPacket?.Invoke(this, ip.Protocol, et);
                        break;
                    }
                default:
                    {
                        IPEndPoint source = new IPEndPoint(ip.SourceAddress, 0);
                        IPEndPoint dest = new IPEndPoint(ip.DestinationAddress, 0);
                        if (_HasFilters && !IsAllowedPacket(source, dest, IPProtocolType.UDP)) return;
                        OnPacket?.Invoke(this, ip.Protocol, et);
                        break;
                    }
            }
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

            _Device.OnPacketArrival += Device_OnPacketArrival;
            _SyncPackets = new BlockingCollection<cPacket>();
            _Device.Open();
            if (!string.IsNullOrEmpty(Filter)) _Device.Filter = Filter;
            _Device.StartCapture();
            _WorkerQueue.RunWorkerAsync();
            _WorkerClean.RunWorkerAsync();

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
        bool IsAllowedPacket(IPEndPoint source, IPEndPoint dest, IPProtocolType protocol)
        {
            if (_Filters != null)
                foreach (IIpPacketFilter filter in _Filters)
                    if (!filter.IsAllowed(source, dest, protocol)) return false;

            return true;
        }
        public void Stop()
        {
            if (_Device != null)
            {
                try
                {
                    if (_Device.Started) _Device.Close();
                }
                catch (Exception e)
                {

                }

                if (_Device != null)
                    _Device.OnPacketArrival -= Device_OnPacketArrival;
            }

            _SyncPackets.CompleteAdding();

            if (_WorkerQueue != null) while (_WorkerQueue.IsBusy) Thread.Sleep(50);
            if (_WorkerClean != null) while (_WorkerClean.IsBusy) Thread.Sleep(50);

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

            if (_WorkerQueue != null)
            {
                _WorkerQueue.Dispose();
                _WorkerQueue = null;
            }
            if (_WorkerClean != null)
            {
                _WorkerClean.Dispose();
                _WorkerClean = null;
            }

            //try { _socket.Close(); }
            //catch { }
            //try { _socket.Dispose(); }
            //catch { }
        }
    }
}