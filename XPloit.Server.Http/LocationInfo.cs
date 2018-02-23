using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace XPloit.Server.Http
{
    public class LocationInfo
    {
        string _ip = "", cn = "", cc = "", ct = "";
        float lat = 0, lon = 0;

        public string Ip { get { return _ip; } }
        public float Latitude { get { return lat; } }
        public float Longitude { get { return lon; } }
        public string CountryName { get { return cn; } }
        public string CountryCode { get { return cc; } }
        public string City { get { return ct; } }
        public string FlagUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(cc)) return "http://api.hostip.info/images/flags/" + cc.ToLower() + ".gif";
                return "";
            }
        }

        public LocationInfo(string ipaddress) : this(ipaddress, false) { }
        public LocationInfo(string ipaddress, bool get) { _ip = ipaddress.Trim(); if (get) GetLocationInfo(); }

        static Dictionary<string, LocationInfo> cachedIps = new Dictionary<string, LocationInfo>();
        public bool GetLocationInfo()
        {
            if (string.IsNullOrEmpty(_ip) || _ip == "127.0.0.1") return false;

            try
            {
                IPAddress ipp = IPAddress.Parse(_ip);
                if (!cachedIps.ContainsKey(_ip))
                {
                    string r;
                    using (WebClient w = new WebClient())
                        r = w.DownloadString("http://api.hostip.info/get_html.php?ip=" + _ip + "&position=true");

                    foreach (string s in r.Replace("\r", "").Split('\n'))
                    {
                        string i = null, d = null;
                        HttpUtilityEx.SeparaEnDos(s, ':', out i, out d);
                        switch (i)
                        {
                            case "Country":
                                {
                                    HttpUtilityEx.SeparaEnDos(d, '(', out i, out d);
                                    cn = i.Trim();
                                    cc = d.Replace(")", "");
                                    break;
                                }
                            case "City":
                                {
                                    d = d.Trim();
                                    if (d == "(Unknown city)") ct = "";
                                    else ct = d;
                                    break;
                                }
                            case "Latitude":
                                {
                                    if (!string.IsNullOrEmpty(d.Trim()))
                                        lat = float.Parse(d, CultureInfo.CreateSpecificCulture("en-US"));
                                    break;
                                }
                            case "Longitude":
                                {
                                    if (!string.IsNullOrEmpty(d.Trim()))
                                        lon = float.Parse(d, CultureInfo.CreateSpecificCulture("en-US"));
                                    break;
                                }
                        }
                    }

                    if (cachedIps.Count > 1000) cachedIps.Clear();
                    cachedIps.Add(_ip, this);
                }
                else
                {
                    LocationInfo li = cachedIps[_ip];
                    this.lon = li.lon;
                    this.lat = li.lat;
                    this.cn = li.cn;
                    this.cc = li.cc;
                    this.ct = li.ct;
                }
                return true;
            }
            catch { }
            return false;
        }

        public override string ToString()
        {
            return "Lat= " + lat.ToString() + ", Long= " + lon.ToString() + ", CountryName= " + cn + ", CountryCode=" + cc + ", City=" + ct + ", FlagUrl=" + FlagUrl;
        }
    }
}