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
        [ConfigurableProperty(Description = "Local address at ssh server (127.0.0.1:3306)")]
        public IPEndPoint SSHLocalAddress { get; set; }
        [ConfigurableProperty(Description = "Local binding address (127.0.0.1:3307)")]
        public IPEndPoint LocalAddress { get; set; }

        [ConfigurableProperty(Description = "Method for forwarding")]
        public EForwardMethod Method { get; set; }
        #endregion

        public PortForwarding() : base()
        {
            SSHLocalAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3306);
            LocalAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3307);
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
                        port = new ForwardedPortRemote(SSHLocalAddress.Address, (uint)SSHLocalAddress.Port, LocalAddress.Address, (uint)LocalAddress.Port);
                        break;
                    }
                case EForwardMethod.RemoteToLocal:
                    {
                        port = new ForwardedPortLocal(LocalAddress.Address.ToString(), (uint)LocalAddress.Port, SSHLocalAddress.Address.ToString(), (uint)SSHLocalAddress.Port);
                        break;
                    }
            }

            WriteInfo("Connecting ...");

            SshClient SSH = new SshClient(SSHHost.Address.ToString(), SSHHost.Port, User, Password);

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