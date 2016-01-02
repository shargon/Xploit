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
        public static bool IsAvailableTcpPort(ushort port)
        {
            TcpListener tcp = null;
            try
            {
                tcp = new TcpListener(IPAddress.Any, port);
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
    }
}