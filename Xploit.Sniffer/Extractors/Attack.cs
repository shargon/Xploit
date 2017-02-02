using System;
using System.Net;
using XPloit.Helpers.Geolocate;
using XPloit.Sniffer.Interfaces;
using XPloit.Helpers;
using Newtonsoft.Json.Converters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace XPloit.Sniffer.Extractors
{
    public class Attack : ICountryRecaller
    {
        public enum EAttackType : byte
        {
            None = 0,
            HttpSqli = 1,
            HttpXss = 2
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
        public EAttackType Type { get; set; }
        /// <summary>
        /// Is Valid
        /// </summary>
        public bool IsValid { get; set; }

        public Attack(EAttackType type) { Type = type; }
        public Attack(DateTime date, IPEndPoint ip, EAttackType type) : this(type)
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
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return JsonHelper.Serialize(this, false, false);
        }
    }
}