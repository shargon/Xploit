using System;
using System.Text;
using XPloit.Core.Sniffer.Enums;

namespace XPloit.Core.Sniffer.Streams
{
    public class TcpStreamMessage
    {
        ETcpEmisor _Emisor;
        byte[] _Data;
        internal int _LastRead = 0;
<<<<<<< HEAD:XPloit.Core/Sniffer/Streams/TcpStreamMessage.cs
=======
        TcpStreamMessage _Previous, _Next;
>>>>>>> parent of d3eaeb5... SharpPcap:Xploit.Sniffer/Streams/TcpStreamMessage.cs

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data { get { return _Data; } }
        /// <summary>
<<<<<<< HEAD:XPloit.Core/Sniffer/Streams/TcpStreamMessage.cs
=======
        /// Previous Stream
        /// </summary>
        public TcpStreamMessage Previous { get { return _Previous; } }
        /// <summary>
        /// Next
        /// </summary>
        public TcpStreamMessage Next { get { return _Next; } }
        /// <summary>
        /// Ascii-Data
        /// </summary>
        public string DataAscii { get { return Encoding.ASCII.GetString(_Data); } }
        /// <summary>
>>>>>>> parent of d3eaeb5... SharpPcap:Xploit.Sniffer/Streams/TcpStreamMessage.cs
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
        public ETcpEmisor Emisor { get { return _Emisor; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="emisor">Emisor</param>
<<<<<<< HEAD:XPloit.Core/Sniffer/Streams/TcpStreamMessage.cs
        public TcpStreamMessage(byte[] data, ETcpEmisor emisor)
=======
        public TcpStreamMessage(byte[] data, ETcpEmisor emisor, TcpStreamMessage previous)
>>>>>>> parent of d3eaeb5... SharpPcap:Xploit.Sniffer/Streams/TcpStreamMessage.cs
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

<<<<<<< HEAD:XPloit.Core/Sniffer/Streams/TcpStreamMessage.cs
        public override string ToString() { return Emisor.ToString() + " (" + _Data.Length.ToString() + ")"; }
=======
        public override string ToString() { return Emisor.ToString() + " (" + _Data.Length.ToString() + ") " + DataAscii; }
>>>>>>> parent of d3eaeb5... SharpPcap:Xploit.Sniffer/Streams/TcpStreamMessage.cs
    }
}