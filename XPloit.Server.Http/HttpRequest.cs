using System;
using System.Collections.Generic;
using System.IO;
using XPloit.Server.Http.Enums;

namespace XPloit.Server.Http
{
    public class HttpRequest : IDisposable
    {
        HttpFile[] _files = new HttpFile[] { };
        public HttpFile[] Files { get { return _files; } }

        bool _VarsToLowerCase;
        HttpHost _host;
        string http_url = "", _IP = "";
        string http_protocol_version = "";
        EHttpMethod http_method = EHttpMethod.Unknown;
        HttpAuth _auth;
        Dictionary<string, string> _header;
        Dictionary<string, string> _get = new Dictionary<string, string>();
        Dictionary<string, string> _post = new Dictionary<string, string>();
        EBrowser _br = EBrowser.Unknown;
        LocationInfo _loc;
        ESO _so = ESO.Unknown;
        HttpPath _http_path;

        public bool IsSocket { get { return http_method == EHttpMethod.SOCKET; } }
        public ESO SO { get { return _so; } }
        public EBrowser Browser { get { return _br; } }
        public HttpHost Host { get { return _host; } }
        public EHttpMethod HttpMethod { get { return http_method; } }
        public string Url { get { return http_url; } }
        public HttpPath Path
        {
            get
            {
                if (_http_path == null) _http_path = new HttpPath(http_url);
                return _http_path;
            }
        }
        public string ProtocolVersion { get { return http_protocol_version; } }
        public HttpAuth Autentication { get { return _auth; } }
        public string Ip { get { return _IP; } }
        public LocationInfo LocationInfo { get { if (_loc == null) _loc = new LocationInfo(_IP); return _loc; } }

        public Dictionary<string, string> Headers { get { return _header; } }
        public Dictionary<string, string> GET { get { return _get; } }
        public Dictionary<string, string> POST { get { return _post; } }

        public string this[string varname] { get { return this[varname, null]; } }
        public string this[string varname, string def]
        {
            get
            {
                string dv = def;

                if (_get.TryGetValue(varname, out dv)) return dv;
                if (_post.TryGetValue(varname, out dv)) return dv;

                return def;
            }
        }
        public string this[string varname, bool get]
        {
            get { return this[varname, get, null]; }
            set { this[varname, get, null] = value; }
        }
        public string this[string varname, bool get, string def]
        {
            get
            {
                if (_VarsToLowerCase) varname = varname.ToLowerInvariant();

                string dv = def;
                if (get)
                {
                    if (_get.TryGetValue(varname, out dv)) return dv;
                }
                else
                {
                    if (_post.TryGetValue(varname, out dv)) return dv;
                }
                return def;
            }
            set
            {
                if (_VarsToLowerCase) varname = varname.ToLowerInvariant();

                if (get)
                {
                    if (value == null) { _get.Remove(varname); return; }
                    if (_get.ContainsKey(varname)) _get[varname] = value;
                    else _get.Add(varname, value);
                }
                else
                {
                    if (value == null) { _post.Remove(varname); return; }
                    if (_post.ContainsKey(varname)) _post[varname] = value;
                    else _post.Add(varname, value);
                }
            }
        }

        internal void SetFiles(HttpFile[] httpFiles) { _files = httpFiles; }
        public HttpRequest(string request, string host, string ip, Dictionary<string, string> header, HttpAuth auth, ESO so, EBrowser br, bool is_socket, bool varsToLowerCase)
        {
            if (header == null)
            {
                http_method = EHttpMethod.Unknown;
                return;
            }

            int ixa = request.IndexOf(' ');
            if (ixa <= -1)
            {
                http_method = EHttpMethod.Unknown;
                return;
            }
            int ixb = request.LastIndexOf(' ');
            if (ixb <= -1 || ixa == ixb)
            {
                http_method = EHttpMethod.Unknown;
                return;
            }

            _VarsToLowerCase = varsToLowerCase;
            _IP = ip;
            _so = so;
            _br = br;
            _header = header;
            _auth = auth;
            _host = new HttpHost(host);

            switch (request.Substring(0, ixa).ToUpper())
            {
                case "HEAD": http_method = EHttpMethod.HEAD; break;
                case "GET": http_method = EHttpMethod.GET; break;
                case "POST": http_method = EHttpMethod.POST; break;
                default: http_method = EHttpMethod.Unknown; break;
            }

            if (is_socket) http_method = EHttpMethod.SOCKET;

            http_url = request.Substring(ixa + 1, ixb - ixa - 1).Trim('/');
            http_protocol_version = request.Substring(ixb + 1);

            int ix_h = http_url.IndexOf('?');
            if (ix_h != -1)
            {
                foreach (string key in http_url.Remove(0, ix_h + 1).Split('&'))
                    try
                    {
                        string iz, dr;
                        HttpProcessor.SeparaEnDos(key, '=', out iz, out dr);
                        if (varsToLowerCase) iz = iz.ToLowerInvariant();

                        _get.Add(HttpServer.UrlDecode(iz), HttpServer.UrlDecode(dr));
                    }
                    catch { }
                http_url = http_url.Substring(0, ix_h);
            }
        }
        internal void AddPost(string name, string var)
        {
            if (_VarsToLowerCase) name = name.ToLowerInvariant();
            try { _post.Add(name, var); } catch { }
        }
        internal void Clear()
        {
            if (_get != null) _get.Clear();
            if (_post != null) _post.Clear();
            if (_header != null) _header.Clear();

            if (_files != null)
            {
                foreach (HttpFile fl in _files)
                    try { if (File.Exists(fl.FileTemp)) File.Delete(fl.FileTemp); }
                    catch { }
                _files = null;
            }
        }

        public bool TryGetHeader(string name, out string value) { return _header.TryGetValue(name, out value); }
        public bool TryGetVariable(string name, out string value, bool get)
        {
            if (!get) return _post.TryGetValue(name, out value);
            return _get.TryGetValue(name, out value);
        }
        public void Dispose() { Clear(); _header = null; _get = null; _post = null; }

        public bool HasBoolIn(bool get, string variable) { return HasBoolIn(get, variable, false); }
        public bool HasBoolIn(bool get, string variable, bool def)
        {
            string dv = this[variable, get];
            if (!string.IsNullOrEmpty(dv))
            {
                dv = dv.ToUpper();
                return dv == "1" || dv == "ON" || dv == "TRUE" || dv == "YES" || dv == "SI" || dv == "S" || dv == "Y";
            }
            return def;
        }
        public int HasIntIn(bool get, string variable, int def)
        {
            string dv = this[variable, get];
            if (!string.IsNullOrEmpty(dv))
            {
                int o = def;
                if (int.TryParse(dv, out o)) return o;
            }
            return def;
        }
        public int HasIntIn(string variable, int def)
        {
            string dv = this[variable];
            if (!string.IsNullOrEmpty(dv))
            {
                int o = def;
                if (int.TryParse(dv, out o)) return o;
            }
            return def;
        }
    }
}