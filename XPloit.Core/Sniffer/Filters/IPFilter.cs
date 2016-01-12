using System.Collections.Generic;
using System.Net;
using XPloit.Core.Sniffer.Headers;
using XPloit.Core.Sniffer.Interfaces;

namespace XPloit.Core.Sniffer.Filters
{
    public class IPFilter : ITcpStreamFilter
    {
        /// <summary>
        /// Not allowed ips
        /// </summary>
        public List<IPAddress> Banned { get; set; }
        /// <summary>
        /// Not allowed ips
        /// </summary>
        public List<IPAddress> OnlyAllowed { get; set; }

        public bool IsAllowed(TcpHeader packet)
        {
            if (IsAllowed(packet.IpHeader.SourceAddress)) return true;
            if (IsAllowed(packet.IpHeader.DestinationAddress)) return true;
            return false;
        }

        /// <summary>
        /// Check if ip its allowed
        /// </summary>
        /// <param name="ip">IP</param>
        /// <returns>True if allowed otherwise false</returns>
        public bool IsAllowed(IPAddress ip)
        {
            string sip = ip.ToString();

            if (Banned != null)
            {
                foreach (IPAddress i in Banned)
                    if (i.ToString() == sip) return false;
            }

            if (OnlyAllowed != null && OnlyAllowed.Count > 0)
            {
                foreach (IPAddress i in OnlyAllowed)
                    if (i.ToString() == sip) return true;

                return false;
            }

            return true;
        }
    }
}