using System.Net;
using XPloit.Core.Multi;

namespace XPloit.Core.Sockets.Configs
{
    public class ClientSocketConfig
    {
        /// <summary>
        /// Listen Address
        /// </summary>
        public IPAddress Address { get; set; }
        /// <summary>
        /// Listen Port
        /// </summary>
        public ushort Port { get; set; }
        /// <summary>
        /// CryptPassword
        /// </summary>
        public Password CryptKey { get; set; }

        public override string ToString()
        {
            return "ListenAddress=" + Address.ToString() + " ListenPort=" + Port.ToString();
        }
    }
}