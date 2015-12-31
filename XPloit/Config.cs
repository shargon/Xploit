using XPloit.Core.Configs;
using XPloit.Core.Multi;

namespace XPloit
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
        /// <summary>
        /// File for run command
        /// </summary>
        public string RunFile { get; set; }
    }
}