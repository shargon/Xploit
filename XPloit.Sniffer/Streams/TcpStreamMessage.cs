using System;
using System.Text;
using XPloit.Sniffer.Enums;

namespace XPloit.Sniffer.Streams
{
    public class TcpStreamMessage : IDisposable
    {
        //const int incBuffer = 1024;

        byte[] _Data;
        ETcpEmisor _Emisor;
        int _DataLength = 0, _DataCapacity = 0;

        //MemoryStream _Stream;

        DateTime _Date;
        internal int _LastRead = 0;
        TcpStreamMessage _Previous, _Next;

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _Data;
                //return _Stream.ToArray();
            }
        }
        /// <summary>
        /// Data Length
        /// </summary>
        public int DataLength { get { return _DataLength; } }
        /// <summary>
        /// Previous Stream
        /// </summary>
        public TcpStreamMessage Previous { get { return _Previous; } }
        /// <summary>
        /// Next
        /// </summary>
        public TcpStreamMessage Next { get { return _Next; } }
        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime Date { get { return _Date; } }
        /// <summary>
        /// Ascii-Data
        /// </summary>
        public string DataAscii { get { return GetString(Encoding.ASCII); } }
        /// <summary>
        /// UTF8-Data
        /// </summary>
        public string DataUTF8 { get { return GetString(Encoding.UTF8); } }
        /// <summary>
        /// Unicode-Data
        /// </summary>
        public string DataUnicode { get { return GetString(Encoding.Unicode); } }
        /// <summary>
        /// HEX-Data
        /// </summary>
        public string DataHex
        {
            get
            {
                byte[] data = Data;
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < _DataLength; x++)
                {
                    sb.Append(data[x].ToString("x2"));
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
        /// <param name="date">Date</param>
        /// <param name="data">Data</param>
        /// <param name="emisor">Emisor</param>
        /// <param name="previous">Previous</param>
        public TcpStreamMessage(DateTime date, byte[] data, ETcpEmisor emisor, TcpStreamMessage previous)
        {
            _Date = date;
            _Emisor = emisor;

            _Data = data;
            //_Stream = new MemoryStream();
            //_Stream.Write(data, 0, data.Length);
            _DataLength = data.Length;
            _DataCapacity = _DataLength;

            if (previous != null)
            {
                _Previous = previous;
                previous._Next = this;
            }
        }
        string GetString(Encoding enc)
        {
            return enc.GetString(_Data, 0, _DataLength);
        }
        /// <summary>
        /// Add data to Stream
        /// </summary>
        /// <param name="data">Data</param>
        public void AddData(byte[] data, int index, int length)
        {
            if (data == null || length <= 0)
                return;

            int value = _DataLength + length;

            if (value > _DataCapacity)
            {
                int num = value;
                if (num < 256) num = 256;
                if (num < _DataCapacity * 2)
                {
                    num = _DataCapacity * 2;
                    if (num > 2147483591) num = ((value > 2147483591) ? value : 2147483591);
                }

                byte[] n = new byte[num];
                Buffer.BlockCopy(_Data, 0, n, 0, _DataLength);
                Buffer.BlockCopy(data, 0, n, _DataLength, length);
                _Data = n;
                _DataCapacity = num;
            }
            else
                Buffer.BlockCopy(data, 0, _Data, _DataLength, length);

            //_Stream.Write(data, 0, length);

            _DataLength += length;
        }
        /// <summary>
        /// Release
        /// </summary>
        public void Dispose()
        {
            //if (_Stream == null) return;

            //_Stream.Dispose();
            //_Stream = null;
            _Data = null;
        }
        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return Emisor.ToString() + " (" + _DataLength.ToString() + ") " + DataAscii;
        }
    }
}