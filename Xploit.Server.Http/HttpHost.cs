using System.Net;

namespace Xploit.Server.Http
{
    public class HttpHost
    {
        string _subdomain = "", _domain = "";
        int port = 80;

        public string Subdomain { get { return _subdomain; } }
        public string Domain { get { return _domain; } }
        public int Port { get { return port; } }

        public HttpHost() { _subdomain = ""; _domain = "localhost"; }
        public HttpHost(string host)
        {
            if (string.IsNullOrEmpty(host)) return;

            int ix = host.IndexOf(':');
            if (ix != -1)
            {
                string pt = host.Remove(0, ix + 1);
                _domain = host.Substring(0, ix);
                if (!int.TryParse(pt, out port)) port = 80;
            }
            else _domain = host;

            IPAddress ipp = null;
            if (!IPAddress.TryParse(_domain, out ipp))
            {
                ix = _domain.IndexOf('.');
                if (ix != -1 && ix != _domain.LastIndexOf('.'))
                {
                    _subdomain = _domain.Substring(0, ix);
                    _domain = _domain.Remove(0, ix + 1);
                }
            }
        }

        public override string ToString() { return ToString(true); }
        public string ToString(bool with_port)
        {
            string dv = _subdomain;
            if (dv != "") dv += ".";
            dv += _domain;
            if (with_port && port != 80) dv += ":80";
            return dv;
        }
    }
}