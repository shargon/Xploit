using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;

namespace Auxiliary.Local
{
    public class Tor : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Tor Process"; } }
        public override Reference[] References { get { return new Reference[] { new Reference(EReferenceType.URL, "https://www.torproject.org/"), }; } }
        public override Target[] Targets
        {
            get { return new Target[] { new Target(EPlatform.Windows, EArquitecture.x86, "Windows x86") }; }
        }
        public override IPayloadRequirements PayloadRequirements { get { return null; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Start socks proxy in this Port")]
        public ushort SocksPort { get; set; }
        [ConfigurableProperty(Description = "Expose this local ports to Tor netowork")]
        public uint[] HiddenServicePorts { get; set; }
        #endregion

        public Tor() { SocksPort = 9051; }

        public override ECheck Check()
        {
            Process[] procs = Process.GetProcessesByName("tor");
            if (procs != null && procs.Length > 0)
            {
                WriteError("Tor is executing on pid " + procs[0].Id.ToString());
                return ECheck.Error;
            }

            if (!SystemHelper.IsAvailableTcpPort(new IPEndPoint(IPAddress.Any, SocksPort)))
            {
                WriteError("Port " + SocksPort.ToString() + " is already in use");
                return ECheck.Error;
            }

            return ECheck.Ok;
        }

        public override bool Run()
        {
            Process[] procs = Process.GetProcessesByName("tor");
            if (procs != null && procs.Length > 0)
            {
                WriteError("Tor is executing on pid " + procs[0].Id.ToString());
                return false;
            }

            WriteInfo("Checking last Version of TorBrowser");
            string html = HttpHelper.DownloadString("https://dist.torproject.org/torbrowser/", this);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            string path = null;
            Version selected = null;
            foreach (HtmlNode o in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (o.Attributes.Count != 1) continue;

                string at = o.Attributes["href"].Value;
                if (string.IsNullOrEmpty(at)) continue;

                at = at.TrimEnd('/');
                if (at.Contains("a") || at.Contains("update")) continue;

                Version v;
                if (!Version.TryParse(at, out v)) continue;

                if (selected == null || v > selected)
                {
                    selected = v;
                    path = at;
                }
            }

            if (string.IsNullOrEmpty(path)) return false;

            WriteInfo("Last stable version", path, ConsoleColor.Green);
            WriteInfo("Checking last Version of Tor");

            html = HttpHelper.DownloadString("https://dist.torproject.org/torbrowser/" + path + "/", this);
            doc.LoadHtml(html);

            string fpath = null;
            foreach (HtmlNode o in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (o.Attributes.Count != 1) continue;

                string at = o.Attributes["href"].Value;
                if (string.IsNullOrEmpty(at)) continue;

                at = at.TrimEnd('/');
                if (!at.StartsWith("tor-win32-") || !at.EndsWith(".zip")) continue;

                if (fpath == null || fpath.CompareTo(at) > 0)
                    fpath = at;
            }

            if (string.IsNullOrEmpty(fpath)) return false;
            WriteInfo("Last stable version", fpath, ConsoleColor.Green);

            // Descargar y descomprimir
            string tempDir = System.IO.Path.GetTempPath();
            string torFile = tempDir + fpath + System.IO.Path.DirectorySeparatorChar + "Tor" + System.IO.Path.DirectorySeparatorChar + "tor.exe";
            if (!File.Exists(torFile))
            {
                WriteInfo("Downloading Tor", StringHelper.Convert2KbWithBytes(HttpHelper.GetHeadResponse("https://dist.torproject.org/torbrowser/" + path + "/" + fpath).ContentLength), ConsoleColor.Green);
                using (MemoryStream output = new MemoryStream())
                {
                    HttpHelper.Response res = HttpHelper.Download("https://dist.torproject.org/torbrowser/" + path + "/" + fpath, output, this);
                    if (!res.IsOk) return false;

                    path = tempDir + fpath + System.IO.Path.DirectorySeparatorChar;

                    WriteInfo("Uncompressing zip file");

                    ZipArchive zip = new ZipArchive(output);
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (entry.CompressedLength == 0) continue;

                        string dir = System.IO.Path.GetDirectoryName(path + entry.FullName);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        FileInfo fi = new FileInfo(path + entry.FullName);
                        if (fi.Exists) fi.Delete();

                        entry.ExtractToFile(fi.FullName);

                        fi.Refresh();
                        WriteInfo(entry.FullName.PadRight(30, '.'), StringHelper.Convert2KbWithBytes(fi.Length), ConsoleColor.Green);
                    }
                }
            }
            else
            {
                WriteInfo("Using files from '" + tempDir + fpath + "'");
            }

            // Tor downloaded
            string torcFile = System.IO.Path.ChangeExtension(torFile, "cfg");

            WriteInfo("Making torc file in '" + torcFile + "'");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SocksPort " + SocksPort);
            sb.AppendLine("SocksBindAddress 0.0.0.0");

            if (HiddenServicePorts != null)
            {
                foreach (int p in HiddenServicePorts)
                {
                    // Clean folder
                    string hservice = tempDir + fpath + System.IO.Path.DirectorySeparatorChar + "Tor" + System.IO.Path.DirectorySeparatorChar + p.ToString();
                    if (Directory.Exists(hservice)) Directory.Delete(hservice, true);

                    sb.AppendLine("HiddenServiceDir " + tempDir + fpath + System.IO.Path.DirectorySeparatorChar + "Tor" + System.IO.Path.DirectorySeparatorChar + p.ToString());
                    sb.AppendLine("HiddenServicePort " + p + " 127.0.0.1:" + p.ToString());
                }
            }

            File.WriteAllText(torcFile, sb.ToString());

            WriteInfo("Executing Tor");

            ProcessStartInfo pi = new ProcessStartInfo(torFile, "-f \"" + torcFile + "\"")
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process pr = Process.Start(pi);

            pr.EnableRaisingEvents = true;
            pr.OutputDataReceived += Pr_OutputDataReceived;
            pr.ErrorDataReceived += Pr_ErrorDataReceived;
            pr.BeginOutputReadLine();
            pr.BeginErrorReadLine();

            WriteInfo("Executed at PID", pr.Id.ToString(), ConsoleColor.Green);

            while (!pr.HasExited)
            {
                // Wait connections
                if (!pr.EnableRaisingEvents)
                    break;

                Thread.Sleep(500);
            }

            if (!pr.HasExited)
            {
                if (HiddenServicePorts != null)
                {
                    foreach (int p in HiddenServicePorts)
                    {
                        string hservice = tempDir + fpath + System.IO.Path.DirectorySeparatorChar + "Tor" + System.IO.Path.DirectorySeparatorChar + p.ToString() + System.IO.Path.DirectorySeparatorChar + "hostname";
                        string[] hd = File.ReadAllLines(hservice);
                        if (hd != null)
                        {
                            foreach (string s in hd)
                            {
                                WriteInfo("Hidden services address for Port " + p.ToString(), s, ConsoleColor.Cyan);
                            }
                        }
                    }
                }

                Job.Create(this, pr);
                return true;
            }

            return false;
        }

        void Pr_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            WriteError(e.Data);
        }
        void Pr_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            WriteInfo(e.Data);

            if (e.Data.Contains("Bootstrapped 100%: Done"))
            {
                // Connection successfully
                Process pr = (Process)sender;
                pr.EnableRaisingEvents = false;
            }
        }
    }
}