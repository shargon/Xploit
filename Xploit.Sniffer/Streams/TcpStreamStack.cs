using PacketDotNet;
using System.Collections.Generic;
using System.Net;
using XPloit.Sniffer.Enums;
using XPloit.Sniffer.Streams;
using Xploit.Sniffer.Enums;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace Xploit.Sniffer.Streams
{
    public class TcpStreamStack
    {
        Dictionary<string, TcpStream> _TcpStreams = new Dictionary<string, TcpStream>();

        int _Dropped;
        public int Dropped { get { return _Dropped; } }
        public TimeSpan TimeoutSync { get; set; }
        public TimeSpan Timeout { get; set; }

        public TcpStreamStack()
        {
            TimeoutSync = TimeSpan.FromSeconds(20);
            Timeout = TimeSpan.FromMinutes(15);
        }

        public static string GetKey(IPEndPoint source, IPEndPoint dest, bool reverse)
        {
            if (reverse) return dest.ToString() + ">" + source.ToString();
            return source.ToString() + ">" + dest.ToString();
        }
        public bool TryGetValue(IPEndPoint source, IPEndPoint dest, out TcpStream ret, out ETcpEmisor em)
        {
            em = dest.Port < 49152 ? ETcpEmisor.Client : ETcpEmisor.Server;

            if (!_TcpStreams.TryGetValue(GetKey(source, dest, em == ETcpEmisor.Server), out ret))
            {
                em = em == ETcpEmisor.Client ? ETcpEmisor.Server : ETcpEmisor.Client;

                if (!_TcpStreams.TryGetValue(GetKey(source, dest, em == ETcpEmisor.Server), out ret))
                {
                    em = ETcpEmisor.Client;
                    return false;
                }
            }

            return true;
        }
        internal bool Remove(string key)
        {
            lock (_TcpStreams) return _TcpStreams.Remove(key);
        }
        public bool GetStream(PhysicalAddress hwSource, PhysicalAddress hwDest, IPEndPoint ipSource, IPEndPoint ipDest, TcpPacket tcp, DateTime date, EStartTcpStreamMethod startTcpStreamMethod, out TcpStream stream)
        {
            ETcpEmisor em;
            if (!TryGetValue(ipSource, ipDest, out stream, out em))
            {
                bool syn = tcp.Syn;
                bool ack = tcp.Ack;

                // No data or no Sync
                switch (startTcpStreamMethod)
                {
                    case EStartTcpStreamMethod.Sync:
                        {
                            if (!syn || ack)
                            {
                                stream = null;
                                return false;
                            }
                            break;
                        }
                    case EStartTcpStreamMethod.SyncAck:
                        {
                            if (!syn || !ack)
                            {
                                stream = null;
                                return false;
                            }
                            break;
                        }
                }

                lock (_TcpStreams)
                {
                    stream = new TcpStream(this, syn && ack ? ETcpEmisor.Server : ETcpEmisor.Client, hwSource, hwDest, ipSource, ipDest, tcp, date);
                    _TcpStreams.Add(stream.Key, stream);
                }
                return true;
            }

            if (!stream.IsClossed)
                stream.Add(date, em, tcp);

            return false;
        }
        internal IEnumerable<TcpStream> CleanByTimeout(DateTime date)
        {
            TcpStream[] ret;
            lock (_TcpStreams) { ret = _TcpStreams.Values.ToArray(); }

            foreach (TcpStream v in ret)
            {
                if (v == null) continue;

                lock (v) if (!v.IsClossed && !IsTimeouted(v, date))
                    {
                        v.Close();
                        _Dropped++;
                        yield return v;
                    }
            }
        }
        bool IsTimeouted(TcpStream v, DateTime date)
        {
            TcpStreamMessage st = v.LastStream;

            if (st == null)
                return date < v.StartDate.Add(TimeoutSync);

            return date < st.Date.Add(Timeout);
        }
    }
}