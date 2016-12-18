using System;
using System.IO;
using System.Net;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Sockets;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Server
{
    public class SocksPortForwarding : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Invisible socks port forwarding"; } }
        public override Target[] Targets
        {
            get
            {
                return new Target[]
                {
                    new Target("Socks4", new Variable("Version",4) ),
                    new Target("Socks5" , new Variable("Version",4)  ),
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Local port for listening")]
        public ushort LocalPort { get; set; }
        [ConfigurableProperty(Description = "Connect to")]
        public IPEndPoint RemoteEndPoint { get; set; }

        #region Optional
        [ConfigurableProperty(Optional = true, Description = "Proxy")]
        public IPEndPoint ProxyEndPoint { get; set; }
        [ConfigurableProperty(Optional = true, Description = "Proxy user")]
        public string ProxyUser { get; set; }
        [ConfigurableProperty(Optional = true, Description = "Proxy Password")]
        public string ProxyPassword { get; set; }
        [RequireExists]
        [ConfigurableProperty(Optional = true, Description = "Filter file OnSend/OnReceive(ref byte[]data, ref int index, ref int length)")]
        public FileInfo FilterFile { get; set; }
        #endregion
        #endregion

        public SocksPortForwarding()
        {
            ProxyEndPoint = new IPEndPoint(IPAddress.Loopback, 9050);
            LocalPort = 10000;
        }

        public override ECheck Check()
        {
            if (!SystemHelper.IsAvailableTcpPort(LocalPort)) return ECheck.Error;

            if (FilterFile != null)
            {
                if (!FilterFile.Exists)
                {
                    WriteError("FilterFile dosen't exists");
                    return ECheck.Error;
                }

                ScriptHelper script = ScriptHelper.CreateFromFile(FilterFile.FullName, ScriptHelper.DefaultCoreOptions);
                object obj = script.CreateNewInstance();

                TcpForwarder.delDataFilter s = (TcpForwarder.delDataFilter)ReflectionHelper.GetDelegate<TcpForwarder.delDataFilter>(obj, "OnSend");
                TcpForwarder.delDataFilter r = (TcpForwarder.delDataFilter)ReflectionHelper.GetDelegate<TcpForwarder.delDataFilter>(obj, "OnReceive");

                ReflectionHelper.FreeObject(obj);

                WriteInfo("Filter Send", s == null ? "NULL" : "OK", s == null ? ConsoleColor.Red : ConsoleColor.Green);
                WriteInfo("Filter Receive", r == null ? "NULL" : "OK", r == null ? ConsoleColor.Red : ConsoleColor.Green);

                return ECheck.Ok;
            }

            return ECheck.Error;
        }

        [NonJobable()]
        public override bool Run()
        {
            byte version = Convert.ToByte(Target["Version"]);

            object filterObject = null;
            TcpForwarder.delDataFilter s = null, r = null;

            if (FilterFile != null)
            {
                if (!FilterFile.Exists)
                {
                    WriteError("FilterFile dosen't exists");
                    return false;
                }

                ScriptHelper script = ScriptHelper.CreateFromFile(FilterFile.FullName, ScriptHelper.DefaultCoreOptions);
                filterObject = script.CreateNewInstance();

                s = (TcpForwarder.delDataFilter)ReflectionHelper.GetDelegate<TcpForwarder.delDataFilter>(filterObject, "OnSend");
                r = (TcpForwarder.delDataFilter)ReflectionHelper.GetDelegate<TcpForwarder.delDataFilter>(filterObject, "OnReceive");

                WriteInfo("Filter Send", s == null ? "NULL" : "OK", s == null ? ConsoleColor.Red : ConsoleColor.Green);
                WriteInfo("Filter Receive", r == null ? "NULL" : "OK", r == null ? ConsoleColor.Red : ConsoleColor.Green);
            }

            TcpForwarder tcp = null;
            try
            {
                tcp = new TcpForwarder(ProxyEndPoint, version, ProxyUser, ProxyPassword, s, r);

                tcp.OnConnect += Tcp_OnConnect;
                tcp.OnEror += Tcp_OnEror;
                tcp.OnDisposed += (sender, e) => { ReflectionHelper.FreeObject(filterObject); };

                tcp.Start(new IPEndPoint(IPAddress.Any, LocalPort), RemoteEndPoint);
            }
            catch
            {
                if (tcp != null) tcp.Dispose();
                return false;
            }

            return CreateJob(tcp) != null;
        }

        void Tcp_OnEror(object sender, EndPoint endPoint) { WriteError("Error connecting to " + endPoint); }
        void Tcp_OnConnect(object sender, EndPoint endPoint) { WriteInfo("Connected: " + endPoint.ToString()); }
    }
}