using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class TelnetCredential : Credential
    {
        public TelnetCredential() : base(ECredentialType.Telnet) { }
        public TelnetCredential(DateTime date, IPEndPoint ip) : base(date, ip, ECredentialType.Telnet) { }
    }
}