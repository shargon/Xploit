using XPloit.Core.Sniffer.Filters;

namespace XPloit.Core.Sockets.Configs
{
    public class ListenSocketConfig : ClientSocketConfig
    {
        /// <summary>
        /// IP Filter
        /// </summary>
        public IPFilter IPFilter { get; set; }
    }
}