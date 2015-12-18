using XPloit.Core.Sockets.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XPloit.Core.Multi;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets
{
    public class XPloitSocketClient : IDisposable
    {
        #region Variables
        static int _dfbl = ushort.MaxValue; //512 * 1024;
        const int ASYNC_READ_BUFFER_SIZE = 150 * 1024;
        object _Tag = null;
        IPEndPoint _IPEndPoint = null;
        Socket _Socket = null;
        Stream _Stream = null;
        XPloitSocket _Parent = null;
        bool _HasTimeOut = true;
        byte[] _ALL_BUFFER = new byte[] { };
        byte[] _ASYNC_READ_BUFFER = null;
        //object read_lock = new object();
        object write_lock = new object();

        DateTime _LastRead = DateTime.Now;
        DateTime _lchk_status = DateTime.Now.AddDays(-1);
        EndPoint _LocalEndPoint = null, _RemoteEndPoint = null;
        Dictionary<string, object> var = null;
        EDissconnectReason _DisconnectReason = EDissconnectReason.None;

        ulong _BytesWritten = 0, _BytesReaded = 0, _MsgSend = 0, _MsgReceived = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Número de bytes escritos
        /// </summary>
        public ulong BytesWritten { get { return _BytesWritten; } }
        /// <summary>
        /// Número de bytes leidos
        /// </summary>
        public ulong BytesReaded { get { return _BytesReaded; } }
        /// <summary>
        /// Número de mensajes enviados
        /// </summary>
        public ulong MsgSend { get { return _MsgSend; } }
        /// <summary>
        /// Número de mensajes recibidos
        /// </summary>
        public ulong MsgReceived { get { return _MsgReceived; } }
        /// <summary>
        /// LocalEndPoint del cliente
        /// </summary>
        public EndPoint LocalEndPoint { get { return _LocalEndPoint; } }
        /// <summary>
        /// RemoteEndPoint del cliente
        /// </summary>
        public EndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }
        /// <summary>
        /// Número máximo de bytes por segundo
        /// </summary>
        public long MaximumBytesPerSecond
        {
            get
            {
                if (_Stream != null && _Stream is StreamSpeedLimit)
                    return ((StreamSpeedLimit)_Stream).MaximumBytesPerSecond;
                return StreamSpeedLimit.Infinite;
            }
            set
            {
                if (_Stream != null && _Stream is StreamSpeedLimit)
                    ((StreamSpeedLimit)_Stream).MaximumBytesPerSecond = value;
            }
        }
        /// <summary>
        /// Tiene o no Timeout
        /// </summary>
        public bool HasTimeOut { get { return _HasTimeOut; } set { _HasTimeOut = value; } }
        /// <summary>
        /// ültima lectura
        /// </summary>
        public DateTime LastRead { get { return _LastRead; } }
        /// <summary>
        /// Razon de desconexión
        /// </summary>
        public EDissconnectReason DisconnectReason { get { return _DisconnectReason; } }
        /// <summary>
        /// Tag
        /// </summary>
        public object Tag { get { return _Tag; } set { _Tag = value; } }
        /// <summary>
        /// Padre del cliente
        /// </summary>
        public XPloitSocket Parent { get { return _Parent; } }
        /// <summary>
        /// IPEndPoint del cliente
        /// </summary>
        public IPEndPoint IPEndPoint { get { return _IPEndPoint; } }
        /// <summary>
        /// Dirección IP del cliente
        /// </summary>
        public IPAddress IPAddress { get { return _IPEndPoint == null ? null : _IPEndPoint.Address; } }
        /// <summary>
        /// Dirección IP del cliente en String
        /// </summary>
        public string IPString { get { return _IPEndPoint == null ? null : _IPEndPoint.Address.ToString(); } }
        /// <summary>
        /// Devuelve si está conectado
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_Socket == null) return false;

                bool dv = false;
                lock (_Socket)
                {
                    DateTime now = DateTime.Now;
                    double sec = (now - _lchk_status).TotalSeconds;
                    if (sec > 0 && sec < 0.4) return true;
                    _lchk_status = now;

                    try
                    {
                        dv = !(_Socket.Poll(1000, SelectMode.SelectRead) && _Socket.Available <= 0);
                        if (!dv && _Socket.Connected)
                        {
                            Thread.Sleep(10);
                            dv = !(_Socket.Poll(1, SelectMode.SelectRead) && _Socket.Available <= 0);
                        }
                    }
                    catch { Disconnect(EDissconnectReason.Error); dv = false; }
                }
                return dv;
            }
        }
        /// <summary>
        /// Devuelve el número de bytes disponibles para lectura
        /// </summary>
        internal int Available
        {
            get
            {
                if (_Socket != null)
                {
                    try { return _Socket.Available; }
                    catch { Disconnect(EDissconnectReason.Error); }
                }
                return -1;
            }
        }
        /// <summary>
        /// Devuelve si tiene o no una variable
        /// </summary>
        /// <param name="name">Nombre de la variable</param>
        /// <returns>Devuelve true si tiene la variable</returns>
        public bool HasVariable(string name)
        {
            lock (var)
            {
                if (var.ContainsKey(name)) return true;
            }
            return false;
        }
        /// <summary>
        /// Obtiene una variable del cliente
        /// </summary>
        /// <param name="name">Nombre de variable</param>
        /// <returns>Devuelve el objeto o NULL en su defecto</returns>
        public object this[string name]
        {
            get
            {
                lock (var)
                {
                    if (var.ContainsKey(name)) return var[name];
                }
                return null;
            }
            set
            {
                lock (var)
                {
                    if (var.ContainsKey(name))
                    {
                        if (value == null) var.Remove(name); else var[name] = value;
                    }
                    else var.Add(name, value);
                }
            }
        }
        /// <summary>
        /// Tamaño por defecto para el buffer
        /// </summary>
        public static int DefaultBufferLength { get { return _dfbl; } set { _dfbl = value; } }
        #endregion

        internal XPloitSocketClient(XPloitSocket parent, Socket tc, bool speedLimit)
        {
            _Parent = parent;

            _LocalEndPoint = tc.LocalEndPoint;
            _RemoteEndPoint = tc.RemoteEndPoint;

            tc.NoDelay = true;
            tc.Blocking = true;
            tc.DontFragment = true;
            tc.SendTimeout = 0;
            tc.ReceiveBufferSize = 0;
            tc.SendBufferSize = 0;
            tc.ReceiveTimeout = 0;

            try
            {
                tc.SendBufferSize = _dfbl;
                tc.ReceiveBufferSize = _dfbl;

                if (speedLimit) _Stream = new StreamSpeedLimit(new NetworkStream(tc), StreamSpeedLimit.Infinite);
                else _Stream = new NetworkStream(tc);

                _IPEndPoint = ((IPEndPoint)tc.RemoteEndPoint);
            }
            catch { }

            _Socket = tc;
            var = new Dictionary<string, object>();
        }
        /// <summary>
        /// Lee del socket y lo suma al buffer
        /// </summary>
        /// <param name="length">Tamaño a leer</param>
        /// <returns>Bytes leidos</returns>
        internal int ReadFronSocket(XPloitSocketClient c)
        {
            if (_Stream == null) return 0;

            int dv = 0;
            //lock (read_lock)
            //{
            int length = 0;
            if ((length = c.Available) > 0)
            {
                int ix = _ALL_BUFFER.Length;
                Array.Resize(ref _ALL_BUFFER, _ALL_BUFFER.Length + length);

                int le = 0;
                do
                {
                    ix += le;

                    le = _Stream.Read(_ALL_BUFFER, ix, length);

                    dv += le;
                    length -= le;

                } while (length > 0);
            }
            if (dv <= 0) return 0;

            foreach (IXPloitSocketMsg msg in _Parent.Protocol.ProcessBuffer(this, ref _ALL_BUFFER))
                if (msg != null) RaiseOnMessage(msg);
            //}
            if (dv > 0)
            {
                _LastRead = DateTime.Now;
                _BytesReaded += (ulong)dv;
            }
            return dv;
        }
        /// <summary>
        /// Añade al buffer los bytes proporcionados
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="index">indice</param>
        /// <param name="length">Tamaño</param>
        /// <returns></returns>
        internal int ReadFromData(byte[] data, int index, int length)
        {
            //lock (read_lock)
            //{
            int ix = _ALL_BUFFER.Length;
            Array.Resize(ref _ALL_BUFFER, ix + length);
            Array.Copy(data, 0, _ALL_BUFFER, ix, length);

            foreach (IXPloitSocketMsg msg in _Parent.Protocol.ProcessBuffer(this, ref _ALL_BUFFER))
                if (msg != null) RaiseOnMessage(msg);
            //}

            _LastRead = DateTime.Now;
            _BytesReaded += (ulong)length;
            return length;
        }
        /// <summary>
        /// Escribe en el cliente
        /// </summary>
        /// <param name="bff">array de bytes</param>
        /// <param name="index">Indice en el buffer de datos</param>
        /// <param name="length">Tamaño de lectura</param>
        /// <param name="msgSendIncrement">Incrementamos en el contador de mensajes enviados</param>
        /// <returns>Devuelve si ha escrito o no</returns>
        internal bool Write(byte[] bff, int index, int length, bool msgSendIncrement)
        {
            try
            {
                if (_Stream != null)
                {
                    lock (write_lock)
                    {
                        _Stream.Write(bff, index, length);
                        //_Stream.Flush();
                    }
                    _BytesWritten += (ulong)length;
                    if (msgSendIncrement) _MsgSend++;
                    return true;
                }
            }
            catch { Disconnect(EDissconnectReason.Error); }
            return false;
        }
        /// <summary>
        /// Realiza la desconexión del cliente
        /// </summary>
        /// <param name="dr">Tipo de desconexión</param>
        public void Disconnect(EDissconnectReason dr)
        {
            bool disok = false;
            if (_Socket != null)
            {
                lock (_Socket)
                {
                    try
                    {
                        _Socket.Close();
                        _Socket.Dispose();
                    }
                    catch { }
                    _Socket = null;
                    disok = true;
                }
            }

            if (_Stream != null)
            {
                try { _Stream.Dispose(); }
                catch { }
                _Stream = null;
            }

            if (disok)
            {
                _DisconnectReason = dr;
                _ALL_BUFFER = new byte[] { };
                _ASYNC_READ_BUFFER = null;
                _Parent.RaiseOnDisconnect(this, dr);
            }
        }
        /// <summary>
        /// Ejecuta el envio del mensaje para el cliente en cuestión
        /// </summary>
        /// <param name="msg">Mensaje a enviar</param>
        public void RaiseOnMessage(IXPloitSocketMsg msg)
        {
            if (_Parent == null) return;

            _Parent.RaiseOnMessage(this, msg);
            _MsgReceived++;
        }
        /// <summary>
        /// Envia los mensajes al cliente en cuestión
        /// </summary>
        /// <param name="msg">Mensajes a enviar</param>
        /// <returns>Devuelve el número de bytes enviados</returns>
        public int Send(params IXPloitSocketMsg[] msg)
        {
            if (_Parent == null || msg == null) return 0;

            int ret = 0;
            foreach (IXPloitSocketMsg mx in msg)
                ret += _Parent.Protocol.Send(this, mx);

            return ret;
        }

        #region LECTURA ASINCRONA
        internal void BeginReceive()
        {
            try
            {
                _ASYNC_READ_BUFFER = new byte[XPloitSocketClient.ASYNC_READ_BUFFER_SIZE];
                _Stream.BeginRead(_ASYNC_READ_BUFFER, 0, XPloitSocketClient.ASYNC_READ_BUFFER_SIZE, new AsyncCallback(Read_Callback), _Stream);
            }
            catch { Disconnect(EDissconnectReason.Error); }
        }
        void Read_Callback(IAsyncResult ar)
        {
            try
            {
                Stream st = (Stream)ar.AsyncState;

                int read = st.EndRead(ar);
                if (read > 0) ReadFromData(_ASYNC_READ_BUFFER, 0, read);

                _Stream.BeginRead(_ASYNC_READ_BUFFER, 0, XPloitSocketClient.ASYNC_READ_BUFFER_SIZE, new AsyncCallback(Read_Callback), _Stream);
            }
            catch { Disconnect(EDissconnectReason.Error); }
        }
        #endregion

        /// <summary>
        /// Liberación de recursos
        /// </summary>
        public void Dispose() { Disconnect(EDissconnectReason.TimeOut); }
        /// <summary>
        /// Método para el debug
        /// </summary>
        /// <returns>Devuelve la Ip en texto del cliente</returns>
        public override string ToString() { return IPString; }
    }
}