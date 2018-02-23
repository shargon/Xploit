﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Sniffer.Enums;
using XPloit.Core.Sniffer.Headers;

namespace XPloit.Core.Sniffer.Streams
{
    public class TcpStream : IEnumerable<TcpStreamMessage>
    {

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

        void Add(TcpHeader packet)
        {
            if (packet.Flags.HasFlag(ETcpFlags.Fin))
                _IsClossed = true;

            if (packet.Data.Length <= 0) return;

            ETcpEmisor emisor =
                (packet.DestinationPort == _DestinationPort && IPAddress.Equals(packet.IpHeader.DestinationAddress, _DestinationAddress) &&
                packet.SourcePort == _SourcePort && IPAddress.Equals(packet.IpHeader.SourceAddress, _SourceAddress)) ? ETcpEmisor.A : ETcpEmisor.B;

            if (_Last == null)
            {
                _Last = new TcpStreamMessage(packet.Data, emisor);
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
                    _Last = new TcpStreamMessage(packet.Data, emisor);
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
            TcpStream tcp = new TcpStream(null);

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

            int ld = l.Data.Length;
            int va = l._LastRead;
            if (ld <= va) return;

            StringBuilder sb = new StringBuilder();
            for (int x = va; x < ld; x++)
            {
                if (va + x % 16 == 0)
                {
                    // comienzo
                    if (Count != 1 || x != 0)
                    {
                        // No esta empezando
                        sb.AppendLine();
                    }

                    sb.Append((l.Emisor == ETcpEmisor.A ? "    " : ""));
                    sb.Append(x.ToString("x2").PadLeft(8, '0') + "  ");
                }
                else
                {
                    sb.Append(" ");
                }

                sb.Append(l.Data[l._LastRead + x].ToString("x2"));
            }

            File.AppendAllText(file, sb.ToString());
            l._LastRead = ld;
        }
    }
}