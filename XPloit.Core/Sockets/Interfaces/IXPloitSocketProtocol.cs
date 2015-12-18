using System.Collections.Generic;
using System.Text;

namespace XPloit.Core.Sockets.Interfaces
{
    public interface IXPloitSocketProtocol
    {
        int Send(XPloitSocketClient cl, IXPloitSocketMsg msg);
        IEnumerable<IXPloitSocketMsg> ProcessBuffer(XPloitSocketClient cl, ref byte[] bxf);
    }
}