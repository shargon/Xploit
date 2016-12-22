using System;
using System.Text;
using XPloit.Sniffer.Enums;

namespace XPloit.Sniffer.Streams
{
    public class TcpStreamMessage
    {
        ETcpEmisor _Emisor;
        byte[] _Data;
        internal int _LastRead = 0;
        TcpStreamMessage _Previous, _Next;

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data { get { return _Data; } }
        /// <summary>
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
        /// UTF8-Data
        /// </summary>
        public string DataUTF8 { get { return Encoding.UTF8.GetString(_Data); } }
        /// <summary>
        /// Unicode-Data
        /// </summary>
        public string DataUnicode { get { return Encoding.Unicode.GetString(_Data); } }
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
        public TcpStreamMessage(byte[] data, ETcpEmisor emisor, TcpStreamMessage previous)
        {
            _Data = data;
            _Emisor = emisor;

            if (previous != null)
            {
                _Previous = previous;
                previous._Next = this;
            }
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

        public override string ToString() { return Emisor.ToString() + " (" + _Data.Length.ToString() + ") " + DataAscii; }
    }
}