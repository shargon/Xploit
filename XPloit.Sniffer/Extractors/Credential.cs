using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class Credential : ExtractBase
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

        protected Credential(ECredentialType type) : base() { Type = type; }
        protected Credential(DateTime date, IPEndPoint ip, ECredentialType type) : base(date, ip) { Type = type; }
    }
}