using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class HttpAttack : Attack
    {
        public HttpAttack() : base(EAttackType.None) { }
        public HttpAttack(EAttackType type, DateTime date, IPEndPoint ip) : base(date, ip, type) { }
        /// <summary>
        /// Host
        /// </summary>
        public string HttpHost { get; set; }
        /// <summary>
        /// Url
        /// </summary>
        public string HttpUrl { get; set; }
        /// <summary>
        /// User
        /// </summary>
        public string[] Get { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string[] Post { get; set; }
    }
}