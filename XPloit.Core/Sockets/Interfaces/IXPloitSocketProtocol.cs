using System.IO;

namespace XPloit.Core.Sockets.Interfaces
{
    public interface IXPloitSocketProtocol
    {
        int Send(IXPloitSocketMsg msg, Stream stream);
        IXPloitSocketMsg Read(Stream stream);
        bool Connect(XPloitSocketClient client);
    }
}