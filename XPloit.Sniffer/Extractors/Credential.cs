using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net;
using XPloit.Helpers;
using XPloit.Helpers.Geolocate;
using XPloit.Sniffer.Interfaces;

namespace XPloit.Sniffer.Extractors
{
    public class Credential : ICountryRecaller
    {
        public enum ECredentialType : byte
        {
            None = 0,
            Ftp = 1,
            Pop3 = 2,
            Telnet = 3,
            HttpGet = 4,
            HttpPost = 5,
            HttpAuth = 6,
        }

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
            get { return _Address.ToString(); }
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
        /// Credential type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]  // JSON.Net
        [BsonRepresentation(BsonType.String)]         // Mongo
        public ECredentialType Type { get; set; }
        /// <summary>
        /// Is Valid
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// User
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        protected Credential(ECredentialType type) { Type = type; }
        protected Credential(DateTime date, IPEndPoint ip, ECredentialType type) : this(type)
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
                    string c, cc;
                    StringHelper.Split(r.ISOCode, '-', out cc, out c);

                    Continent = cc;
                    Country = c;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return JsonHelper.Serialize(this, false, false);
        }
    }
}