using System;
using System.Net;
using System.Net.Sockets;
using XPloit.Core.Attributes;

namespace XPloit.Core.Interfaces
{
    public class TcpModule : Module
    {
        #region Properties
        [ConfigurableProperty(Description = "End point connection")]
        public IPEndPoint EndPoint { get; set; }
        [ConfigurableProperty(Description = "Timeout for write")]
        public TimeSpan SendTimeout { get; set; }
        [ConfigurableProperty(Description = "Timeout for read")]
        public TimeSpan ReceiveTimeout { get; set; }
        #endregion

        protected TcpModule()
        {
            SendTimeout = TimeSpan.FromSeconds(10);
            ReceiveTimeout = TimeSpan.FromSeconds(10);
        }

        protected void Close(TcpClient tcp)
        {
            if (tcp != null)
            {
                try { tcp.Close(); }
                catch { }
            }
        }
        protected TcpClient Connect()
        {
            TcpClient tcp = new TcpClient();
            tcp.NoDelay = true;
            tcp.SendTimeout = (int)SendTimeout.TotalMilliseconds;
            tcp.ReceiveTimeout = (int)ReceiveTimeout.TotalMilliseconds;

            tcp.Connect(EndPoint);
            return tcp;
        }
    }
}