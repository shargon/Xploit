using PacketDotNet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using XPloit.Helpers;
using XPloit.Sniffer.Enums;

namespace XPloit.Sniffer.Streams
{
    public class TcpStream : IEnumerable<TcpStreamMessage>
    {
        bool _IsFirst;
        TcpStreamMessage _Last;
        List<TcpStreamMessage> _InternalList = new List<TcpStreamMessage>();

        public TcpStreamMessage this[int index]
        {
            get
            {
                if (_InternalList.Count <= index) return null;
                return _InternalList[index];
            }
        }

        void Add(IpPacket ip, TcpPacket tcp)
        {
            if (tcp.Fin)
                _IsClossed = true;

            if (tcp.PayloadData.Length <= 0) return;

            ETcpEmisor emisor =
                (tcp.DestinationPort == _DestinationPort && Equals(ip.DestinationAddress, _DestinationAddress) &&
                tcp.SourcePort == _SourcePort && Equals(ip.SourceAddress, _SourceAddress)) ? ETcpEmisor.A : ETcpEmisor.B;

            if (_Last == null)
            {
                _Last = new TcpStreamMessage(tcp.PayloadData, emisor);
                _InternalList.Add(_Last);
            }
            else
            {
                // Check if its the same
                if (_Last.Emisor == emisor)
                    _Last.AddData(tcp.PayloadData);
                else
                {
                    // New Packet
                    _Last = new TcpStreamMessage(tcp.PayloadData, emisor);
                    _InternalList.Add(_Last);
                }
            }
        }

        public static TcpStream GetStream(List<TcpStream> streams, IpPacket ip, TcpPacket tcp)
        {
            foreach (TcpStream stream in streams)
                if (stream.IsSame(ip, tcp))
                {
                    stream.Add(ip, tcp);
                    return stream;
                }

            if (tcp.PayloadData.Length == 0)
                return null;

            TcpStream ret = new TcpStream(ip, tcp);
            streams.Add(ret);
            return ret;
        }

        bool IsSame(IpPacket ip, TcpPacket tcp)
        {
            //if (_SeqNumber != packet.SequenceNumber)
            //    return false;

            // same 
            if (tcp.DestinationPort == _DestinationPort && tcp.SourcePort == _SourcePort &&
                Equals(ip.DestinationAddress, _DestinationAddress) && Equals(ip.SourceAddress, _SourceAddress))
                return true;

            // same
            if (tcp.DestinationPort == _SourcePort && tcp.SourcePort == _DestinationPort &&
                Equals(ip.DestinationAddress, _SourceAddress) && Equals(ip.DestinationAddress, _SourceAddress))
                return true;

            return false;
        }

        //uint _SeqNumber;
        const int DumpStep = 16;
        byte _Writed = 0;
        char[] _LastWrited = new char[DumpStep];
        bool _IsClossed = false;
        ushort _DestinationPort, _SourcePort;
        IPAddress _DestinationAddress, _SourceAddress;

        public int Count { get { return _InternalList.Count; } }
        public TcpStreamMessage LastStream { get { return _Last; } }
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
        /// <param name="ip">Ip</param>
        /// <param name="tcp">Packet</param>
        public TcpStream(IpPacket ip, TcpPacket tcp)
        {
            _IsFirst = true;
            if (ip != null && tcp != null)
            {
                _DestinationPort = tcp.DestinationPort;
                _SourcePort = tcp.SourcePort;

                _DestinationAddress = ip.DestinationAddress;
                _SourceAddress = ip.SourceAddress;

                Add(ip, tcp);
            }
            else
            {
                _IsClossed = true;
            }
        }

        public IEnumerator<TcpStreamMessage> GetEnumerator() { return _InternalList.GetEnumerator(); }
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
            TcpStream tcp = new TcpStream(null, null);

            if (!string.IsNullOrEmpty(file))
            {
                string[] sp = File.ReadAllLines(file);

                foreach (string line in sp)
                {
                    string l = line.TrimStart().Replace(":", "");

                    ETcpEmisor em = line.StartsWith(" ") ? ETcpEmisor.A : ETcpEmisor.B;

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
                        tcp._Last = new TcpStreamMessage(data, em);
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
                            tcp._Last = new TcpStreamMessage(data, em);
                            tcp._InternalList.Add(tcp._Last);
                        }
                    }
                }
            }

            return tcp;
        }

        /// <summary>
        /// Dump to file
        /// </summary>
        /// <param name="file">File</param>
        public void DumpToFile(string file)
        {
            TcpStreamMessage l = _Last;
            if (l == null) return;

            int ld, va;
            lock (l)
            {
                ld = l.Data.Length;
                va = l._LastRead;
                if (ld <= va) return;
            }

            StringBuilder sb = new StringBuilder();
            for (int x = va; x < ld; x++)
            {
                if (x % DumpStep == 0)
                {
                    // comienzo
                    if (_IsFirst) _IsFirst = false;
                    else
                    {
                        if (_Writed != DumpStep)
                        {
                            // Spaces
                            sb.Append("".PadLeft((DumpStep - _Writed) * 3, ' '));
                        }

                        sb.Append("\t\t");

                        for (int xx = 0; xx < _Writed; xx++) sb.Append(_LastWrited[xx]);

                        // No esta empezando
                        sb.AppendLine();
                    }

                    _Writed = 0;
                    sb.Append((l.Emisor == ETcpEmisor.A ? "\t\t" : ""));
                    sb.Append(x.ToString("x2").PadLeft(8, '0') + "  ");
                }
                else
                {
                    sb.Append(" ");
                }

                byte val = l.Data[x - va];
                sb.Append(val.ToString("x2"));

                char cv = (char)val;
                if (char.IsControl(cv)) cv = '.';

                _LastWrited[_Writed] = cv;
                _Writed++;
            }

            File.AppendAllText(file, sb.ToString());
            l._LastRead = ld;
        }
    }
}