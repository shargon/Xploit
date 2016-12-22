using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using Xploit.Server.Http.Enums;
using Xploit.Server.Http.Interfaces;

namespace Xploit.Server.Http
{
    public class HttpServer : IHttpServer
    {
        long _cache_maxb = 0, _cache_curb = 0;
        public long CacheMaxBytes { get { return _cache_maxb; } set { _cache_maxb = value; } }
        public long CacheCurrentBytes { get { return _cache_curb; } set { _cache_curb = value; } }

        Dictionary<string, HttpCache> _CACHE = new Dictionary<string, HttpCache>();
        public void CacheSet(HttpCache cache)
        {
            string tag = cache.ETag;
            HttpCache ct;
            lock (_CACHE)
            {
                if (_CACHE.TryGetValue(tag, out ct))
                {
                    _cache_curb -= ct.Length;
                    _CACHE[tag] = cache;
                    _cache_curb += cache.Length;
                }
                else
                {
                    _CACHE.Add(tag, cache);
                    _cache_curb += cache.Length;
                }
            }
            if (_cache_curb > _cache_maxb)
            {
                //delete
                List<HttpCache> rem = new List<HttpCache>();
                foreach (HttpCache cx in _CACHE.Values) rem.Add(cx);
                rem.Sort(HttpCache.SortByDate);

                while (_cache_curb > _cache_maxb)
                {
                    HttpCache c = rem[0];
                    rem.RemoveAt(0);
                    CacheDel(c.ETag);
                }
            }
        }
        public HttpCache CacheGet(string etag)
        {
            HttpCache ct;
            if (_CACHE.TryGetValue(etag, out ct)) return ct;
            return null;
        }
        public bool CacheDel(string etag)
        {
            HttpCache ct = CacheGet(etag);
            if (ct == null) return false;
            lock (_CACHE)
            {
                _cache_curb -= ct.Length;
                return _CACHE.Remove(etag);
            }
        }

        ushort _port = 80;
        TcpListener listener = null;
        Dictionary<string, object> _memVariables = new Dictionary<string, object>();
        Dictionary<string, List<HttpProcessor>> _PUSH = new Dictionary<string, List<HttpProcessor>>();
        List<string> TMP_FILES = new List<string>();

        string _sname = "S";
        string _dir_session = "http_session";
        bool is_active = true;

        public const uint MB = 1024 * 1024;
        public virtual bool GetAndPostNamesToLowerCase { get { return true; } }
        public virtual uint MaxPostMultiPart { get { return 500 * MB; } } //500mb
        public virtual uint MaxPost { get { return 10 * MB; } } //5mb
        public virtual bool ProgreesOnAllPost { get { return false; } }//progreso para variables normales de post
        public virtual bool AllowKeepAlive { get { return true; } }
        public virtual bool SessionCheckFakeId { get { return true; } }
        public ushort Port { get { return _port; } }

        public Dictionary<string, object> MemoryVariables { get { return _memVariables; } }

        public string SessionDirectory { get { return _dir_session; } set { _dir_session = value.TrimEnd('\\'); } }
        public string SessionCookieName { get { return _sname; } set { if (!string.IsNullOrEmpty(value)) _sname = value; } }

        public HttpServer() : this(80) { }
        public HttpServer(ushort port) { _port = port; }

        public void Start() { Start(_port); }
        public void Start(ushort port)
        {
            _port = port;
            Stop();

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);

            //Thread thread = new Thread(new ThreadStart(ProcessThread));
            //thread.Name = "HTTP_" + port.ToString();
            //thread.IsBackground = true;
            //thread.Start();
        }
        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            if (!is_active) return;
            try
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient s = listener.EndAcceptTcpClient(ar);
                if (s != null)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(Process));
                    thread.Name = "HTTP_" + s.Client.RemoteEndPoint.ToString();
                    thread.IsBackground = true;
                    thread.Start(s);
                }
            }
            catch { }
            if (!is_active) return;
            try
            {
                listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);
            }
            catch { }
        }/*
        void ProcessThread()
        {
            while (is_active)
            {
                try
                {
                    TcpClient s = listener.AcceptTcpClient();
                    if (s != null)
                    {
                        Thread thread = new Thread(new ParameterizedThreadStart(Process));
                        thread.Name = "HTTP_" + s.Client.RemoteEndPoint.ToString();
                        thread.IsBackground = true;
                        thread.Start(s);
                    }
                    Thread.Sleep(1);
                }
                catch { }
            }
        }*/
        void Process(object cl)
        {
            TcpClient client = (TcpClient)cl;
            if (!OnAccept(client))
            {
                client.Close();
                return;
            }
            HttpProcessor processor = new HttpProcessor(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), client.GetStream(), this, false);
            if (processor.IsSocket)
            {
                Html5SocketMsg msg = null;
                try
                {
                    while ((msg = processor.ReadSocketMsg()) != null)
                    {
                        switch (msg.Type)
                        {
                            case ESocketDataType.ping: { processor.WriteSocketMsg(msg.Data, ESocketDataType.pong); break; }
                            case ESocketDataType.text:
                            case ESocketDataType.binary: { OnSocketMsg(processor, msg); break; }
                            case ESocketDataType.close: return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { processor.WriteSocketMsg(ex.ToString(), ESocketDataType.close); }
                    catch { }
                }
                finally
                {
                    processor.Close();
                    processor.Dispose();
                }
            }
            else
            {
                while (processor.KeepAlive)
                {
                    processor = processor.ReProcess();
                    if (processor == null) return;
                }
            }
            processor.Close();
            processor.Dispose();
        }
        public void Stop()
        {
            if (listener != null) { is_active = false; listener.Stop(); listener = null; }
            _memVariables.Clear();
            lock (_PUSH) { _PUSH.Clear(); }

            if (Directory.Exists(_dir_session))
                foreach (string file in Directory.GetFiles(_dir_session))
                {
                    FileInfo fi = new FileInfo(file);
                    TimeSpan ts = DateTime.Now - fi.LastAccessTime;
                    if (ts.TotalDays > 30) File.Delete(file);
                }
            lock (TMP_FILES)
            {
                foreach (string fl in TMP_FILES) { try { File.Delete(fl); } catch { } }
                TMP_FILES.Clear();
            }
        }

        public virtual void OnPostProgress(HttpPostProgress arg, EHttpPostState end) { }
        public virtual bool OnAccept(TcpClient p) { return true; }
        public virtual void OnRequest(HttpProcessor p) { }
        public virtual void OnRequestSocket(HttpProcessor p) { }
        public virtual void OnSocketMsg(HttpProcessor p, Html5SocketMsg msg) { }

        public static string Md5(string cad)
        {
            if (string.IsNullOrEmpty(cad)) return "";
            return Md5(Encoding.UTF8.GetBytes(cad));
        }
        public static string Md5(byte[] bs)
        {
            if (bs == null) return "";

            MD5CryptoServiceProvider cmd5 = new MD5CryptoServiceProvider();
            bs = cmd5.ComputeHash(bs);
            StringBuilder s = new StringBuilder();
            foreach (byte b in bs) s.Append(b.ToString("x2").ToLower());
            return s.ToString();
        }
        public static byte[] Gzip(byte[] buff, int index, int count, bool comprees)
        {
            if (buff == null || buff.Length == 0) return new byte[] { };
            MemoryStream ms = new MemoryStream();
            if (comprees)
            {
                GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, false);
                gzip.Write(buff, index, count);
                gzip.Close(); gzip.Dispose();
            }
            else
            {
                MemoryStream msg = new MemoryStream(buff, index, count);
                GZipStream gzip = new GZipStream(msg, CompressionMode.Decompress, false);

                int n = 0;
                byte[] bytes = new byte[10240];
                while ((n = gzip.Read(bytes, 0, bytes.Length)) != 0) ms.Write(bytes, 0, n);

                gzip.Close(); gzip.Dispose();
            }
            buff = ms.ToArray();
            ms.Close(); ms.Dispose();
            return buff;
        }

        public static string UrlEncode(string cad) { return HttpUtility.UrlEncode(cad); }
        public static string UrlDecode(string cad) { return HttpUtility.UrlDecode(cad); }
        public static string HtmlEncode(string cad) { return HttpUtility.HtmlEncode(cad); }
        public static string HtmlDecode(string cad) { return HttpUtility.HtmlDecode(cad); }

        //UDF
        //instalacion
        //http://www.mysqludf.org/lib_mysqludf_sys/index.php
        //compilacion
        //http://dev.mysql.com/doc/refman/5.0/es/udf-compiling.html
        //codigo udp
        //http://bazaar.launchpad.net/~fallenpegasus/mysql-udf-sendudp/trunk/view/head:/udf_sendudp.c
        //ejemplo
        //http://bugs.mysql.com/bug.php?id=28745
        //https://dev.mysql.com/doc/refman/5.5/en/udf-compiling.html
        //codeproject
        //http://www.codeproject.com/Articles/15643/MySQL-User-Defined-Functions
        //http://rpbouman.blogspot.com.es/2007/09/creating-mysql-udfs-with-microsoft.html

        public int PushMessage(string push_code, string msg) { return PushMessage(push_code, msg, null, true, null); }
        public int PushMessage(string push_code, string msg, string only_for_this_session, bool only_one_per_session, string msg_other_session)
        {
            int dv = 0;
            lock (_PUSH)
            {
                msg = "#PUSH#" + push_code + "#" + msg;

                bool other = !string.IsNullOrEmpty(msg_other_session);
                if (other) msg_other_session = "#PUSH#" + push_code + "#" + msg_other_session;

                bool only_ses = !string.IsNullOrEmpty(only_for_this_session);
                List<HttpProcessor> ls = null;
                if (_PUSH.TryGetValue(push_code, out ls))
                {
                    lock (ls)
                    {
                        for (int x = ls.Count - 1; x >= 0; x--)
                        {
                            HttpProcessor http = ls[x];

                            if (only_ses && only_for_this_session != http.SessionID)
                            {
                                if (other) http.WriteSocketMsg(msg_other_session);
                                continue;
                            }
                            http.WriteSocketMsg(msg);
                            dv++;

                            if (only_one_per_session && only_ses)
                            {
                                if (!other) break;
                                msg = msg_other_session;
                            }
                        }
                    }
                }
            }

            return dv;
        }

        public bool PushAdd(HttpProcessor http, string push_code)
        {
            lock (_PUSH)
            {
                List<HttpProcessor> ls = null;
                if (_PUSH.TryGetValue(push_code, out ls))
                {
                    if (!ls.Contains(http)) { ls.Add(http); return true; }
                }
                else
                {
                    ls = new List<HttpProcessor>();
                    ls.Add(http);
                    _PUSH.Add(push_code, ls);
                    return true;
                }
            }
            return false;
        }
        public void PushDel(HttpProcessor http, string push_code)
        {
            lock (_PUSH)
            {
                List<HttpProcessor> ls = null;
                if (_PUSH.TryGetValue(push_code, out ls))
                {
                    ls.Remove(http);
                    if (ls.Count == 0) _PUSH.Remove(push_code);
                }
            }
        }

        public string TempFileGet()
        {
            string tmp = Path.GetTempFileName();
            lock (TMP_FILES) { TMP_FILES.Add(tmp); }
            return tmp;
        }
        public bool TempFileDel(string file)
        {
            try
            {
                lock (TMP_FILES) { TMP_FILES.Remove(file); }
                File.Delete(file);
                return true;
            }
            catch { return false; }
        }
    }
}