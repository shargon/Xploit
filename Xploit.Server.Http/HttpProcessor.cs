using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Xploit.Server.Http.Enums;
using Xploit.Server.Http.Interfaces;

namespace Xploit.Server.Http
{
    public class HttpProcessor : IDisposable
    {
        IHttpServer _Server;
        IHttpSession ses;
        TcpClient socket;
        Stream stream;
        static Encoding codec_iso = Encoding.GetEncoding("ISO-8859-1");
        Encoding codec_write = codec_iso, codec_post = codec_iso;

        string _sessionID, _IP;
        const int BUF_SIZE = 8192;
        bool _is_socket = false, _ReadOnly = false;
        bool is_write_headers = false, _gzip = false, _keep_alive = false, _send_length = false;
        DateTime date = DateTime.Now;

        HttpRequest _req;
        HttpPostProgress post_pg;
        StringBuilder send_headers;
        Dictionary<string, HttpCookie> httpCookie = new Dictionary<string, HttpCookie>();

        public Encoding WriteEncoding { get { return codec_write; } set { codec_write = value; } }
        public Encoding ReadEncoding { get { return codec_post; } }

        public DateTime Date { get { return date; } }
        public bool AllowGzip { get { return _gzip; } }
        public bool KeepAlive { get { return _keep_alive; } }
        public bool IsSocket { get { return _is_socket; } }
        public string SessionID { get { return _sessionID; } }

        public IHttpSession Session { get { if (ses == null) return StartSession(); return ses; } }
        public HttpRequest Request { get { return _req; } }

        public HttpProcessor(string ip, Stream s, IHttpServer serv, bool readOnly)
        {
            _IP = ip;
            _Server = serv;
            stream = s;
            _ReadOnly = readOnly;
            IProcess();
        }
        void IProcess()
        {
            try
            {
                if (!parseRequest() || _req == null)
                {
                    IProcessError(new Exception("Request error"));
                    return;
                }

                switch (_req.HttpMethod)
                {
                    case EHttpMethod.Unknown: return;
                    case EHttpMethod.POST: parsePost(); break;
                    case EHttpMethod.SOCKET: parseSocket(); _is_socket = true; return;
                }

                WriteHeader("Date", DateTime.Now.ToUniversalTime().ToString("R"));
                WriteHeader("Server", "MAIS");

                if (_Server != null)
                {
                    if (IsSocket) _Server.OnRequestSocket(this);
                    else _Server.OnRequest(this);
                }
            }
            catch (Exception e) { IProcessError(e); }
            finally
            {
                if (stream != null) stream.Flush();
                if (post_pg != null && _Server != null) _Server.OnPostProgress(post_pg, EHttpPostState.End);
            }
        }
        void IProcessError(Exception error)
        {
            try
            {
                if (_is_socket)
                {
                    WriteSocketMsg(error.Message, ESocketDataType.close);
                    Close();
                }
                else ResultServerError(error);
            }
            catch { }
            _keep_alive = false;
        }
        #region sockets
        void parseSocket()
        {
            string socket_key = "", socket_ext = "", socket_ver = "";
            if (!_req.TryGetHeader("Sec-WebSocket-Key", out socket_key) ||
                !_req.TryGetHeader("Sec-WebSocket-Version", out socket_ver) ||
                !_req.TryGetHeader("Sec-WebSocket-Extensions", out socket_ext))
                return;

            string socket_protocol = "";
            if (!Request.TryGetHeader("Sec-WebSocket-Protocol", out socket_protocol)) socket_protocol = "";
            if (!string.IsNullOrEmpty(socket_protocol)) _sessionID = socket_protocol;

            StringBuilder send = new StringBuilder();
            send.AppendLine("HTTP/1.1 101 Switching Protocols");
            WriteHeader("Upgrade", "websocket");
            WriteHeader("Connection", "Upgrade");
            WriteHeader("Sec-WebSocket-Accept", Html5SocketMsg.ComputeWebSocketHandshakeSecurityHash09(socket_key));
            //WriteHeader("Sec-WebSocket-Protocol", "chat");
            is_write_headers = true;

            if (send_headers != null) send.Append(send_headers.ToString());
            send.AppendLine();

            Write(send.ToString());

            //WriteSocketMsg("Mensaje de Prueba 1");
            //WriteSocketMsg("Este es un mensaje de prueba 2");
            //WriteSocketMsg(LIB.Replicate("Este es un mensaje de prueba 2", 7));
            //WriteSocketMsg(LIB.Replicate("Este es un mensaje de prueba 2", 2200));
        }

        public void WriteSocketMsg(string msg) { WriteSocketMsg(msg, ESocketDataType.text); }
        public void WriteSocketMsg(string msg, ESocketDataType type)
        {
            byte[] payload = Encoding.UTF8.GetBytes(msg);
            WriteSocketMsg(payload, type);
        }
        public void WriteSocketMsg(byte[] payload, ESocketDataType type)
        {
            byte[] dmp = Html5SocketMsg.hybi10Encode(payload, type);
            stream.Write(dmp, 0, dmp.Length);
        }
        public Html5SocketMsg ReadSocketMsg() { return Html5SocketMsg.GetMsg(codec_post, stream); }

        #endregion
        void parsePost()
        {
            string vcon = null;
            if (!_req.TryGetHeader("Content-Length", out vcon)) return;
            int content_len = int.Parse(vcon);

            string contenttype = null;
            if (_req.TryGetHeader("Content-Type", out contenttype))
            {
                string lou = contenttype.ToLower();
                int ix = lou.IndexOf("charset=");
                if (ix != -1)
                {
                    string charset = contenttype.Remove(0, ix + 8);
                    ix = charset.IndexOf(';');
                    if (ix != -1) charset = charset.Substring(0, ix);

                    codec_post = Encoding.GetEncoding(charset);
                }

                bool file = lou.StartsWith("multipart/form-data");

                uint MAX_POST = file ? _Server.MaxPostMultiPart : _Server.MaxPost;
                if (MAX_POST >= 0 && content_len > MAX_POST)
                    throw new Exception(string.Format("POST Content-Length({0}) too big for this server", content_len));

                if (file || _Server.ProgreesOnAllPost)
                {
                    //progreso
                    HttpCookie ck = null;
                    if (httpCookie.TryGetValue(_Server.SessionCookieName, out ck)) _sessionID = ck.Value;

                    post_pg = new HttpPostProgress(this, _Server, _IP, _req, _sessionID, content_len, file);
                    _Server.OnPostProgress(post_pg, EHttpPostState.Start);
                    if (post_pg.Cancel) throw new Exception("Client disconnected");
                }
                if (file)
                {
                    //multipart post
                    HttpMultiPartParser mp = new HttpMultiPartParser(codec_post, contenttype, BUF_SIZE);
                    mp.Process(stream, content_len, post_pg);
                    if (mp.Files != null) _req.SetFiles(mp.Files);

                    if (mp.HasError) throw (new Exception("Error in Post"));
                    foreach (HttpMultiPartParser.Var v1 in mp.Vars) _req.AddPost(v1.Name, v1.Value);
                }
                else
                {
                    //variables post
                    byte[] buf = new byte[BUF_SIZE];
                    int to_read = content_len, numread = 0;

                    MemoryStream ms = new MemoryStream();
                    try
                    {
                        while (to_read > 0)
                        {
                            numread = stream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                            if (post_pg != null) //progreso
                            {
                                post_pg.UpdateValue(numread, true);
                                if (post_pg.Cancel) throw new Exception("Client disconnected");
                            }
                            if (numread == 0)
                            {
                                if (to_read == 0) break;
                                else { throw new Exception("Client disconnected during Post"); }
                            }
                            to_read -= numread;
                            ms.Write(buf, 0, numread);
                        }
                    }
                    catch (Exception ex) { ms.Close(); ms.Dispose(); throw (ex); }

                    if (ms != null)
                    {
                        byte[] dv = ms.ToArray();
                        ms.Close(); ms.Dispose();

                        string name, val;
                        foreach (string s in codec_post.GetString(dv).Split('&'))
                        {
                            SeparaEnDos(HttpUtility.UrlDecode(s), '=', out name, out val);
                            _req.AddPost(name, val);
                        }
                    }
                }
            }
        }

        bool parseRequest()
        {
            string request;
            if (!streamReadLine(stream, out request)) return false;

            string host = "";
            string line;
            bool issocket = false;

            ESO so = ESO.Unknown;
            EBrowser br = EBrowser.Unknown;
            HttpAuth aut = null;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            while (streamReadLine(stream, out line))
            {
                if (string.IsNullOrEmpty(line)) { break; }

                string name, val;
                SeparaEnDos(line, ':', out name, out val);
                string lname = name.ToLower();
                val = val.TrimStart(' ');

                switch (lname)
                {
                    case "connection": { if (_Server != null && _Server.AllowKeepAlive) _keep_alive = val.ToLower().Contains("keep-alive"); break; }
                    case "accept-encoding": { _gzip = val.ToLower().Contains("gzip"); break; }
                    case "host": { host = val; break; }
                    case "upgrade": { issocket = val == "websocket"; break; }
                    case "cookie":
                        {
                            string nc, vc;
                            foreach (string s in val.Split(';'))
                            {
                                SeparaEnDos(s, '=', out nc, out vc);
                                nc = HttpUtility.UrlDecode(nc.TrimStart(' '));
                                vc = HttpUtility.UrlDecode(vc);
                                try { httpCookie.Add(nc, new HttpCookie(nc, vc)); }
                                catch { }
                            }
                            break;
                        }
                    case "authorization":
                        {
                            string tp, vl;
                            SeparaEnDos(val, ' ', out tp, out vl);
                            if (tp.ToLower() == "basic")
                            {
                                byte[] dv = Convert.FromBase64String(vl);
                                vl = codec_iso.GetString(dv);
                                SeparaEnDos(vl, ':', out tp, out vl);
                                aut = new HttpAuth(tp, vl);
                            }
                            break;
                        }
                    case "user-agent":
                        {
                            string tp, vl;
                            SeparaEnDos(val, ' ', out tp, out vl);
                            //SO
                            vl = vl.ToLower();
                            if (vl.Contains("windows")) so = ESO.Windows;
                            else
                            {
                                if (vl.Contains("android")) so = ESO.Android;
                                else
                                {
                                    if (vl.Contains("iphone")) so = ESO.Iphone;
                                    else
                                    {
                                        if (vl.Contains("ipad")) so = ESO.Ipad;
                                        else
                                        {
                                            if (vl.Contains("ipod")) so = ESO.Ipod;
                                            else
                                            {
                                                if (vl.Contains("mac")) so = ESO.Mac;
                                                else
                                                {
                                                    if (vl.Contains("linux")) so = ESO.Linux;
                                                    else
                                                    {
                                                        if (vl.Contains("x11")) so = ESO.Unix;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //Browser
                            if (vl.Contains("chrome/")) br = EBrowser.Chrome;
                            else
                            {
                                if (vl.Contains("firefox")) br = EBrowser.FireFox;
                                else
                                {
                                    if (vl.Contains("msie")) br = EBrowser.IExplorer;
                                    else
                                    {
                                        if (vl.Contains("konqueror")) br = EBrowser.Konqueror;
                                        else
                                        {
                                            if (vl.Contains("opera/")) br = EBrowser.Opera;
                                            else
                                            {
                                                if (vl.Contains("safari/")) br = EBrowser.Safari;
                                                else
                                                {
                                                    tp = tp.ToLower();
                                                    if (tp.Contains("dalvik/")) br = EBrowser.Dalvik;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }

                headers.Add(name, val);
            }

            _req = new HttpRequest(request, host, _IP, headers, aut, so, br, issocket, _Server.GetAndPostNamesToLowerCase);
            return true;
        }

        //WRITE
        public bool InCache(string etag, bool raise_not_modified)
        {
            string tiene = null;
            //etag = "\"" + etag + "\"";
            //WriteHeader("Expires", now.AddDays(7).ToString("R"));

            //WriteHeader("Cache-Control", "private, max-age=31536000");
            WriteHeader("Cache-Control", "private");
            WriteHeader("Etag", etag);
            //WriteHeader("Expires", DateTime.Now.ToUniversalTime().AddYears(1).ToString("R"));

            bool dv = (_req.TryGetHeader("If-None-Match", out tiene) && tiene == etag);
            if (dv && raise_not_modified) ResultNotModified();
            return dv;
        }

        public void ResultOK() { ResultOK(null, 0, 0, "text/html", false); }
        public void ResultOK(string html, bool inGzip) { ResultOK(html, "text/html", inGzip); }
        public void ResultOK(string html, string contentType, bool inGzip)
        {
            int l = 0;
            byte[] bhtml = null;
            if (!string.IsNullOrEmpty(html)) { bhtml = codec_write.GetBytes(html); l = bhtml.Length; }

            ResultOK(bhtml, 0, l, contentType, inGzip);
        }
        public void ResultOK(byte[] html, string contentType, bool inGzip) { ResultOK(html, 0, html.Length, contentType, inGzip); }
        public void ResultOK(byte[] html, int index, int length, string contentType, bool inGzip)
        {
            if (inGzip)
            {
                if (html != null)
                {
                    html = HttpServer.Gzip(html, index, length, true);
                    index = 0;
                    length = html.Length;
                    WriteHeader("Content-Length", length.ToString());
                }
                WriteHeader("Content-Encoding", "gzip");
            }
            else
            {
                if (html != null) WriteHeader("Content-Length", length.ToString());
            }
            WriteHeader("Content-Type", contentType + ";charset=" + codec_write.WebName);
            //WriteHeader("Content-Type", contentType);
            WriteResult("200 OK");

            if (html != null) Write(html, index, length);
        }

        public void ResultNotModified() { WriteResult("304 Not Modified"); }
        public void ResultNotFound() { WriteResult("404 File not found"); }
        public void ResultNotAuthorized(string title)
        {
            WriteHeader("WWW-Authenticate", "Basic realm=\"" + title + "\"");
            WriteResult("401 Not Authorized");
        }
        public void ResultDownload(Stream fs, string file_name, string content_type)
        {
            //cuidado con el file_name, o puede fallar
            WriteHeader("Content-Description", "File Transfer");
            WriteHeader("Content-Disposition", "attachment; filename=" + file_name);
            WriteHeader("Content-Type", content_type);
            WriteHeader("Content-Transfer-Encoding", "binary");
            WriteHeader("Content-Length", fs.Length.ToString());
            WriteResult("200 OK");

            Write(fs, false);
        }
        public void ResultServerError(Exception e)
        {
            WriteResult("500 Internal Error");

            string error = e.ToString();
            error = "<font style='color:red'><b>ERROR</b></font><hr>" + HttpServer.HtmlEncode(error).Replace(Environment.NewLine, "<br>").Replace(" ", "&nbsp;");
            Write(error);
        }

        void WriteResult(string res)
        {
            is_write_headers = true;
            StringBuilder send = new StringBuilder();
            send.AppendLine("HTTP/1.0 " + res);

            if (send_headers != null) send.Append(send_headers.ToString());
            if (_keep_alive && _send_length) send.AppendLine("Connection: keep-alive");
            else { _keep_alive = false; send.AppendLine("Connection: close"); }

            send.AppendLine();

            Write(send.ToString());
        }

        public IHttpSession StartSession() { return StartSession(true); }
        IHttpSession StartSession(bool allow_create)
        {
            if (ses != null) return ses;

            HttpCookie ck = null;
            if (!httpCookie.TryGetValue(_Server.SessionCookieName, out ck))
            {
                //no tiene iniciada la sesion por cookie
                if (!allow_create) return null;

                _sessionID = GetNewSessionID();
                SetCookie(new HttpCookie(_Server.SessionCookieName, _sessionID));
            }
            else
            {
                //aceptarla
                if (!isFakeID(ck.Value)) _sessionID = ck.Value;
            }

            ses = new HttpSessionFile(_Server.SessionDirectory, _sessionID);
            return ses;
        }
        bool isFakeID(string id)
        {
            if (!_Server.SessionCheckFakeId) return false;

            string browser = "";
            if (!_req.Headers.TryGetValue("User-Agent", out browser)) browser = "?";

            string cad = HttpServer.Md5("BR=" + browser);
            return !id.StartsWith(cad);
        }
        string GetNewSessionID()
        {
            string browser = "";
            if (!_req.Headers.TryGetValue("User-Agent", out browser)) browser = "?";

            return HttpServer.Md5("BR=" + browser) + HttpServer.Md5("ID=" + _req.Ip + DateTime.Now.Ticks.ToString());
        }

        public void SetCookie(HttpCookie cok)
        {
            string name = HttpUtility.UrlEncode(cok.Name);
            string val = HttpUtility.UrlEncode(cok.Value);
            //Set-Cookie: LAST_LANG=es; expires=Tue, 13-Jan-2015 07:39:29 GMT; Max-Age=31536000; path=/; domain=.php.net
            WriteHeader("Set-Cookie", name + "=" + val + "; path=/");
        }
        public void WriteHeader(string name, string value)
        {
            if (_ReadOnly) return;
            if (is_write_headers) throw (new Exception("Headers already sent"));

            if (name == "Content-Length") _send_length = true;
            if (send_headers == null) send_headers = new StringBuilder();
            send_headers.AppendLine(name + ": " + value);
        }

        void Write(string text) { byte[] bx = codec_write.GetBytes(text); Write(bx, 0, bx.Length); }
        void Write(byte[] bx) { Write(bx, 0, bx.Length); }
        void Write(byte[] bx, int index, int lg)
        {
            if (_ReadOnly) return;
            stream.Write(bx, index, lg);
        }

        void Write(Stream stream, bool dispose)
        {
            if (_ReadOnly) return;
            //meter parametro para aceptar o no rangos
            int le = 0;
            byte[] bf = new byte[BUF_SIZE];
            while ((le = stream.Read(bf, 0, BUF_SIZE)) > 0) Write(bf, 0, le);

            if (dispose) { stream.Close(); stream.Dispose(); }
        }

        //STATIC
        internal static void SeparaEnDos(string palabra, char sep, out string izq, out string drc)
        {
            int fi = string.IsNullOrEmpty(palabra) ? -1 : palabra.IndexOf(sep);
            if (fi == -1) { izq = palabra; drc = ""; return; }

            izq = palabra.Substring(0, fi);
            drc = palabra.Substring(fi + 1, palabra.Length - fi - 1);
        }
        static bool streamReadLine(Stream sr, out string ret)
        {
            int next_char;
            StringBuilder data = new StringBuilder();
            while (true)
            {
                next_char = sr.ReadByte();
                if (next_char == -1)
                {
                    ret = null;
                    return false;
                }

                if (next_char == '\n') break;
                if (next_char == '\r') continue;

                data.Append((char)next_char);
            }
            ret = data.ToString();
            return true;
        }

        public void Close() { Close(false); }
        void Close(bool only_free)//para keep-alive
        {
            if (_is_socket)
            {
                try { WriteSocketMsg("Kill", ESocketDataType.close); }
                catch { }
            }
            if (stream != null)
            {
                if (!only_free) { stream.Close(); stream.Dispose(); }
            }
            if (socket != null)
            {
                if (!only_free) { socket.Close(); }
                socket = null;
            }

            if (send_headers != null) send_headers.Clear();
            if (_req != null)
            {
                _req.Clear();
                if (!only_free) _req.Dispose();
            }
            httpCookie.Clear();
            if (ses != null) ses.Clear();

            lock (pushes)
            {
                if (pushes.Count > 0)
                {
                    if (_Server != null)
                        foreach (string s in pushes)
                            _Server.PushDel(this, s);

                    pushes.Clear();
                }
            }
        }

        List<string> pushes = new List<string>();
        public void PushDel(string push_code)
        {
            lock (pushes)
            {
                _Server.PushDel(this, push_code);
                pushes.Remove(push_code);
            }
        }
        public void PushAdd(string push_code)
        {
            lock (pushes)
            {
                if (_Server.PushAdd(this, push_code))
                    pushes.Add(push_code);
            }
        }

        internal HttpProcessor ReProcess()
        {
            TcpClient s = socket;
            IHttpServer h = _Server;
            Close(true);

            try { if (!s.Connected || !s.Client.Connected) { Close(); return null; } }
            catch { Close(); return null; }

            return new HttpProcessor(_IP, stream, h, _ReadOnly);
        }
        public void Dispose() { Close(false); }
    }
}