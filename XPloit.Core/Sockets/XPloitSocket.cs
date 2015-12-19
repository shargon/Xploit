using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XPloit.Core.Helpers;
using XPloit.Core.Multi;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Exceptions;
using XPloit.Core.Sockets.Interfaces;

namespace XPloit.Core.Sockets
{
    public class XPloitSocket : IDisposable
    {
        #region VARS
        const int DEFAULT_PORT = 1024;
        Socket _Server = null;
        XPloitSocketClient _Client = null;
        int _MaxConnections = 2000;

        XPloitSocketClient[] _Clients = new XPloitSocketClient[] { }; //array por que se realizaran mas lecturas que añadir/eliminar
        bool _IsServer = false, _IsStopping = false, _AutoReconnect = false, _UseSpeedLimit = false;
        IPFilter _IPFilter = null;
        Thread _Thread = null;
        IPEndPoint _IPEndPoint = null;

        IXPloitSocketProtocol _Protocol = null;
        TimeSpan _TimeOut = new TimeSpan(0, 0, 30);
        #endregion

        #region EVENTS
        public delegate void delEventHandler(XPloitSocket sender, object args);

        public delegate bool delIsClient(XPloitSocketClient client, object[] tag);
        public delegate void delOnDisconnect(XPloitSocket sender, XPloitSocketClient cl, EDissconnectReason e);
        public delegate void delOnConnect(XPloitSocket sender, XPloitSocketClient cl);
        public delegate void delOnOverrideTimeout(XPloitSocket sender, XPloitSocketClient cl, CancelEventArgs disconnect);
        public delegate void delOnMessage(XPloitSocket sender, XPloitSocketClient cl, IXPloitSocketMsg msg);

        public event delEventHandler OnStart = null;
        public event delEventHandler OnStop = null;
        public event delOnDisconnect OnDisconnect = null;
        public event delOnConnect OnConnect = null;
        public event delOnOverrideTimeout OnOverrideTimeout = null;
        public event delOnMessage OnMessage = null;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Lista de clientes conectados
        /// </summary>
        public ReadOnlyCollection<XPloitSocketClient> Clients
        {
            get { return Array.AsReadOnly<XPloitSocketClient>(_Clients); }
        }
        /// <summary>
        /// Establece si se usa Límite de velocidad
        /// </summary>
        public bool UseSpeedLimit { get { return _UseSpeedLimit; } set { _UseSpeedLimit = value; } }
        /// <summary>
        /// Establece si se autoreconecta
        /// </summary>
        public bool AutoReconnect { get { return _AutoReconnect; } set { _AutoReconnect = value; } }
        /// <summary>
        /// Establece el Timeout entre mensajes para desconexión
        /// </summary>
        public TimeSpan TimeOut { get { return _TimeOut; } set { _TimeOut = value; } }
        /// <summary>
        /// Establece el número máximo de conexiones permitidas
        /// </summary>
        public int MaxConnections { get { return _MaxConnections; } set { _MaxConnections = value; } }
        /// <summary>
        /// Establece el número de conexiones activos
        /// </summary>
        public int Connections { get { return _Clients.Length; } }
        /// <summary>
        /// Filtros de ip
        /// </summary>
        public IPFilter IPFilter { get { return _IPFilter; } set { _IPFilter = value; } }
        /// <summary>
        /// Protocolo de uso
        /// </summary>
        public IXPloitSocketProtocol Protocol { get { return _Protocol; } }
        /// <summary>
        /// Establece si es un gestor de Servidor o de cliente
        /// </summary>
        public bool IsServer
        {
            get { return _IsServer; }
            set
            {
                if (Enable) throw (new StopFirstException());
                _IsServer = value;
            }
        }
        /// <summary>
        /// Devuelve si se está parando
        /// </summary>
        public bool IsStopping { get { return _IsStopping; } }
        /// <summary>
        /// Host de conexión
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get { return _IPEndPoint; }
            set
            {
                if (Enable) throw (new StopFirstException());
                _IPEndPoint = value;
            }
        }
        /// <summary>
        /// Ip del EndPoint
        /// </summary>
        public IPAddress IPAddress
        {
            get { return _IPEndPoint == null ? null : _IPEndPoint.Address; }
            set
            {
                if (Enable) throw (new StopFirstException());
                _IPEndPoint = new IPEndPoint(value, _IPEndPoint.Port);
            }
        }
        /// <summary>
        /// Port del EndPoint
        /// </summary>
        public int Port
        {
            get { return _IPEndPoint == null ? DEFAULT_PORT : _IPEndPoint.Port; }
            set
            {
                if (Enable) throw (new StopFirstException());
                _IPEndPoint = new IPEndPoint(_IPEndPoint.Address, DEFAULT_PORT);
            }
        }
        /// <summary>
        /// Devuelve o establece si está activo
        /// </summary>
        public bool Enable
        {
            get
            {
                if (IsServer) return !_IsStopping && _Server != null && _Server.IsBound;
                return _Client != null && _Client.IsConnected;
            }
            set
            {
                if (value == Enable) return;
                if (value) Start(); else Stop();
            }
        }
        #endregion

        #region PARADA Y INICIO DEL MANAGER
        public void Stop() { Stop(false, true); }
        public void Stop(bool reconnect) { Stop(reconnect, true); }
        public void Stop(bool reconnect, bool raiseEvent)
        {
            lock (this)
            {
                if (reconnect && !_AutoReconnect) reconnect = false;

                if (_Thread != null)
                {
                    if (raiseEvent && OnStop != null) OnStop(this, null);

                    _IsStopping = true;
                    foreach (XPloitSocketClient c in _Clients) c.Disconnect(EDissconnectReason.Requested);

                    if (_IsServer)
                    {
                        if (_Thread != null)
                        {
                            if (_Thread.IsAlive) _Thread.Join(new TimeSpan(0, 0, 0, 0, 3));
                            //if (_t.IsAlive) _t.Abort();
                            _Thread = null;
                        }
                    }
                    else
                    {
                        if (!reconnect && _Thread != null)
                        {
                            lock (_Thread)
                            {
                                if (_Thread.IsAlive)
                                    _Thread.Join(new TimeSpan(0, 0, 0, 0, 3));
                                //if (_t.IsAlive) _t.Abort();
                                _Thread = null;
                            }
                        }
                    }
                    _IsStopping = false;
                }
                if (_Server != null) { _Server.Close(); _Server = null; }
                if (_Client != null) { _Client.Disconnect(EDissconnectReason.Requested); _Client = null; }
            }
            if (reconnect) { Thread.Sleep(1000); Start(reconnect); }
        }
        public bool Start() { return Start(false); }
        bool Start(bool reconnect)
        {
            if (!reconnect) Stop(false);
            try
            {
                lock (this)
                {
                    if (IsServer)
                    {
                        _Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        _Server.ExclusiveAddressUse = true;
                        _Server.NoDelay = true;
                        _Server.Bind(_IPEndPoint);
                        _Server.Listen(50);
                        //_t1.Start();

                        _Thread = new Thread(new ParameterizedThreadStart(server_thread));
                        _Thread.Name = "SOCKET-SERVER";
                    }
                    else
                    {
                        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        s.Connect(_IPEndPoint);

                        _Client = new XPloitSocketClient(this, s, _UseSpeedLimit);
                        _Thread = new Thread(new ParameterizedThreadStart(noserver_thread));
                        _Thread.Name = "SOCKET-CLIENT";

                        if (!Add(_Client)) throw (new ProtocolException());
                    }

                    _Thread.SetApartmentState(ApartmentState.MTA);
                    _Thread.IsBackground = true;
                    _Thread.Start(this);
                    if (OnStart != null) OnStart(this, null);
                }
                return true;
            }
            catch { }
            Stop(reconnect);
            return false;
        }
        #endregion

        #region SOCKET
        bool RaiseOnTimeOut(XPloitSocketClient cl)
        {
            if (OnOverrideTimeout != null)
            {
                CancelEventArgs cn = new CancelEventArgs(false);
                OnOverrideTimeout(this, cl, cn);

                if (cn.Cancel) return true;
            }
            return false;
        }
        static void noserver_thread(object o)
        {
            XPloitSocket cs = (XPloitSocket)o;
            XPloitSocketClient c = cs._Client;
            bool check_time_out = cs._TimeOut != TimeSpan.Zero;

            try
            {
                while (!cs._IsStopping && c.IsConnected)
                {
                    //lectura sincrona en este hilo
                    if (!c.Read(c) && check_time_out && c.HasTimeOut)
                    {
                        if (DateTime.Now - c.LastRead > cs._TimeOut && !cs.RaiseOnTimeOut(c))
                        {
                            c.Disconnect(EDissconnectReason.TimeOut);
                            break;
                        }
                    }
                    Thread.Sleep(0);
                }
            }
            catch { }

            cs.Remove(c, c.DisconnectReason);
            cs.Stop(true);
        }
        void socket_acept(IAsyncResult ar)
        {
            Socket hold = ((XPloitSocket)ar.AsyncState)._Server;
            if (!Enable) return;

            if (hold != null /*&& hold.Connected*/)
            {
                try
                {
                    Socket work = hold.EndAccept(ar);
                    if (work.Connected)
                    {
                        XPloitSocketClient tc = new XPloitSocketClient(this, work, _UseSpeedLimit);

                        if (_IPFilter != null && !_IPFilter.IsAllowed(tc.IPAddress)) tc.Disconnect(EDissconnectReason.Banned);
                        else
                            if (Connections >= _MaxConnections) tc.Disconnect(EDissconnectReason.MaxAllowed);
                            else Add(tc);
                    }
                }
                catch { }
            }
            if (_Server == null) return;

            AsyncCallback callme = new AsyncCallback(socket_acept);
            _Server.BeginAccept(callme, this);
        }
        static void server_thread(object o)
        {
            XPloitSocket cs = (XPloitSocket)o;
            DateTime time = DateTime.Now;

            bool check_time_out = cs._TimeOut != TimeSpan.Zero;
            AsyncCallback callme = new AsyncCallback(cs.socket_acept);
            cs._Server.BeginAccept(callme, cs);

            while (!cs._IsStopping)
            {
                //lock (cs._Clients) ya hace el lock el añadido
                //{
                for (int x = cs._Clients.Length - 1; x >= 0; x--)
                {
                    XPloitSocketClient c = cs._Clients[x];

                    //lectura sincrona en este hilo
                    if (!c.Read(c) && check_time_out && c.HasTimeOut)
                    {
                        if (DateTime.Now - c.LastRead > cs._TimeOut && !cs.RaiseOnTimeOut(c))
                        {
                            cs.Remove(c, EDissconnectReason.TimeOut);
                            continue;
                        }
                    }

                    if (!c.IsConnected) { cs.Remove(c, c.DisconnectReason); }
                    //}
                }
                Thread.Sleep(0);
            }
        }
        #endregion

        #region CONSTRUCTORES
        /// <summary>
        /// Constructor de cliente
        /// </summary>
        /// <param name="protocol">Protocolo</param>
        /// <param name="hostAndPort">Host y puerto</param>
        public XPloitSocket(IXPloitSocketProtocol protocol, string hostAndPort)
        {
            if (protocol == null) throw (new ArgumentNullException("protocol"));

            ushort port = DEFAULT_PORT;
            IPAddress host = null;
            if (!IPHelper.ParseIpPort(hostAndPort, out host, ref port))
            {
                throw (new ArgumentNullException("Host"));
            }

            _Protocol = protocol;
            _IPEndPoint = new IPEndPoint(host, port);
            IsServer = false;
        }
        /// <summary>
        /// Constructor de cliente
        /// </summary>
        /// <param name="protocol">Protocolo</param>
        /// <param name="host">Host</param>
        /// <param name="port">Puerto</param>
        public XPloitSocket(IXPloitSocketProtocol protocol, IPAddress host, ushort port, bool asServer)
        {
            if (protocol == null) throw (new ArgumentNullException("protocol"));

            _Protocol = protocol;
            _IPEndPoint = new IPEndPoint(host == null ? IPAddress.Any : host, port);
            IsServer = asServer;
        }
        /// <summary>
        /// Constructor de Servidor
        /// </summary>
        /// <param name="protocol">Protocolo</param>
        /// <param name="port">Puerto para leer</param>
        public XPloitSocket(IXPloitSocketProtocol protocol, ushort port)
        {
            if (protocol == null) throw (new ArgumentNullException("protocol"));

            _Protocol = protocol;
            _IPEndPoint = new IPEndPoint(IPAddress.Any, port);
            IsServer = true;
        }
        #endregion

        #region METODOS DE ENVIO
        public int Send(XPloitSocketClient cl, params  IXPloitSocketMsg[] msg)
        {
            if (cl == null) return 0;
            return cl.Send(msg);
        }
        public int SendAll(params IXPloitSocketMsg[] msg)
        {
            if (_Clients == null) return 0;

            int dv = 0;
            lock (_Clients)
            {
                foreach (XPloitSocketClient cl in _Clients)
                    dv += cl.Send(msg);
            }
            return dv;
        }
        #endregion

        /// <summary>
        /// En caso de servidor se permite conectar como cliente a otro servidor y que la lógica no cambie
        /// </summary>
        /// <param name="host">Host de conexión</param>
        /// <param name="prto">Puerto de conexión</param>
        /// <returns>Devuelve true si es capaz de conectar al server</returns>
        public bool ConnectClient(IPAddress ip, int prto)
        {
            if (!IsServer) throw (new Exception("ONLY USE Y IN SERVER MODE"));
            if (_IPFilter != null && !_IPFilter.IsAllowed(ip)) return false;
            if (Connections >= _MaxConnections) return false;

            XPloitSocketClient cl = null;
            Socket cl2 = null;
            try
            {
                cl2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                cl2.Connect(ip, prto);
                return Add(new XPloitSocketClient(this, cl2, _UseSpeedLimit));
            }
            catch
            {
                if (cl2 != null) { cl2.Close(); }
                if (cl != null) cl.Disconnect(EDissconnectReason.Error);
            }
            return false;
        }
        /// <summary>
        /// Add client
        /// </summary>
        /// <param name="cl">Client</param>
        bool Add(XPloitSocketClient cl)
        {
            if (!_Protocol.Connect(cl))
            {
                cl.Disconnect(EDissconnectReason.Protocol);
                return false;
            }

            lock (_Clients)
            {
                int l = _Clients.Length;
                Array.Resize(ref _Clients, l + 1);
                _Clients[l] = cl;
            }

            if (OnConnect != null) OnConnect(this, cl);
            return true;
        }
        void Remove(XPloitSocketClient c, EDissconnectReason dr)
        {
            lock (_Clients)
            {
                int l = _Clients.Length;
                XPloitSocketClient[] cs = new XPloitSocketClient[l - 1];
                for (int x = 0, y = 0; x < l; x++)
                {
                    XPloitSocketClient o = _Clients[x];
                    if (o == c) continue;

                    cs[y] = o;
                    y++;
                }
                _Clients = cs;
            }
            if (c != null) c.Disconnect(dr);
        }

        #region RAISES
        internal void RaiseOnDisconnect(XPloitSocketClient cl, EDissconnectReason dr)
        {
            if (OnDisconnect != null) OnDisconnect(this, cl, dr);
        }
        public void RaiseOnMessage(XPloitSocketClient cl, IXPloitSocketMsg msg)
        {
            if (msg == null) return;
            if (OnMessage != null) OnMessage(this, cl, msg);
        }
        public int RaiseActionForClient(delIsClient comparer, delIsClient action, params object[] tag)
        {
            if (action == null) return 0;
            int dv = 0;
            lock (_Clients)
            {
                foreach (XPloitSocketClient client in _Clients)
                    if (comparer == null || comparer(client, tag))
                    {
                        if (action(client, tag)) dv++;
                    }
            }
            return dv;
        }
        #endregion

        #region SEARCH CLIENT
        public XPloitSocketClient SearchClient(IPEndPoint ip_endpoint)
        {
            lock (_Clients)
            {
                foreach (XPloitSocketClient cl1 in _Clients)
                    if (cl1.IPEndPoint.Equals(ip_endpoint)) return cl1;
            }
            return null;
        }
        public XPloitSocketClient[] SearchClients(IPAddress ip)
        {
            List<XPloitSocketClient> lx = new List<XPloitSocketClient>();
            lock (_Clients)
            {
                foreach (XPloitSocketClient cl1 in _Clients)
                    if (cl1.IPAddress.Equals(ip)) lx.Add(cl1);
            }
            return lx.ToArray();
        }
        public XPloitSocketClient[] SearchClients(delIsClient comparer, params object[] tag)
        {
            List<XPloitSocketClient> lx = new List<XPloitSocketClient>();
            lock (_Clients)
            {
                foreach (XPloitSocketClient cl1 in _Clients)
                    if (comparer == null || comparer(cl1, tag)) lx.Add(cl1);
            }
            return lx.ToArray();
        }
        #endregion

        public void Dispose() { Stop(false, true); }
    }
}