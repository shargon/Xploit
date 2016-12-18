using System;
using System.Net;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local
{
    public class DetectTorExitNode : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Check if a ip its a Tor exit node"; } }
        public override Reference[] References
        {
            get { return new Reference[] { new Reference(EReferenceType.URL, "https://check.torproject.org/exit-addresses") }; }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Remote ip for check")]
        public IPAddress RemoteIp { get; set; }
        #endregion

        public override bool Run()
        {
            Check();
            return true;
        }
        public override ECheck Check()
        {
            WriteInfo("Updating tor exit node list", TorHelper.UpdateTorExitNodeList(false).ToString(), ConsoleColor.Green);

            bool res = TorHelper.IsTorExitNode(RemoteIp);
            WriteInfo("Check tor exit node '" + RemoteIp.ToString() + "' results", res ? "EXIT-NODE DETECTED!" : "NOT LISTED", res ? ConsoleColor.Red : ConsoleColor.Green);

            Thread.Sleep(1000);

            return res ? ECheck.Ok : ECheck.Error;
        }
    }
}