using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class Pop3Credential : Credential
    {
        public Pop3Credential() : base(ECredentialType.Pop3) { }
        public Pop3Credential(DateTime date, IPEndPoint ip) : base(date, ip, ECredentialType.Pop3) { }
        /// <summary>
        /// IsAPOP https://tools.ietf.org/html/rfc1939#page-15
        /// </summary>
        public string AuthType { get; set; }

    }
}