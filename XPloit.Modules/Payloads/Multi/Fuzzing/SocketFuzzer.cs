using Auxiliary.Local.Fuzzing;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;

namespace Payloads.Multi.Windows.WMI.Action
{
    public class SocketFuzzer : Payload, Fuzzer.IFuzzerPayload
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Send fuzzer by socket"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "End point connection")]
        public IPEndPoint EndPoint { get; set; }
        [ConfigurableProperty(Required = true, Description = "Number of bytes for read, after send")]
        public int ReadAfterSend { get; set; }
        [ConfigurableProperty(Required = true, Description = "Number of bytes for read, before send")]
        public int ReadBeforeSend { get; set; }
        #endregion

        public SocketFuzzer()
        {
            ReadAfterSend = 1;
            ReadBeforeSend = 0;
        }

        public void Run(string ascii)
        {
            byte[] data = Encoding.ASCII.GetBytes(ascii), tmp = new byte[Math.Max(ReadAfterSend, ReadBeforeSend)];

            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                socket.SendBufferSize = data.Length;
                socket.NoDelay = true;

                socket.Connect(EndPoint);

                if (ReadBeforeSend > 0)
                    socket.Receive(tmp, 0, ReadBeforeSend, SocketFlags.None);

                socket.Send(data, 0, data.Length, SocketFlags.None);

                if (ReadAfterSend > 0)
                    socket.Receive(tmp, 0, ReadBeforeSend, SocketFlags.None);

                socket.Close();
            }
        }
    }
}