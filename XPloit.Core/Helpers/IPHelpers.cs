using System.Net;
using System.Text.RegularExpressions;

namespace XPloit.Core.Helpers
{
    public class IPHelper
    {
        /// <summary>
        /// Search a IPv4 in the string
        /// </summary>
        /// <param name="cad">Input</param>
        /// <param name="ip">Return IPAddress</param>
        public static bool SearchIPv4(string cad, out string ip)
        {
            ip = "";
            string pat = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
            Regex r = new Regex(pat);
            foreach (Match m in r.Matches(cad))
            {
                ip = m.Value;
                break;
            }
            return !string.IsNullOrEmpty(ip);
        }
        /// <summary>
        /// Check if Ip its Private
        /// </summary>
        /// <param name="ip">IP Address</param>
        public static bool IsPrivate(IPAddress ip)
        {
            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    {
                        // The private address ranges are defined in RFC1918. They are:
                        //   - 10.0.0.0 - 10.255.255.255 (10/8 prefix)
                        //   - 172.16.0.0 - 172.31.255.255 (172.16/12 prefix)
                        //   - 192.168.0.0 - 192.168.255.255 (192.168/16 prefix)

                        byte[] iaryIPAddress = ip.GetAddressBytes();
                        if (iaryIPAddress[0] == 10 || (iaryIPAddress[0] == 192 && iaryIPAddress[1] == 168) || (iaryIPAddress[0] == 172 && (iaryIPAddress[1] >= 16 && iaryIPAddress[1] <= 31)))
                            return true;

                        // IP Address is "probably" public. This doesn't catch some VPN ranges like OpenVPN and Hamachi.
                        return false;
                    }
                case AddressFamily.InterNetworkV6:
                    {
                        // Haven't NAT
                        return false;
                    }
            }
            return false;
        }
        /// <summary>
        /// Parsea una cadena para separarla entre ip y puerto
        /// </summary>
        /// <param name="cad">Cadena del tipo 192.168.1.5:6666 o 192.168.1.5</param>
        /// <param name="ip">Devuelve la ip</param>
        /// <param name="prto">Devuelve el puerto, es por referencia para usarlo a su vez como valor por defecto</param>
        /// <returns>Devuelve true si es capaz de parsearlo correctamente</returns>
        public static bool ParseIpPort(string cad, out IPAddress ip, ref ushort prto)
        {
            ip = null;
            if (string.IsNullOrEmpty(cad) || cad.Length < 3) return false;

            string[] ar = cad.Split(':');
            if (ar.Length > 2) return false;

            if (!IPAddress.TryParse(ar[0], out ip)) return false;
            if (ar.Length == 2)
            {
                ushort dprto = prto;
                if (!ushort.TryParse(ar[1], out dprto)) return false;
                prto = dprto;
            }
            return true;
        }
        /// <summary>
        /// Devuelve si la ip es una ip local
        /// </summary>
        /// <param name="ip">Ip</param>
        /// <returns>Devuelve true si es una ip local</returns>
        public static bool IsLocalHost(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return true;

            try
            {
                IPAddress[] hostIPs = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

                foreach (IPAddress hostIP in hostIPs)
                    if (hostIP.Equals(ip)) return true;
            }
            catch { }
            return false;
        }
    }
}
