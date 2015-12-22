using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XPloit.Core.Parsers
{
    public class WireSharkTCPStreamHexDump
    {
        public class Dump
        {
            /// <summary>
            /// Parent
            /// </summary>
            public Packet Parent { get; private set; }
            /// <summary>
            /// Offset
            /// </summary>
            public long Offset { get; private set; }
            /// <summary>
            /// Index
            /// </summary>
            public int Index { get; private set; }
            /// <summary>
            /// Index
            /// </summary>
            public int Length { get; private set; }

            public Dump(Packet parent, long offset, int index, int length)
            {
                Parent = parent;
                Offset = offset;
                Index = index;
                Length = length;
            }

            public override string ToString()
            {
                return Offset.ToString("X").PadLeft(8, '0') + "  " + Encoding.ASCII.GetString(Parent.Data, Index, Length);
            }
        }
        public class Packet
        {
            byte[] _Data;
            Dump[] _Dump;
            bool _IsSend;

            /// <summary>
            /// Is Send
            /// </summary>
            public bool IsSend { get { return _IsSend; } }
            /// <summary>
            /// Is Receive?
            /// </summary>
            public bool IsReceive { get { return !_IsSend; } }
            /// <summary>
            /// Data
            /// </summary>
            public Dump[] Dump { get { return _Dump; } }
            /// <summary>
            /// Datas
            /// </summary>
            public byte[] Data { get { return _Data; } }

            public Packet(bool isSend, byte[] data, long offset, int divideLines)
            {
                _Data = data;
                _IsSend = isSend;

                List<Dump> l = new List<Dump>();

                int length = 0;
                for (int x = 0, m = data.Length; x < m; x++)
                {
                    length++;

                    if (length == divideLines || x + 1 == m)
                    {
                        l.Add(new Dump(this, offset + x, x, length));
                        length = 0;
                    }
                }

                _Dump = l.ToArray();
            }
        }

        Packet[] _All = null, _Send = null, _Receive = null;

        public Packet[] Send { get { return _Send; } }
        public Packet[] Receive { get { return _Receive; } }
        public Packet[] All { get { return _All; } }

        public static WireSharkTCPStreamHexDump FromFile(string wireSharkTCPStreamFile)
        {
            List<Packet> all = new List<Packet>();
            List<Packet> send = new List<Packet>();
            List<Packet> rec = new List<Packet>();

            using (StreamReader sr = new StreamReader(wireSharkTCPStreamFile))
            {
                string line;
                long offsetSend = 0, offsetReceive = 0;

                bool isSend = false;
                Packet p = null;

                List<byte> data = new List<byte>();
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.Trim() == string.Empty) continue;
                    if (line.Length < 60) continue;

                    bool bIsSend = line.StartsWith(" ");
                    line = line.Substring(9).Trim().Replace("  ", " ").Replace("\t", "");
                    if (line.Length < 48) continue;
                    line = line.Substring(0, 48);

                    // Parse hex chars

                    byte[] dx = new byte[] { };

                    // Read line hex

                    data.AddRange(dx);

                    if (bIsSend) offsetSend += dx.Length;
                    else offsetReceive += dx.Length;

                    // Hay cambio
                    if (bIsSend != isSend && data.Count > 0)
                    {
                        // Add to stack
                        if (isSend) { p = new Packet(true, data.ToArray(), offsetSend, 16); send.Add(p); }
                        else { p = new Packet(false, data.ToArray(), offsetReceive, 16); rec.Add(p); }

                        data.Clear();
                        all.Add(p);
                    }
                }

                if (data.Count > 0)
                {
                    // Add to stack
                    if (isSend) { p = new Packet(true, data.ToArray(), offsetSend, 16); send.Add(p); }
                    else { p = new Packet(false, data.ToArray(), offsetReceive, 16); rec.Add(p); }

                    data.Clear();
                    all.Add(p);
                }
            }


            return new WireSharkTCPStreamHexDump()
            {
                _All = all.ToArray(),
                _Receive = rec.ToArray(),
                _Send = send.ToArray()
            };
        }
    }
}
