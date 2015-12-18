using XPloit.Core.Configs;
using XPloit.Core.Multi;

namespace XPloit.Configs
{
    public class Config
    {
        /// <summary>
        /// Config for Telnet run mode
        /// </summary>
        public ListenSocketConfig TelnetInterface { get; set; }
        /// <summary>
        /// User
        /// </summary>
        public User User { get; set; }
    }
}