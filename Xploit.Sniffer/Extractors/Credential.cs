using System;
using System.Net;
using Xploit.Helpers.Geolocate;
using XPloit.Helpers;

namespace Xploit.Sniffer.Extractors
{
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
}