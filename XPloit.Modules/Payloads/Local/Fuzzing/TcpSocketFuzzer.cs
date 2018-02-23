using Auxiliary.Local.Fuzzing;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.Fuzzing
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Send fuzzer by TCP Socket")]
    public class TcpSocketFuzzer : Payload, StreamFuzzer.IFuzzerPayload
    {
        #region Properties
        [ConfigurableProperty(Description = "End point connection")]
        public IPEndPoint EndPoint { get; set; }
        [ConfigurableProperty(Description = "Timeout for write")]
        public TimeSpan SendTimeout { get; set; }
        [ConfigurableProperty(Description = "Timeout for read")]
        public TimeSpan ReceiveTimeout { get; set; }
        #endregion

        public TcpSocketFuzzer()
        {
            SendTimeout = TimeSpan.FromSeconds(10);
            ReceiveTimeout = TimeSpan.FromSeconds(10);
        }

        public Stream CreateStream(byte[] data)
        {
            TcpClient tcp = new TcpClient();
            tcp.NoDelay = true;
            tcp.SendTimeout = (int)SendTimeout.TotalMilliseconds;
            tcp.ReceiveTimeout = (int)ReceiveTimeout.TotalMilliseconds;

            tcp.Connect(EndPoint);
            return tcp.GetStream();
        }
    }
}