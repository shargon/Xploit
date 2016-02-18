﻿using Auxiliary.Local.Fuzzing;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using XPloit.Core;
using XPloit.Core.Attributes;

namespace Payloads.Multi.Fuzzing
{
    public class TcpSocketFuzzer : Payload, StreamFuzzer.IFuzzerPayload
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Send fuzzer by TCP Socket"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "End point connection")]
        public IPEndPoint EndPoint { get; set; }
        [ConfigurableProperty(Required = true, Description = "Timeout for write")]
        public TimeSpan SendTimeout { get; set; }
        [ConfigurableProperty(Required = true, Description = "Timeout for read")]
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