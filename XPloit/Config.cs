using XPloit.Core.Multi;
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
        /// User
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// File for run command
        /// </summary>
        public string Resource { get; set; }
    }
}