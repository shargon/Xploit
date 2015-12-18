using System.Net;
using System.Text.RegularExpressions;

namespace XPloit.Core.Helpers
{
    public class IPHelper
    {
        public static bool SearchIP(string cad, out string ip)
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
        public static bool IsOwnIpAddress(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return true;

            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (IPAddress hostIP in hostIPs)
                    if (hostIP.Equals(ip)) return true;
            }
            catch { }
            return false;
        }
    }
}