using XPloit.Sniffer.Enums;
using XPloit.Sniffer.Streams;

namespace XPloit.Sniffer.Interfaces
{
    public interface IObjectExtractor
    {
        EExtractorReturn GetObjects(TcpStream stream, out object[] cred);
    }
}