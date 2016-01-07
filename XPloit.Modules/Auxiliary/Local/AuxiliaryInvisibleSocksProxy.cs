using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Sockets.Proxy;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliaryInvisibleSocksProxy : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Invisible socks proxy"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Name { get { return "InvisibleSocksProxy"; } }
        public override Target[] Targets
        {
            get
            {
                return new Target[]
                {
                    new Target(EPlatform.None ,"Socks4", new Variable("Version",4) ),
                    new Target(EPlatform.None ,"Socks5" , new Variable("Version",4)  ),
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Local port for listening")]
        public ushort LocalPort { get; set; }
        [ConfigurableProperty(Required = true, Description = "Connect to")]
        public IPEndPoint RemoteEndPoint { get; set; }

        [ConfigurableProperty(Required = true, Description = "Proxy")]
        public IPEndPoint ProxyEndPoint { get; set; }
        [ConfigurableProperty(Description = "Proxy user")]
        public string ProxyUser { get; set; }
        [ConfigurableProperty(Description = "Proxy Password")]
        public string ProxyPassword { get; set; }
        #endregion

        public AuxiliaryInvisibleSocksProxy()
        {
            ProxyEndPoint = new IPEndPoint(IPAddress.Loopback, 9050);
            LocalPort = 10000;
        }

        public override ECheck Check()
        {
            if (SystemHelper.IsAvailableTcpPort(LocalPort)) return ECheck.Ok;
            return ECheck.Error;
        }

        public override bool Run()
        {
            byte version = Convert.ToByte(Target["Version"]);

            TcpForwarder tcp = new TcpForwarder(this, ProxyEndPoint, version, ProxyUser, ProxyPassword);
            try
            {
                tcp.Start(new IPEndPoint(IPAddress.Any, LocalPort), RemoteEndPoint);
            }
            catch
            {
                tcp.Dispose();
                return false;
            }
            return CreateJob(tcp) != null;
        }

        public class TcpForwarder : Job.IJobable
        {
            List<TcpForwarder> socks = new List<TcpForwarder>();

            Socket MainSocket = null;
            IPEndPoint ProxySocket = null;
            byte bSocketType = 0;
            AuxiliaryInvisibleSocksProxy _Module;
            string _User, _Password;

            public TcpForwarder(AuxiliaryInvisibleSocksProxy m, IPEndPoint proxy_socket, byte socket_type, string user, string password) :
                this(m, proxy_socket, socket_type, false, user, password) { }
            TcpForwarder(AuxiliaryInvisibleSocksProxy m, IPEndPoint proxy_socket, byte socket_type, bool this_is_socks, string user, string password)
            {
                _Module = m;
                _User = user;
                _Password = password;

                if ((socket_type == 4 || socket_type == 5) && proxy_socket != null)
                {
                    ProxySocket = proxy_socket;
                    bSocketType = socket_type;

                    if (this_is_socks)
                    {
                        MainSocket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            ProxyEndPoint = proxy_socket,
                            ProxyType = socket_type == 4 ? ProxyTypes.Socks4 : ProxyTypes.Socks5,
                            ProxyUser = string.IsNullOrEmpty(user) ? null : user,
                            ProxyPass = string.IsNullOrEmpty(password) ? null : password
                        };
                        return;
                    }
                }

                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            public void Start(IPEndPoint local, IPEndPoint remote)
            {
                MainSocket.Bind(local);
                MainSocket.Listen(10);

                Thread th = new Thread(new ParameterizedThreadStart(thStart));
                th.IsBackground = true;
                th.Name = "InvisibleSocks";
                th.Start(remote);
            }
            void thStart(object sender)
            {
                IPEndPoint remote = (IPEndPoint)sender;

                while (!IsDisposed)
                {
                    try
                    {
                        Socket source = MainSocket.Accept();

                        _Module.WriteInfo("Connected: " + source.RemoteEndPoint.ToString());

                        TcpForwarder destination = new TcpForwarder(_Module, ProxySocket, bSocketType, true, _User, _Password);
                        socks.Add(destination);

                        State state = new State(source, destination.MainSocket);
                        destination.Connect(remote, source);
                        source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            void Connect(EndPoint remoteEndpoint, Socket destination)
            {
                State state = new State(MainSocket, destination);

                try
                {
                    if (MainSocket is ProxySocket) ((ProxySocket)MainSocket).ConnectSocks(remoteEndpoint);
                    else MainSocket.Connect(remoteEndpoint);

                    MainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
                }
                catch
                {
                    _Module.WriteError("Error connecting to " + remoteEndpoint.ToString());
                }
            }

            public override void OnDispose()
            {
                try { MainSocket.Dispose(); }
                catch { }

                try
                {
                    lock (socks)
                    {
                        foreach (TcpForwarder tcp in socks) tcp.Dispose();
                    }
                }
                catch { }

                base.OnDispose();
            }

            static void OnDataReceive(IAsyncResult result)
            {
                State state = (State)result.AsyncState;
                try
                {
                    int bytesRead = state.SourceSocket.EndReceive(result);
                    if (bytesRead > 0)
                    {
                        state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                        state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                    }
                }
                catch
                {
                    state.DestinationSocket.Close();
                    state.SourceSocket.Close();
                }
            }

            class State
            {
                public Socket SourceSocket { get; private set; }
                public Socket DestinationSocket { get; private set; }
                public byte[] Buffer { get; private set; }

                public State(Socket source, Socket destination)
                {
                    SourceSocket = source;
                    DestinationSocket = destination;
                    Buffer = new byte[8192];
                }
            }
        }
    }
}
