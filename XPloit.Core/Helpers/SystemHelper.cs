using System;
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
    }
}