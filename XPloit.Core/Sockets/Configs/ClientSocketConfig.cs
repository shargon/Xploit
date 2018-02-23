using System.Net;
using XPloit.Core.Streams;

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
        public string CryptKey { get; set; }

        public override string ToString()
        {
            return "Address=" + Address.ToString() + " Port=" + Port.ToString();
        }
    }
}
