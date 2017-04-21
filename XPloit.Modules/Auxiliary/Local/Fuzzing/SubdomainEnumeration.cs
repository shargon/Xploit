using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Streams;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Fuzzing
{
    [ModuleInfo(Author = "Álvaro Díaz Hernández", Description = "Enumerating Subdomains for a wordlist")]
    public class SubdomainEnumeration : Module
    {
        #region Configure
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "Without References") }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Domain")]
        public string Domain { get; set; }

        [RequireExists()]
        [ConfigurableProperty(Description = "WordList path")]
        public FileInfo WordList { get; set; }
        #endregion

        public override ECheck Check()
        {
            // Info about word list
            string tempFile = null;
            Stream stream = File.OpenRead(WordList.FullName);

            if (FileHelper.DetectFileFormat(stream, true, true) == FileHelper.EFileFormat.Gzip)
            {
                WriteInfo("Decompress gzip wordlist");
                WriteInfo("Compressed size", StringHelper.Convert2KbWithBytes(stream.Length), ConsoleColor.Green);

                stream.Close();
                stream.Dispose();

                using (FileStream streamR = File.OpenRead(WordList.FullName))
                using (GZipStream gz = new GZipStream(streamR, CompressionMode.Decompress))
                {
                    // decompress
                    tempFile = Path.GetTempFileName();
                    stream = File.Open(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    gz.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }

                WriteInfo("Decompressed size", StringHelper.Convert2KbWithBytes(stream.Length), ConsoleColor.Green);
            }

            using (StreamLineReader reader = new StreamLineReader(stream))
                WriteInfo("Subdomains count", reader.GetCount(0).ToString(), ConsoleColor.Green);

            // Clean up
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }

            if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                File.Delete(tempFile);

            // Info about main domain
            IPHostEntry hostInfo = Dns.GetHostEntry(Domain);
            if (hostInfo != null)
            {
                string ip =
                   hostInfo.AddressList == null || hostInfo.AddressList.Length == 0 ? "FOUND" :
                   string.Join(",", hostInfo.AddressList.Select(u => u.ToString()).ToArray());

                WriteInfo("Host: " + Domain.ToString(), ip, ConsoleColor.Green);
                return ECheck.Ok;
            }

            return ECheck.NotSure;
        }
        public override bool Run()
        {
            // Read the list and check if exists the subdomain.  
            Parallel.ForEach(FileHelper.ReadWordFile(WordList.FullName), subdomain =>
             {
                 string url = subdomain + "." + Domain;
                 try
                 {
                     IPHostEntry hostInfo = Dns.GetHostEntry(url);
                     if (hostInfo.ToString() != "")
                     {
                         string ip =
                            hostInfo.AddressList == null || hostInfo.AddressList.Length == 0 ? "FOUND" :
                            string.Join(",", hostInfo.AddressList.Select(u => u.ToString()).ToArray());

                         WriteInfo("Subdomain found: " + subdomain.ToString() + "." + Domain.ToString(), ip, ConsoleColor.Green);
                     }
                 }
                 catch
                 {
                 }
             });

            return true;
        }
    }
}
