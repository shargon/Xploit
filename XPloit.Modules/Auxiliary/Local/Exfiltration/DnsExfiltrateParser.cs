using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.VerbalExpressions;

namespace Auxiliary.Local.Exfiltration
{
    public class DnsExfiltrateParser : Module, AESHelper.IAESConfig
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "DNS-Exfiltration file parser"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
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
        #endregion

        #region Properties
        [FileRequireExists]
        [ConfigurableProperty(Required = true, Description = "File for parse")]
        public FileInfo File { get; set; }
        [ConfigurableProperty(Required = true, Description = "Folder where write the files")]
        public DirectoryInfo OutFolder { get; set; }

        [ConfigurableProperty(Required = true, Description = "Length of fileId")]
        public int FileIdLength { get; set; }
        [ConfigurableProperty(Required = true, Description = "Regex for Data")]
        public string RegexData { get; set; }

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

        public DnsExfiltrateParser()
        {
            AesIterations = 1000;
            AesKeyLength = AESHelper.EKeyLength.Length256;
            AesRGBSalt = null;

            RegexData = new VerbalExpressions().Find("\t").Something().Find("\t").ToString();
            FileIdLength = 2;
        }

        class packet
        {
            public int Order;
            public byte[] Data;
        }

        int sortPacket(packet a, packet b) { return a.Order.CompareTo(b.Order); }

        bool parse(string line, out string fileId, out packet packet)
        {
            fileId = "";
            packet = null;

            if (string.IsNullOrEmpty(line)) return false;

            Match m = Regex.Match(line, RegexData);
            if (m == null || !m.Success) return false;

            string dom;
            string data = m.Value.Trim();
            StringHelper.Split(data, '.', out data, out dom);
            fileId = data.Substring(0, FileIdLength);

            if (string.IsNullOrEmpty(fileId)) return false;
            int packetNum = BitConverterHelper.ToInt32(HexHelper.FromHexString(data.Substring(FileIdLength, 8)), 0);

            packet = new packet() { Data = HexHelper.FromHexString(data.Remove(0, FileIdLength + 8)), Order = packetNum };

            return true;
        }
        public override bool Run()
        {
            if (!File.Exists) return false;
            if (!OutFolder.Exists) return false;

            AESHelper aes = AESHelper.Create(this);
            if (aes != null) WriteInfo("Using AES Encryption");
            else WriteError("Read in RawMode (without any Encryption)");

            WriteInfo("Start reading file ...");
            Dictionary<string, List<packet>> dic = new Dictionary<string, List<packet>>();

            using (Stream fs = (Stream)File.OpenRead())
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string fileId;
                    packet packet;

                    if (!parse(line, out fileId, out packet)) continue;

                    if (dic.ContainsKey(fileId))
                    {
                        dic[fileId].Add(packet);
                    }
                    else
                    {
                        List<packet> pc = new List<packet>();
                        pc.Add(packet);
                        dic.Add(fileId, pc);
                    }
                }
            }

            WriteInfo("Located " + dic.Keys.Count.ToString() + (dic.Keys.Count == 1 ? " file" : " files"));
            if (dic.Keys.Count > 0)
            {
                WriteInfo("Reordering packets ...");

                foreach (string k in dic.Keys)
                {
                    List<packet> p = dic[k];
                    p.Sort(sortPacket);
                }

                WriteInfo("Dump files ...");

                foreach (string k in dic.Keys)
                {
                    List<packet> lp = dic[k];
                    string path = OutFolder + System.IO.Path.DirectorySeparatorChar.ToString() + k + ".dat";

                    using (MemoryStream ms = new MemoryStream())
                    {
                        foreach (packet p in lp)
                            ms.Write(p.Data, 0, p.Data.Length);

                        if (aes != null)
                        {
                            byte[] d = aes.Decrypt(ms.ToArray());
                            if (d == null)
                            {
                                WriteError("Error in decrypt process");
                                continue;
                            }
                            System.IO.File.WriteAllBytes(path, d);
                        }
                        else
                        {
                            System.IO.File.WriteAllBytes(path, ms.ToArray());
                        }

                        WriteInfo("Created file '" + path + "'", new FileInfo(path).Length.ToString(), ConsoleColor.Green);
                    }
                }
            }
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

                if (!OutFolder.Exists)
                {
                    WriteError("Folder must exists");
                    return ECheck.Error;
                }

                return ECheck.Ok;
            }
            catch { return ECheck.Error; }
        }
    }
}