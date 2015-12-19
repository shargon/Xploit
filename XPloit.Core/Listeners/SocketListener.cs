using System;
using System.Net;
using System.Text;
using XPloit.Core.Configs;
using XPloit.Core.Helpers.Crypt;
using XPloit.Core.Interfaces;
using XPloit.Core.Sockets;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;
using XPloit.Core.Sockets.Protocols;

namespace XPloit.Core.Listeners
{
    public class SocketListener : IListener
    {
        Encoding _Codec = Encoding.UTF8;

        ListenSocketConfig _Config;
        XPloitSocket _Socket;
        bool _IsServer;

        public override string ToString()
        {
            IPAddress ip = _Config.ListenAddress;
            if (ip == null) ip = IPAddress.Any;

            return "SocketListener (" + ip.ToString() + ":" + _Config.ListenPort.ToString() + ")";
        }

        /// <summary>
        /// IsServer
        /// </summary>
        public bool IsServer { get { return _IsServer; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configurar</param>
        /// <param name="isServer">True if its server</param>
        public SocketListener(ListenSocketConfig config, bool isServer)
        {
            _Config = config;
            _IsServer = isServer;
        }

        public override bool IsStarted { get { return _Socket != null && _Socket.Enable; } }

        public override bool Start()
        {
            Stop();

            // Crypt password

            AESHelper crypt = null;
            if (_Config.CryptKey != null && !string.IsNullOrEmpty(_Config.CryptKey.RawPassword))
                crypt = new AESHelper(_Config.CryptKey.RawPassword, "Made with love ;)", 20000, "**#Int#Vector#**", AESHelper.EKeyLength.Length_256);

            _Socket = new XPloitSocket(new XPloitSocketProtocol(_Codec, crypt, XPloitSocketProtocol.EProtocolMode.None), _Config.ListenAddress, _Config.ListenPort, _IsServer)
            {
                IPFilter = _Config.IPFilter,
                TimeOut = TimeSpan.Zero
            };

            _Socket.OnConnect += _Socket_OnConnect;
            _Socket.OnDisconnect += _Socket_OnDisconnect;
            _Socket.OnMessage += _Socket_OnMessage;
            return _Socket.Start();
        }

        void _Socket_OnConnect(XPloitSocket sender, XPloitSocketClient cl)
        {
            //cl.Tag=new StreamListener(_Codec,null,null,null);
        }
        void _Socket_OnDisconnect(XPloitSocket sender, XPloitSocketClient cl, EDissconnectReason e)
        {

        }
        void _Socket_OnMessage(XPloitSocket sender, XPloitSocketClient cl, IXPloitSocketMsg msg)
        {

        }

        public override bool Stop()
        {
            if (_Socket != null)
            {
                _Socket.Dispose();
                _Socket = null;
            }

            return true;
        }
    }
}