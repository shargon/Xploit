using System;
using System.Net;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local.Fingerprinting
{
    [ModuleInfo(Author = "Álvaro Díaz Hernández", Description = "Enumerating Subdomains for a wordlist")]
    public class SubdomainEnumeration : Module
    {
        #region Configure
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "Without References") }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Domain")]
        public string domain { get; set; }
        #endregion

        public override bool Run()
        {
            Check();
            return true;
        }
        public override ECheck Check()
        {
            string subdomain;
            string url = "";
            bool res = false;
            // Read the list and check if exists the subdomain.  
            System.IO.StreamReader file = new System.IO.StreamReader(@"subdomains.txt");
            WriteInfo("", "Checking +100k of subdomains, this will take a lot of time.", ConsoleColor.Blue);
            while ((subdomain = file.ReadLine()) != null)
            {
                url = subdomain + "." + domain;
                try
                {
                    IPHostEntry hostInfo = Dns.GetHostByName(url);
                    if (hostInfo.ToString() != "")
                        res = true;
                    else
                        res = false;
                }
                catch (Exception e)
                {
                    res = false;
                }

                if (res ==true)
                {
                    WriteInfo("Subdomain found:" + subdomain.ToString() + "." + domain.ToString(), "FOUND" , ConsoleColor.Green);
                }
               // WriteInfo("Checking subdomain: '" + subdomain.ToString() + "." + domain.ToString() + "' results", res ? "EXISTS!" : "NOT EXISTS", res ? ConsoleColor.Green : ConsoleColor.Red);
            }

            return res ? ECheck.Ok : ECheck.Error;
        }
    }
}
