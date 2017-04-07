using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;


namespace XPloit.Helpers
{
    public class SubdomainEnumerationHelper
    {
        public class SubdomainEnumerationOptions
        {
            /// <summary>
            /// Include files
            /// </summary>
            public string[] domain { get; set; }
        }

        public static bool SubdomainCheck(string url)
        {
            try
            {
                IPHostEntry hostInfo = Dns.GetHostByName(url);
                if (hostInfo.ToString() != "")
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}