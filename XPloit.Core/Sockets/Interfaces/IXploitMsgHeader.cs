using System;
using XPloit.Core.Sockets.Enums;

namespace XPloit.Core.Sockets.Interfaces
{
    public class IXploitMsgHeader
    {
        /// <summary>
        /// Especifica el tipo de la clase
        /// </summary>
        public virtual EXPloitSocketMsgHeader Type { get { throw (new Exception("ERROR")); } }
    }
}