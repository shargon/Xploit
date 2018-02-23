using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class HttpCredential : Credential
    {
        public HttpCredential() : base(ECredentialType.None) { }
        public HttpCredential(ECredentialType type, DateTime date, IPEndPoint ip) : base(date, ip, type) { }
        /// <summary>
        /// Host
        /// </summary>
        public string HttpHost { get; set; }
        /// <summary>
        /// Url
        /// </summary>
        public string HttpUrl { get; set; }
    }
}