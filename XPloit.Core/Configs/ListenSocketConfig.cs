using System.Net;
using XPloit.Core.Multi;

namespace XPloit.Core.Configs
{
    public class ListenSocketConfig
    {
        /// <summary>
        /// Listen Address
        /// </summary>
        public IPAddress ListenAddress { get; set; }
        /// <summary>
        /// Listen Port
        /// </summary>
        public ushort ListenPort { get; set; }
        /// <summary>
        /// IP Filter
        /// </summary>
        public IPFilter IPFilter { get; set; }
        /// <summary>
        /// CryptPassword
        /// </summary>
        public Password CryptKey { get; set; }

        public override string ToString()
        {
            return "ListenAddress=" + ListenAddress.ToString() + " ListenPort=" + ListenPort.ToString();
        }
    }
}