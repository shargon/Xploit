using PacketDotNet;
using System.Collections.Generic;
using System.Net;
using XPloit.Sniffer.Enums;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace XPloit.Sniffer.Streams
{
    public class TcpStreamStack
    {
        public class ResumePacket
        {
            public DateTime Date;
            public PhysicalAddress HwSource, HwDest;
            public IPEndPoint IpSource, IpDest;
            public TcpPacket Tcp;
        }

        Dictionary<string, TcpStream> _TcpStreams = new Dictionary<string, TcpStream>();

        public TimeSpan TimeoutSync { get; set; }
        public TimeSpan Timeout { get; set; }

        public TcpStreamStack()
        {
            TimeoutSync = TimeSpan.FromSeconds(20);
            Timeout = TimeSpan.FromMinutes(10);
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
        public bool GetStream(ResumePacket packet, EStartTcpStreamMethod startTcpStreamMethod, out TcpStream stream)
        {
            if (!TryGetValue(packet.IpSource, packet.IpDest, out stream, out var em))
            {
                var syn = packet.Tcp.Synchronize;
                var ack = packet.Tcp.Acknowledgment;

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
                    stream = new TcpStream(this, syn && ack ? ETcpEmisor.Server : ETcpEmisor.Client, packet.HwSource, packet.HwDest, packet.IpSource, packet.IpDest, packet.Tcp, packet.Date);
                    _TcpStreams.Add(stream.Key, stream);
                }
                return true;
            }

            if (!stream.IsClossed)
                stream.Add(packet.Date, em, packet.Tcp);

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