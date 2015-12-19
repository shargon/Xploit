using XPloit.Core.Configs;
using XPloit.Core.Multi;

namespace XPloit.Configs
{
    public class Config
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
    }
}