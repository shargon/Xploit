using XPloit.Core.Sockets.Configs;

namespace XPloit
{
    class Config
    {
        /// <summary>
        /// Config socket server
        /// </summary>
        public ListenSocketConfig Listen { get; set; }
        /// <summary>
        /// Config socket client
        /// </summary>
        public ClientSocketConfig Connect { get; set; }
        /// <summary>
        /// File for replay command
        /// </summary>
        public string Replay { get; set; }
    }
}