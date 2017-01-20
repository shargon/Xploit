using Xploit.Sniffer.Enums;
using XPloit.Sniffer.Streams;

namespace Xploit.Sniffer.Interfaces
{
    public interface IObjectExtractor
    {
        EExtractorReturn GetObjects(TcpStream stream, out object[] cred);
    }
}