using System;
using System.Net;

namespace XPloit.Sniffer.Extractors
{
    public class FTPCredential : Credential
    {
        public FTPCredential() : base(ECredentialType.Ftp) { }
        public FTPCredential(DateTime date, IPEndPoint ip) : base(date, ip, ECredentialType.Ftp) { }
    }
}