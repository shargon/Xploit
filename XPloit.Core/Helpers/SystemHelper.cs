using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

namespace XPloit.Core.Helpers
{
    public class SystemHelper
    {
        /// <summary>
        /// Is Linux
        /// </summary>
        public static bool IsLinux
        {
            get
            {
                PlatformID p = Environment.OSVersion.Platform;
                return (p == PlatformID.Unix) /*|| (p == 6) || (p == 128)*/;
            }
        }
        /// <summary>
        /// Is Windows
        /// </summary>
        public static bool IsWindows
        {
            get
            {
                PlatformID p = Environment.OSVersion.Platform;
                return p == PlatformID.Win32NT || p == PlatformID.Win32S || p == PlatformID.Win32Windows;
            }
        }
        /// <summary>
        /// Return true if is administratod
        /// </summary>
        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        /// <summary>
        /// Check if its avaiable tcp port
        /// </summary>
        /// <param name="port">Port</param>
        public static bool IsAvailableTcpPort(ushort port) { return IsAvailableTcpPort(new IPEndPoint(IPAddress.Any, port)); }
        /// <summary>
        /// Check if its avaiable tcp port
        /// </summary>
        /// <param name="point">IPEndPoint</param>
        public static bool IsAvailableTcpPort(IPEndPoint point)
        {
            TcpListener tcp = null;
            try
            {
                tcp = new TcpListener(point);
                tcp.Start();

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (tcp != null)
                {
                    tcp.Stop();
                }
            }
        }
        /// <summary>
        /// Check if its avaiable udp port
        /// </summary>
        /// <param name="port">Port</param>
        public static bool IsAvailableUdpPort(ushort port) { return IsAvailableUdpPort(new IPEndPoint(IPAddress.Any, port)); }
        /// <summary>
        /// Check if its avaiable udp port
        /// </summary>
        /// <param name="point">IPEndPoint</param>
        public static bool IsAvailableUdpPort(IPEndPoint point)
        {
            UdpClient udp = null;
            try
            {
                udp = new UdpClient(point);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (udp != null)
                {
                    udp.Close();
                }
            }
        }
    }
}
