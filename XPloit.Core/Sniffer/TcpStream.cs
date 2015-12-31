using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using XPloit.Core.Helpers;

namespace XPloit.Core.Sniffer
{
    public class TcpStream : IEnumerable<TcpStream.Stream>
    {
        public enum EEmisor { A, B }
        public class Stream
        {
            EEmisor _Emisor;
            byte[] _Data;
            int _Tag = 0;

            /// <summary>
            /// Tag
            /// </summary>
            public int Tag { get { return _Tag; } set { _Tag = value; } }
            /// <summary>
            /// Data
            /// </summary>
            public byte[] Data { get { return _Data; } }
            /// <summary>
            /// UTF8-Data
            /// </summary>
            public string DataAscii { get { return Encoding.ASCII.GetString(_Data); } }
            /// <summary>
            /// HEX-Data
            /// </summary>
            public string DataHex
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in _Data)
                    {
                        sb.Append(b.ToString("x2"));
                        sb.Append(" ");
                    }

                    return sb.ToString().Trim();
                }
            }
            /// <summary>
            /// IsSend
            /// </summary>
            public EEmisor Emisor { get { return _Emisor; } }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="data">Data</param>
            /// <param name="emisor">Emisor</param>
            public Stream(byte[] data, EEmisor emisor)
            {
                _Data = data;
                _Emisor = emisor;
            }
            /// <summary>
            /// Add data to Stream
            /// </summary>
            /// <param name="data">Data</param>
            internal void AddData(byte[] data)
            {
                if (data == null) return;
                int l = _Data.Length;
                if (l == 0) return;

                Array.Resize(ref _Data, l + data.Length);
                Array.Copy(data, 0, _Data, l, data.Length);
            }

            public override string ToString() { return Emisor.ToString() + " (" + _Data.Length.ToString() + ")"; }
        }

        Stream _Last;
        List<Stream> _InternalList = new List<Stream>();

        public Stream this[int index]
        {
            get
            {
                if (_InternalList.Count <= index) return null;
                return _InternalList[index];
            }
        }

        void Add(TcpHeader packet)
        {
            if (packet.Flags.HasFlag(TcpFlags.Fin))
                _IsClossed = true;

            if (packet.Data.Length <= 0) return;

            EEmisor emisor =
                (packet.DestinationPort == _DestinationPort && IPAddress.Equals(packet.IpHeader.DestinationAddress, _DestinationAddress) &&
                packet.SourcePort == _SourcePort && IPAddress.Equals(packet.IpHeader.SourceAddress, _SourceAddress)) ? EEmisor.A : EEmisor.B;

            if (_Last == null)
            {
                _Last = new Stream(packet.Data, emisor);
                _InternalList.Add(_Last);
            }
            else
            {
                // Check if its the same
                if (_Last.Emisor == emisor)
                    _Last.AddData(packet.Data);
                else
                {
                    // New Packet
                    _Last = new Stream(packet.Data, emisor);
                    _InternalList.Add(_Last);
                }
            }
        }

        public static TcpStream GetStream(List<TcpStream> streams, TcpHeader packet)
        {
            foreach (TcpStream stream in streams)
            {
                if (stream.IsSame(packet))
                {
                    stream.Add(packet);
                    return stream;
                }
            }

            if (packet.Data.Length == 0)
                return null;

            TcpStream ret = new TcpStream(packet);
            streams.Add(ret);
            return ret;
        }

        bool IsSame(TcpHeader packet)
        {
            //if (_SeqNumber != packet.SequenceNumber)
            //    return false;

            // same 
            if (packet.DestinationPort == _DestinationPort && packet.SourcePort == _SourcePort &&
                IPAddress.Equals(packet.IpHeader.DestinationAddress, _DestinationAddress) &&
                IPAddress.Equals(packet.IpHeader.SourceAddress, _SourceAddress))
                return true;

            // same
            if (packet.DestinationPort == _SourcePort && packet.SourcePort == _DestinationPort &&
                IPAddress.Equals(packet.IpHeader.DestinationAddress, _SourceAddress) &&
                IPAddress.Equals(packet.IpHeader.DestinationAddress, _SourceAddress))
                return true;

            return false;
        }

        //uint _SeqNumber;
        bool _IsClossed = false;
        ushort _DestinationPort, _SourcePort;
        IPAddress _DestinationAddress, _SourceAddress;

        public int Count { get { return _InternalList.Count; } }
        public TcpStream.Stream LastStream { get { return _Last; } }
        public bool IsClossed { get { return _IsClossed; } }
        public ushort DestinationPort { get { return _DestinationPort; } }
        public ushort SourcePort { get { return _SourcePort; } }
        public IPAddress SourceAddress { get { return _SourceAddress; } }
        public IPAddress DestinationAddress { get { return _DestinationAddress; } }
        public IPEndPoint Source { get { return new IPEndPoint(_SourceAddress, _SourcePort); } }
        public IPEndPoint Destination { get { return new IPEndPoint(_DestinationAddress, _DestinationPort); } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packet">Packet</param>
        public TcpStream(TcpHeader packet)
        {
            if (packet != null)
            {
                _DestinationPort = packet.DestinationPort;
                _SourcePort = packet.SourcePort;

                _DestinationAddress = packet.IpHeader.DestinationAddress;
                _SourceAddress = packet.IpHeader.SourceAddress;

                Add(packet);
            }
            else
            {
                _IsClossed = true;
            }
        }

        public IEnumerator<Stream> GetEnumerator() { return _InternalList.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return _InternalList.GetEnumerator(); }

        public override string ToString()
        {
            return Count.ToString() + (_IsClossed ? " [Clossed]" : "");
        }

        /// <summary>
        /// Load TCP Stream from WireShark TCPStreamFormat
        /// </summary>
        /// <param name="file">File</param>
        public static TcpStream FromFile(string file)
        {
            TcpStream tcp = new TcpStream(null);

            if (!string.IsNullOrEmpty(file))
            {
                string[] sp = File.ReadAllLines(file);

                foreach (string line in sp)
                {
                    string l = line.TrimStart().Replace(":", "");

                    EEmisor em = line.StartsWith(" ") ? EEmisor.A : EEmisor.B;
                   
                    if (l.Length >= 9 && !l.Substring(0, 8).Contains(" "))
                    {
                        // remove offset
                        l = l.Substring(8, l.Length - 8).Trim();
                    }

                    if (l.Length > 48)
                        l = l.Substring(0, 48);
                    l = l.Replace(" ", "");

                    byte[] data = HexHelper.FromHexString(l);

                    if (tcp._Last == null)
                    {
                        tcp._Last = new Stream(data, em);
                        tcp._InternalList.Add(tcp._Last);
                    }
                    else
                    {
                        // Check if its the same
                        if (tcp._Last.Emisor == em)
                            tcp._Last.AddData(data);
                        else
                        {
                            // New Packet
                            tcp._Last = new Stream(data, em);
                            tcp._InternalList.Add(tcp._Last);
                        }
                    }
                }
            }

            return tcp;
        }
    }
}