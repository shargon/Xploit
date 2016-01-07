using System;
using System.Net;
using System.Text;
using XPloit.Core.Helpers.Crypt;
using XPloit.Core.Interfaces;
using XPloit.Core.Multi;
using XPloit.Core.Sockets;
using XPloit.Core.Sockets.Configs;
using XPloit.Core.Sockets.Enums;
using XPloit.Core.Sockets.Interfaces;
using XPloit.Core.Sockets.Protocols;

namespace XPloit.Core.Listeners
{
    public class SocketListener : IListener
    {
        Encoding _Codec = Encoding.UTF8;

        IPFilter _IPFilter;
        ClientSocketConfig _Config;
        XPloitSocket _Socket;
        bool _IsServer;

        public override string ToString()
        {
            IPAddress ip = _Config.Address;
            if (ip == null) ip = IPAddress.Any;

            return "SocketListener (" + ip.ToString() + ":" + _Config.Port.ToString() + ")";
        }

        /// <summary>
        /// IsServer
        /// </summary>
        public bool IsServer { get { return _IsServer; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configurar</param>
        public SocketListener(ClientSocketConfig config)
        {
            _Config = config;
            _IsServer = config is ListenSocketConfig;
            _IPFilter = _IsServer ? ((ListenSocketConfig)config).IPFilter : null;
        }

        public override bool IsStarted { get { return _Socket != null && _Socket.Enable; } }

        public override bool Start()
        {
            Stop();

            // Crypt password

            AESHelper crypt = null;
            if (_Config.CryptKey != null && !string.IsNullOrEmpty(_Config.CryptKey.RawPassword))
                crypt = new AESHelper(_Config.CryptKey.RawPassword, "Made with love ;)", 20000, "**#Int#Vector#**", AESHelper.EKeyLength.Length_256);

            _Socket = new XPloitSocket(new XPloitSocketProtocol(_Codec, crypt, XPloitSocketProtocol.EProtocolMode.UInt16), _Config.Address, _Config.Port, _IsServer)
            {
                IPFilter = _IPFilter,
                TimeOut = TimeSpan.Zero
            };

            _Socket.OnConnect += _Socket_OnConnect;
            _Socket.OnDisconnect += _Socket_OnDisconnect;
            _Socket.OnMessage += _Socket_OnMessage;
            return _Socket.Start();
        }

        void _Socket_OnConnect(XPloitSocket sender, XPloitSocketClient client)
        {
        }
        void _Socket_OnDisconnect(XPloitSocket sender, XPloitSocketClient client, EDissconnectReason e)
        {
            
        }
        void _Socket_OnMessage(XPloitSocket sender, XPloitSocketClient client, IXPloitSocketMsg msg)
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