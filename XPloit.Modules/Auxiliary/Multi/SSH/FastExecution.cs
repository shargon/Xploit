using Renci.SshNet;
using Renci.SshNet.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Multi.SSH
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Execute SSH stream to exe machine")]
    public class FastExecution : SShBaseModule
    {
        #region Properties
        [ConfigurableProperty(Description = "Remote command from ssh server")]
        public string Command { get; set; }

        [RequireExists()]
        [ConfigurableProperty(Description = "Path of executable for local execution")]
        public FileInfo Execute { get; set; }
        [ConfigurableProperty(Description = "Arguments for local execution", Optional = true)]
        public string Arguments { get; set; }
        #endregion

        public FastExecution() : base()
        {
            Command = "ls";
        }

        protected override bool RunSSHAction(SshClient ssh)
        {
            IDictionary<TerminalModes, uint> termkvp = new Dictionary<TerminalModes, uint>();
            termkvp.Add(TerminalModes.ECHO, 53);

            using (Process pr = Process.Start(new ProcessStartInfo()
            {
                Arguments = Arguments == null ? "" : Arguments,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
            }))
            {
                ShellStream cmd = null;
                try
                {
                    cmd = ssh.CreateShellStream("xterm", 100, 60, 1000, 600, 4096, termkvp);

                    // Read from SSH
                    cmd.DataReceived += (object sender, ShellDataEventArgs e) =>
                    {
                        string line = Encoding.UTF8.GetString(e.Data);
                        WriteInfo("> " + line);

                        pr.StandardInput.WriteLine(line);
                        pr.StandardInput.Flush();
                    };
                }
                catch { }

                while (!pr.HasExited && !ssh.IsConnected)
                {
                    // Read from program
                    string line = pr.StandardOutput.ReadLine();
                    WriteInfo("< " + line);

                    cmd.WriteLine(line);
                    cmd.Flush();

                    Thread.Sleep(10);
                }

                pr.WaitForExit();
            }

            return true;
        }
    }
}