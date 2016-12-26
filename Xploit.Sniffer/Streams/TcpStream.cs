using PacketDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Xploit.Sniffer;
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

        TcpPacketStack _ClientStack;
        TcpPacketStack _ServerStack;
        const int DumpStep = 16;
        byte _Writed;
        long _ClientLength;
        long _ServerLength;
        char[] _LastWrited = new char[DumpStep];
        bool _IsClossed;
        string _Key;
        IPEndPoint _Destination, _Source;
        DateTime _StartDate;
        TcpStreamStack _TcpStack;

        public DateTime StartDate { get { return _StartDate; } }
        public string Key { get { return _Key; } }
        public long ClientLength { get { return _ClientLength; } }
        public long ServerLength { get { return _ServerLength; } }
        public long Length { get { return _ClientLength + _ServerLength; } }
        public int Count { get { return _InternalList.Count; } }
        public TcpStreamMessage LastStream { get { return _Last; } }
        public TcpStreamMessage FirstStream { get { return _InternalList.Count == 0 ? null : _InternalList[0]; } }
        public bool IsClossed { get { return _IsClossed; } }
        public IPEndPoint Source { get { return _Source; } }
        public IPEndPoint Destination { get { return _Destination; } }
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
        public void Add(DateTime date, ETcpEmisor emisor, TcpPacket tcp)
        {
            uint sequenceNumber = tcp.SequenceNumber;

            TcpPacketStack stack = null;
            switch (emisor)
            {
                case ETcpEmisor.Client:
                    {
                        if (_ClientStack == null)
                        {
                            _ClientStack = new TcpPacketStack((uint)(sequenceNumber + tcp.PayloadData.Length + (tcp.Syn ? 1 : 0)));
                            AppendPacket(date, _ClientStack, emisor, tcp);
                            return;
                        }
                        stack = _ClientStack;
                        break;
                    }
                case ETcpEmisor.Server:
                    {
                        if (_ServerStack == null)
                        {
                            _ServerStack = new TcpPacketStack((uint)(sequenceNumber + tcp.PayloadData.Length + (tcp.Syn && tcp.Ack ? 1 : 0)));
                            AppendPacket(date, _ServerStack, emisor, tcp);
                            return;
                        }
                        stack = _ServerStack;
                        break;
                    }
            }

            if (stack.SequenceNumber == sequenceNumber)
                AppendPacket(date, stack, emisor, tcp);
            else stack.Append(sequenceNumber, tcp);

            // Try process
            while (stack.TryGetNextPacket(out tcp))
                AppendPacket(date, stack, emisor, tcp);
        }
        void AppendPacket(DateTime date, TcpPacketStack stack, ETcpEmisor emisor, TcpPacket tcp)
        {
            if (tcp.Fin || tcp.Rst)
                Close();

            byte[] payload = tcp.PayloadData;
            if (payload == null) return;

            uint l = (uint)payload.Length;
            if (l <= 0) return;
            stack.SequenceNumber += l;

            if (_Last == null)
            {
                _Last = new TcpStreamMessage(date, payload, emisor, null);
                _InternalList.Add(_Last);
            }
            else
            {
                // Check if its the same
                if (_Last.Emisor == emisor)
                    _Last.AddData(payload);
                else
                {
                    // New Packet
                    _Last = new TcpStreamMessage(date, payload, emisor, _Last);
                    _InternalList.Add(_Last);
                }
            }

            if (emisor == ETcpEmisor.Client) _ClientLength += l;
            else _ServerLength += l;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emisor">Emisor</param>
        /// <param name="source">Ip Source</param>
        /// <param name="dest">Ip Dest</param>
        /// <param name="tcp">Packet</param>
        /// <param name="date">Date</param>
        public TcpStream(TcpStreamStack stack, ETcpEmisor emisor, IPEndPoint source, IPEndPoint dest, TcpPacket tcp, DateTime date)
        {
            _TcpStack = stack;
            _IsFirst = true;
            _StartDate = date;
            _Key = TcpStreamStack.GetKey(source, dest, emisor == ETcpEmisor.Server);
            if (tcp != null)
            {
                if (emisor == ETcpEmisor.Server)
                {
                    _Destination = source;
                    _Source = dest;
                }
                else
                {
                    _Source = source;
                    _Destination = dest;
                }

                Add(date, emisor, tcp);
            }
            else Close();
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

                IPEndPoint empty = new IPEndPoint(IPAddress.None, 0);

                foreach (string line in sp)
                {
                    string l = line.TrimStart().Replace(":", "");

                    ETcpEmisor em = line.StartsWith("\t") ? ETcpEmisor.Server : ETcpEmisor.Client;
                    if (tcp == null)
                        tcp = new TcpStream(null, em, empty, empty, null, DateTime.Now);

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
                        tcp._Last = new TcpStreamMessage(DateTime.Now, data, em, null);
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
                            tcp._Last = new TcpStreamMessage(DateTime.Now, data, em, tcp._Last);
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
        public void Close()
        {
            _IsClossed = true;
            if (_TcpStack != null) _TcpStack.Remove(_Key);
            if (_ClientStack != null) _ClientStack.Clear();
            if (_ServerStack != null) _ServerStack.Clear();
        }
        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            Close();
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