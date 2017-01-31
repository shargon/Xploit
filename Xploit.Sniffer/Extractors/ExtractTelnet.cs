using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using XPloit.Sniffer.Enums;
using XPloit.Sniffer.Interfaces;
using XPloit.Sniffer.Streams;

namespace XPloit.Sniffer.Extractors
{
    public class ExtractTelnet : IObjectExtractor
    {
        public class TelnetCredential : Credential
        {
            public TelnetCredential() : base(ECredentialType.Telnet) { }
            public TelnetCredential(DateTime date, IPEndPoint ip) : base(date, ip, ECredentialType.Telnet) { }
            /// <summary>
            /// User
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string Password { get; set; }
        }


        static IObjectExtractor _Current = new ExtractTelnet();
        public static IObjectExtractor Current { get { return _Current; } }

        List<string> _DiscardFields = new List<string>(new string[] { "help", "ayuda" });
        List<string> _UserFields = new List<string>(new string[] { "user", "usuario", "login" });
        List<string> _PasswordFields = new List<string>(new string[] { "pass", "key", "credential", "clave" });

        string CleanTelnet(byte[] data, int index, int length)
        {
            for (int x = index, max = index + length; x < max; x++)
            {
                if (data[x] == 0xff)
                {
                    data[x] = 0x00;
                    if (x + 2 < max)
                    {
                        byte n = data[x + 1];
                        if (n >= 0xf0 && n <= 0xff)
                        {
                            data[x + 1] = 0x00;
                            data[x + 2] = 0x00;
                            x += 2;
                        }
                        else
                        {

                        }
                    }
                }
            }

            string ret = Encoding.ASCII.GetString(data, index, length);
            return ret.Trim('\r', '\n', '\0');
        }
        public EExtractorReturn GetObjects(TcpStream stream, out object[] cred)
        {
            if (!stream.IsClossed)
            {
                cred = null;
                if (stream.FirstStream != null && stream.FirstStream.Emisor != ETcpEmisor.Server) return EExtractorReturn.DontRetry;
                return EExtractorReturn.Retry;
            }

            if (stream.ClientLength < 1)
            {
                cred = null;
                return EExtractorReturn.DontRetry;
            }

            bool isTelnet = false;
            string user = null, password = null;

            string nextIs = null;

            TelnetCredential last = null;
            List<TelnetCredential> ret = new List<TelnetCredential>();

            foreach (TcpStreamMessage pack in stream)
            {
                switch (pack.Emisor)
                {
                    case ETcpEmisor.Server:
                        {
                            string serverl = CleanTelnet(pack.Data, 0, pack.Data.Length).ToLowerInvariant();
                            if (string.IsNullOrEmpty(serverl))
                            {
                                isTelnet = true;
                                continue;
                            }

                            // Check
                            bool dontCheckUsers = false;
                            foreach (string uf in _DiscardFields) if (serverl.Contains(uf)) { isTelnet = dontCheckUsers = true; break; }
                            if (!dontCheckUsers)
                            {
                                foreach (string uf in _UserFields) if (serverl.Contains(uf)) { nextIs = "user"; isTelnet = true; break; }
                                foreach (string uf in _PasswordFields) if (serverl.Contains(uf)) { nextIs = "pwd"; isTelnet = true; break; }
                            }
                            break;
                        }
                    case ETcpEmisor.Client:
                        {
                            if (!isTelnet)
                            {
                                cred = null;
                                return EExtractorReturn.DontRetry;
                            }

                            string data = CleanTelnet(pack.Data, 0, pack.Data.Length);
                            if (string.IsNullOrEmpty(data)) continue;

                            switch (nextIs)
                            {
                                case "user":
                                    {
                                        user = data;
                                        break;
                                    }
                                case "pwd":
                                    {
                                        password = data;
                                        nextIs = "valid-check";

                                        last = new TelnetCredential(stream.StartDate, stream.Destination)
                                        {
                                            Password = password,
                                            User = user,
                                            IsValid = false
                                        };
                                        ret.Add(last);
                                        break;
                                    }
                                case "valid-check":
                                    {
                                        if (last != null)
                                        {
                                            last.IsValid = true;
                                            cred = ret.Count == 0 ? null : ret.ToArray();
                                            return cred != null ? EExtractorReturn.True : EExtractorReturn.DontRetry;
                                        }
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        {
                            cred = null;
                            return EExtractorReturn.DontRetry;
                        }
                }
            }

            if (!string.IsNullOrEmpty(user) && password == null)
            {
                last = new TelnetCredential(stream.StartDate, stream.Destination)
                {
                    Password = password,
                    User = user,
                    IsValid = false
                };
                ret.Add(last);
            }

            cred = ret.Count == 0 ? null : ret.ToArray();
            return cred != null ? EExtractorReturn.True : EExtractorReturn.DontRetry;
        }
    }
}