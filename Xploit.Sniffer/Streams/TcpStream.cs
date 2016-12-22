using PacketDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Xploit.Sniffer.Streams;
using XPloit.Helpers;
using XPloit.Sniffer.Enums;

namespace XPloit.Sniffer.Streams
{
    public class TcpStream : IEnumerable<TcpStreamMessage>, IDisposable
    {
        bool _IsFirst;
        TcpStreamMessage _Last;
        List<TcpStreamMessage> _InternalList = new List<TcpStreamMessage>();

        TcpStreamStack _ClientStack;
        TcpStreamStack _ServerStack;
        const int DumpStep = 16;
        byte _Writed;
        long _ClientLength ;
        long _ServerLength ;
        char[] _LastWrited = new char[DumpStep];
        bool _IsClossed = false;
        string _Key;
        ushort _DestinationPort, _SourcePort;
        IPAddress _DestinationAddress, _SourceAddress;

        public string Key { get { return _Key; } }
        public long ClientLength { get { return _ClientLength; } }
        public long ServerLength { get { return _ServerLength; } }
        public long Length { get { return _ClientLength + _ServerLength; } }
        public int Count { get { return _InternalList.Count; } }
        public TcpStreamMessage LastStream { get { return _Last; } }
        public TcpStreamMessage FirstStream { get { return _InternalList.Count == 0 ? null : _InternalList[0]; } }
        public bool IsClossed { get { return _IsClossed; } }
        public ushort DestinationPort { get { return _DestinationPort; } }
        public ushort SourcePort { get { return _SourcePort; } }
        public IPAddress SourceAddress { get { return _SourceAddress; } }
        public IPAddress DestinationAddress { get { return _DestinationAddress; } }
        public IPEndPoint Source { get { return new IPEndPoint(_SourceAddress, _SourcePort); } }
        public IPEndPoint Destination { get { return new IPEndPoint(_DestinationAddress, _DestinationPort); } }
        /// <summary>
        /// Variables
        /// </summary>
        public dynamic Variables { get; set; }

        public TcpStreamMessage this[int index]
        {
            get
            {
                if (_InternalList.Count <= index) return null;
                return _InternalList[index];
            }
        }
        void Add(ETcpEmisor emisor, TcpPacket tcp)
        {
            TcpStreamStack stack = null;
            switch (emisor)
            {
                case ETcpEmisor.Client:
                    {
                        if (_ClientStack == null)
                        {
                            _ClientStack = new TcpStreamStack((uint)(tcp.SequenceNumber + tcp.PayloadData.Length + (tcp.Syn ? 1 : 0)));
                            AppendPacket(_ClientStack, emisor, tcp);
                            return;
                        }
                        stack = _ClientStack;
                        break;
                    }
                case ETcpEmisor.Server:
                    {
                        if (_ServerStack == null)
                        {
                            _ServerStack = new TcpStreamStack((uint)(tcp.SequenceNumber + tcp.PayloadData.Length + (tcp.Syn && tcp.Ack ? 1 : 0)));
                            AppendPacket(_ServerStack, emisor, tcp);
                            return;
                        }
                        stack = _ServerStack;
                        break;
                    }
            }

            if (stack.SequenceNumber == tcp.SequenceNumber)
                AppendPacket(stack, emisor, tcp);
            else stack.Append(tcp.SequenceNumber, tcp);

            // Try process
            while (stack.TryGetNextPacket(out tcp))
                AppendPacket(stack, emisor, tcp);
        }
        void AppendPacket(TcpStreamStack stack, ETcpEmisor emisor, TcpPacket tcp)
        {
            if (tcp.Fin || tcp.Rst)
                _IsClossed = true;

            uint l = (uint)tcp.PayloadData.Length;
            if (l <= 0) return;
            stack.SequenceNumber += l;

            if (_Last == null)
            {
                _Last = new TcpStreamMessage(tcp.PayloadData, emisor, null);
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
                    _Last = new TcpStreamMessage(tcp.PayloadData, emisor, _Last);
                    _InternalList.Add(_Last);
                }
            }

            if (emisor == ETcpEmisor.Client) _ClientLength += l;
            else _ServerLength += l;
        }
        public static TcpStream GetStream(Dictionary<string, TcpStream> streams, IpPacket ip, TcpPacket tcp, bool startTcpStreamOnlyWithSync, out bool isNew)
        {
            string key = GetKey(ip, tcp, false);
            TcpStream ret;
            ETcpEmisor em;
            if (!streams.TryGetValue(key, out ret))
            {
                key = GetKey(ip, tcp, true);
                if (!streams.TryGetValue(key, out ret))
                {
                    // No data or no Sync
                    if ((startTcpStreamOnlyWithSync && !tcp.Syn) || tcp.Rst)
                    {
                        isNew = false;
                        return null;
                    }

                    isNew = true;
                    return new TcpStream(tcp.Syn ? ETcpEmisor.Client : ETcpEmisor.Server, ip, tcp);
                }
                else em = ETcpEmisor.Server;
            }
            else em = ETcpEmisor.Client;

            isNew = false;

            if (!ret.IsClossed) ret.Add(em, tcp);
            return ret;
        }
        static string GetKey(IpPacket ip, TcpPacket tcp, bool reverse)
        {
            if (reverse) return ip.DestinationAddress.ToString() + ":" + tcp.DestinationPort.ToString() + ">" + ip.SourceAddress.ToString() + ":" + tcp.SourcePort.ToString();
            return ip.SourceAddress.ToString() + ":" + tcp.SourcePort.ToString() + ">" + ip.DestinationAddress.ToString() + ":" + tcp.DestinationPort.ToString();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">Ip</param>
        /// <param name="tcp">Packet</param>
        public TcpStream(ETcpEmisor emisor, IpPacket ip, TcpPacket tcp)
        {
            _IsFirst = true;
            _Key = GetKey(ip, tcp, false);
            if (ip != null && tcp != null)
            {
                _DestinationPort = tcp.DestinationPort;
                _SourcePort = tcp.SourcePort;

                _DestinationAddress = ip.DestinationAddress;
                _SourceAddress = ip.SourceAddress;

                Add(emisor, tcp);
            }
            else
            {
                _IsClossed = true;
            }
        }
        /// <summary>
        /// Load TCP Stream from WireShark TCPStreamFormat
        /// </summary>
        /// <param name="file">File</param>
        public static TcpStream FromFile(string file)
        {
            TcpStream tcp = null;

            if (!string.IsNullOrEmpty(file))
            {
                string[] sp = File.ReadAllLines(file);

                foreach (string line in sp)
                {
                    string l = line.TrimStart().Replace(":", "");

                    ETcpEmisor em = line.StartsWith("\t") ? ETcpEmisor.Server : ETcpEmisor.Client;
                    if (tcp == null)
                        tcp = new TcpStream(em, null, null);

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
                        tcp._Last = new TcpStreamMessage(data, em, null);
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
                            tcp._Last = new TcpStreamMessage(data, em, tcp._Last);
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
                    sb.Append((l.Emisor == ETcpEmisor.Client ? "\t\t" : ""));
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
        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            _IsClossed = true;
            if (_ClientStack != null) _ClientStack.Clear();
            if (_ServerStack != null) _ServerStack.Clear();
            _InternalList.Clear();
        }
        public IEnumerator<TcpStreamMessage> GetEnumerator() { return _InternalList.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return _InternalList.GetEnumerator(); }
        public override string ToString()
        {
            return Count.ToString() + (_IsClossed ? " [Clossed]" : "");
        }
    }
}