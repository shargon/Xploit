using Xploit.Sniffer.Enums;
using Xploit.Sniffer.Extractors;
using XPloit.Sniffer.Streams;

namespace Xploit.Sniffer.Interfaces
{
    public interface ICredentialExtractor
    {
        EExtractorReturn GetCredentials(TcpStream stream, out Credential[] cred);
    }
}