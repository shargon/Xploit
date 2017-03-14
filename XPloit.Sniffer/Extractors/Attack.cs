using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class Attack : ExtractBase
    {
        public enum EAttackType : byte
        {
            None = 0,
            HttpSqli = 1,
            HttpXss = 2
        }

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

        protected Attack(EAttackType type) : base() { Type = type; }
        protected Attack(DateTime date, IPEndPoint ip, EAttackType type) : base(date, ip) { Type = type; }
    }
}