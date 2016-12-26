using Renci.SshNet;
using System.Net;
using System.Threading;
using XPloit.Core.Interfaces;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Multi.SSH
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Port Forwarding from SSH machine")]
    public class PortForwarding : SShBaseModule
    {
        public enum EForwardMethod : byte
        {
            LocalToRemote = 0,
            RemoteToLocal = 1,
        }

        #region Properties
        [ConfigurableProperty(Description = "Remote Host from server")]
        public IPEndPoint RemoteHost { get; set; }
        [ConfigurableProperty(Description = "Local Host")]
        public IPEndPoint LocalHost { get; set; }

        [ConfigurableProperty(Description = "Method for forwarding")]
        public EForwardMethod Method { get; set; }
        #endregion

        public PortForwarding() : base()
        {
            RemoteHost = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80);
            LocalHost = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            Method = EForwardMethod.RemoteToLocal;
        }

        public override bool Run()
        {
            ForwardedPort port;
            switch (Method)
            {
                default:
                case EForwardMethod.LocalToRemote:
                    {
                        port = new ForwardedPortRemote(RemoteHost.Address, (uint)RemoteHost.Port, LocalHost.Address, (uint)LocalHost.Port);
                        break;
                    }
                case EForwardMethod.RemoteToLocal:
                    {
                        port = new ForwardedPortLocal(LocalHost.Address.ToString(), (uint)LocalHost.Port, RemoteHost.Address.ToString(), (uint)RemoteHost.Port);
                        break;
                    }
            }

            WriteInfo("Connecting ...");

            SshClient SSH = new SshClient(Host.Address.ToString(), Host.Port, User, Password);

            SSH.Connect();
            WriteInfo("Connected successful");

            Thread thread = new Thread(new ParameterizedThreadStart(jobRun));
            thread.IsBackground = true;
            thread.Name = "SSH-PortForwarding";

            thread.Start(new object[] { SSH, port });

            IJobable job = thread;
            job.Tag = SSH;
            job.DisposeTag = true;

            CreateJob(job);
            return true;
        }
        /// <summary>
        /// Thread
        /// </summary>
        /// <param name="o">Parameters</param>
        void jobRun(object o)
        {
            SshClient ssh = (SshClient)((object[])o)[0];
            ForwardedPort port = (ForwardedPort)((object[])o)[1];

            ssh.AddForwardedPort(port);
            port.RequestReceived += (sender2, e2) =>
            {
                WriteInfo(e2.OriginatorHost + ":" + e2.OriginatorPort);
            };

            port.Start();

            while (port.IsStarted) { Thread.Sleep(10); }
        }
    }
}