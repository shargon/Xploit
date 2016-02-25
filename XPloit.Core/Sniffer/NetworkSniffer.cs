using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using XPloit.Core.Interfaces;
using XPloit.Core.Sniffer.Headers;
using XPloit.Core.Sniffer.Interfaces;
using XPloit.Core.Sniffer.Streams;

namespace XPloit.Core.Sniffer
{
    public class NetworkSniffer : IJobable
    {
        const ushort BufferLength = 32 * 1024;
        readonly Socket _socket;

        public delegate void delPacket(ProtocolType protocolType, IPacket packet);
        public delegate void delTcpStream(TcpStream stream);

        public event delPacket OnPacket;
        public event delTcpStream OnTcpStream;

        ITcpStreamFilter[] _Filters = null;
        List<TcpStream> _TcpStreams = new List<TcpStream>();

        /// <summary>
        /// Filter
        /// </summary>
        public ITcpStreamFilter[] Filters { get { return _Filters; } set { _Filters = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bindTo">Ip for bind</param>
        public NetworkSniffer(IPAddress bindTo)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            _socket.Bind(new IPEndPoint(bindTo, 0));
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4] { 1, 0, 0, 0 };

            _socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
        }
        /// <summary>
        /// Start sniffing
        /// </summary>
        public void Start() { Receive(); }
        void Receive()
        {
            byte[] header = new byte[BufferLength];//ushort.MaxValue];
            //int l = _socket.Receive(header, SocketFlags.None);
            //rec(header, l);
            _socket.BeginReceive(header, 0, header.Length, SocketFlags.None, OnReceive, header);
        }
        void OnReceive(IAsyncResult ar)
        {
            try
            {
                int received = _socket.EndReceive(ar);
                rec((byte[])ar.AsyncState, received);
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
        }
        bool AllowTcpPacket(TcpHeader tcp)
        {
            if (_Filters == null) return true;
            foreach (ITcpStreamFilter filter in _Filters)
            {
                if (!filter.IsAllowed(tcp)) return false;
            }
            return true;
        }
        /// <summary>
        /// Liberación de recursos
        /// </summary>
        public override void OnDispose()
        {
            if (_socket == null) return;

            try { _socket.Close(); }
            catch { }
            try { _socket.Dispose(); }
            catch { }
        }
    }
}