using System;
using Xploit.Server.Http.Enums;
using Xploit.Server.Http.Interfaces;

namespace Xploit.Server.Http
{
    public class HttpPostProgress
    {
        object _tag = null;
        bool _cancel = false, _mpart = false;
        int _max = 0, _cur = 0;
        string _ses = "", _ip = "";
        HttpProcessor _proc = null;
        HttpRequest _req = null;
        IHttpSession _sescl = null;

        public string IP { get { return _ip; } }
        public string SessionID { get { return _ses; } }
        public int ContentMax { get { return _max; } }
        public int ContentValue { get { return _cur; } }
        public HttpRequest Request { get { return _req; } }
        public double ContentProgress { get { return (_cur * 100.0) / _max; } }
        public double Speed { get { return _bytessec; } }
        public object Tag { get { return _tag; } }
        public bool IsMultiPart { get { return _mpart; } }

        public IHttpSession Session
        {
            get
            {
                if (_sescl == null && !string.IsNullOrEmpty(_ses))
                    _sescl = _proc.StartSession();
                return _sescl;
            }
        }

        int last_va = 0;
        double _bytessec = 0;
        IHttpServer _sender = null;
        DateTime last = DateTime.MinValue;

        public HttpPostProgress(HttpProcessor proc, IHttpServer srv, string ip, HttpRequest req, string session, int max, bool mpart)
        {
            _proc = proc;
            _sender = srv;
            _req = req;
            _ip = ip;
            _ses = session;
            _max = max;
            _mpart = mpart;
        }

        internal void UpdateValue(int val, bool raise_event)
        {
            _cur += val;
            if (last == DateTime.MinValue) last = DateTime.Now;
            else
            {
                DateTime now = DateTime.Now;
                TimeSpan ts = now - last;
                double tss = ts.TotalSeconds;
                if (tss >= 1)
                {
                    _bytessec = (_cur - last_va) / tss;
                    last = now;
                    last_va = _cur;
                }
            }

            if (raise_event) { _sender.OnPostProgress(this, EHttpPostState.Progress); }
        }
        public bool Cancel { get { return _cancel; } set { _cancel = value; } }
    }
}