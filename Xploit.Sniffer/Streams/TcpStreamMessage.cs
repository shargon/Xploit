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
        public ETcpEmisor Emisor { get { return _Emisor; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="emisor">Emisor</param>
        public TcpStreamMessage(byte[] data, ETcpEmisor emisor)
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
}