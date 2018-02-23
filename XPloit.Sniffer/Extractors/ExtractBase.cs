using System;
using System.Net;
using XPloit.Helpers;
using XPloit.Helpers.Geolocate;
using XPloit.Sniffer.Interfaces;

namespace XPloit.Sniffer.Extractors
{
    public class ExtractBase : ICountryRecaller
    {
        IPAddress _Address;
        /// <summary>
        /// Date
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// Address
        /// </summary>
        public string Address
        {
            get { return _Address == null ? "" : _Address.ToString(); }
            set { _Address = IPAddress.Parse(value); }
        }
        /// <summary>
        /// Continent
        /// </summary>
        public string Continent { get; set; }
        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected ExtractBase() { }
        /// <summary>
        /// Base
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="ip">Ip</param>
        protected ExtractBase(DateTime date, IPEndPoint ip) : this()
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
                    string cn, ct;
                    if (StringHelper.Split(r.ISOCode, '-', out cn, out ct))
                    {
                        Continent = cn;
                        Country = ct;
                    }
                    else
                    {
                        Country = r.ISOCode;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString() { return JsonHelper.Serialize(this, false, false); }
    }
}