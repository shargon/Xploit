﻿using Renci.SshNet;
using System;
using System.IO;
using System.Text;
using XPloit.Core.Attributes;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Multi.SSH
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Get a binay from SSH machine")]
    public class DownloadFile : SShBaseModule
    {
        delegate byte[] onData(string text);
        public enum EDumpMethod : byte
        {
            scp = 0,
            hexdump = 1,
            xxd = 2,
            perl = 3,
        }

        #region Properties
        [ConfigurableProperty(Description = "Remote Path from ssh server")]
        public string RemotePath { get; set; }
        //[RequireExists()]
        [ConfigurableProperty(Description = "Path for dump")]
        public FileInfo DumpPath { get; set; }
        [ConfigurableProperty(Description = "Method for dump")]
        public EDumpMethod Method { get; set; }
        #endregion

        public DownloadFile() : base()
        {
            Method = EDumpMethod.scp;
        }

        byte[] hexdump(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string x in text.Split(new char[] { '\n', '\r' }))
            {
                if (string.IsNullOrEmpty(x)) continue;

                int ix = x.IndexOf(' ');
                if (ix < 0) continue;

                string cad = x.Substring(ix);
                ix = cad.IndexOf('|');
                if (ix >= 0)
                    cad = cad.Substring(0, ix);

                sb.Append(cad);
            }
            return xxd(sb.ToString());
        }
        byte[] xxd(string text)
        {
            text = text.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            return HexHelper.FromHexString(text);
        }

        public override bool Run()
        {
            if (Method == EDumpMethod.scp)
            {
                WriteInfo("Connecting ...");

                using (ScpClient SSH = new ScpClient(SSHHost.Address.ToString(), SSHHost.Port, User, Password))
                {
                    SSH.Connect();
                    WriteInfo("Connected successful");

                    WriteInfo("Executing", "SCP download", ConsoleColor.Cyan);
                    SSH.Download(RemotePath, DumpPath);

                    DumpPath.Refresh();
                    WriteInfo("Download successful", StringHelper.Convert2KbWithBytes(DumpPath.Length), ConsoleColor.Cyan);
                    return true;
                }
            }

            return base.Run();
        }
        protected override bool RunSSHAction(SshClient ssh)
        {
            string command;
            onData handler;
            switch (Method)
            {
                default:
                case EDumpMethod.hexdump:
                    {
                        command = "hexdump -v -x " + RemotePath;
                        handler = new onData(hexdump);
                        break;
                    }
                case EDumpMethod.xxd:
                    {
                        command = "xxd -p " + RemotePath;
                        handler = new onData(xxd);
                        break;
                    }
                case EDumpMethod.perl:
                    {
                        command = "perl -e 'local $/; print unpack \"H*\", <>' " + RemotePath;
                        handler = new onData(xxd);
                        break;
                    }
            }

            WriteInfo("Executing", command, ConsoleColor.Cyan);

            SshCommand cmd = ssh.RunCommand(command);

            if (!string.IsNullOrEmpty(cmd.Error))
                WriteError(cmd.Error);

            if (!string.IsNullOrEmpty(cmd.Result))
            {
                byte[] data = handler.Invoke(cmd.Result);
                if (data != null && data.Length > 0)
                {
                    File.WriteAllBytes(DumpPath.FullName, data);
                    WriteInfo("Download successful", StringHelper.Convert2KbWithBytes(data.Length), ConsoleColor.Cyan);
                }
            }

            return true;
        }
    }
}