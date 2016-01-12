using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XPloit.Core.Streams;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets
{
    public class XPloitSocketClient : IDisposable
    {
        #region Variables
        static int _dfbl = ushort.MaxValue; //512 * 1024;
        object _Tag;
        IPEndPoint _IPEndPoint;
        Socket _Socket;
        Stream _Stream;
        IXPloitSocketProtocol _Protocol;
        bool _HasTimeOut = true;

        DateTime _LastRead = DateTime.Now;
        DateTime _lchk_status = DateTime.Now.AddDays(-1);
        EndPoint _LocalEndPoint, _RemoteEndPoint;
        Dictionary<string, object> _Variables = null;
        EDissconnectReason _DisconnectReason = EDissconnectReason.None;
        Dictionary<Guid, IXPloitSocketMsg> _Actions = new Dictionary<Guid, IXPloitSocketMsg>();

        ulong _MsgSend = 0, _MsgReceived = 0;

        public delegate void delOnDisconnect(XPloitSocketClient sender, EDissconnectReason e);
        public delegate void delOnMessage(XPloitSocketClient sender, IXPloitSocketMsg msg);

        public event delOnDisconnect OnDisconnect;
        public event delOnMessage OnMessage;
        #endregion

        #region Properties
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
        /// Protocol
        /// </summary>
        public IXPloitSocketProtocol Protocol { get { return _Protocol; } }
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
        /// Devuelve si tiene o no una variable
        /// </summary>
        /// <param name="name">Nombre de la variable</param>
        /// <returns>Devuelve true si tiene la variable</returns>
        public bool HasVariable(string name)
        {
            lock (_Variables)
            {
                if (_Variables.ContainsKey(name)) return true;
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
                lock (_Variables)
                {
                    if (_Variables.ContainsKey(name)) return _Variables[name];
                }
                return null;
            }
            set
            {
                lock (_Variables)
                {
                    if (_Variables.ContainsKey(name))
                    {
                        if (value == null) _Variables.Remove(name); else _Variables[name] = value;
                    }
                    else _Variables.Add(name, value);
                }
            }
        }
        /// <summary>
        /// Tamaño por defecto para el buffer
        /// </summary>
        public static int DefaultBufferLength { get { return _dfbl; } set { _dfbl = value; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocol">Protocol</param>
        /// <param name="socket">Socket</param>
        /// <param name="speedLimit">Use speedLimit</param>
        public XPloitSocketClient(IXPloitSocketProtocol protocol, Socket socket, bool speedLimit)
        {
            _Protocol = protocol;

            _LocalEndPoint = socket.LocalEndPoint;
            _RemoteEndPoint = socket.RemoteEndPoint;

            socket.NoDelay = true;
            socket.Blocking = true;
            socket.DontFragment = true;
            socket.SendTimeout = 0;
            socket.ReceiveBufferSize = 0;
            socket.SendBufferSize = 0;
            socket.ReceiveTimeout = 0;

            try
            {
                socket.SendBufferSize = _dfbl;
                socket.ReceiveBufferSize = _dfbl;

                if (speedLimit) _Stream = new StreamSpeedLimit(new NetworkStream(socket), StreamSpeedLimit.Infinite);
                else _Stream = new NetworkStream(socket);

                _IPEndPoint = ((IPEndPoint)socket.RemoteEndPoint);
            }
            catch { }

            _Socket = socket;
            _Variables = new Dictionary<string, object>();
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
                if (OnDisconnect != null)
                    OnDisconnect(this, dr);
            }
        }
        /// <summary>
        /// Envia los mensajes al cliente en cuestión
        /// </summary>
        /// <param name="msg">Mensajes a enviar</param>
        /// <returns>Devuelve el número de bytes enviados</returns>
        public int Send(params IXPloitSocketMsg[] msg)
        {
            if (_Protocol == null || msg == null) return 0;

            int ret = 0;
            try
            {
                foreach (IXPloitSocketMsg mx in msg)
                {
                    ret += _Protocol.Send(mx, _Stream);
                    _MsgSend++;
                }
                _Stream.Flush();
            }
            catch { Disconnect(EDissconnectReason.Error); }

            return ret;
        }
        /// <summary>
        /// Envia el mensaje y devuelve una respuesta, Este método nunca hay que llamarlo desde el hilo del Socket
        /// </summary>
        /// <param name="msg">Mensaje</param>
        public IXPloitSocketMsg SendAndWait(IXPloitSocketMsg msg)
        {
            Guid wait = msg.Id;
            if (Send(msg) <= 0) return null;

            IXPloitSocketMsg msgRet;
            while (!_Actions.TryGetValue(wait, out msgRet))
            {
                //Read();
                Thread.Sleep(0);
            }
            return msgRet;
        }
        /// <summary>
        /// Lee el mensaje o devuelve null si no hay
        /// </summary>
        public IXPloitSocketMsg Read()
        {
            if (_Stream == null) return null;

            try
            {
                if (_Socket.Available <= 0) return null;

                IXPloitSocketMsg msg = _Protocol.Read(_Stream);
                _LastRead = DateTime.Now;

                if (msg == null) return null;
                _MsgReceived++;

                if (!Guid.Equals(msg.InResponseTo, Guid.Empty))
                {
                    _Actions.Add(msg.InResponseTo, msg);
                    return null;
                }

                if (OnMessage != null)
                    OnMessage(this, msg);

                return msg;
            }
            catch
            {
                Disconnect(EDissconnectReason.Error);
                return null;
            }
        }
        /// <summary>
        /// Lee el mensaje
        /// </summary>
        public IXPloitSocketMsg ReadOrWait()
        {
            IXPloitSocketMsg msg;

            do msg = Read();
            while (msg == null);

            return msg;
        }
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