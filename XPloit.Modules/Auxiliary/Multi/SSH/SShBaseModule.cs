using Renci.SshNet;
using System;
using System.Net;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace Auxiliary.Multi.SSH
{
    public class SShBaseModule : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Host")]
        public IPEndPoint Host { get; set; }
        [ConfigurableProperty(Description = "User")]
        public string User { get; set; }
        [ConfigurableProperty(Description = "Password")]
        public string Password { get; set; }
        #endregion

        protected SShBaseModule()
        {
            Host = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 22);
            User = "root";
        }
        public override ECheck Check()
        {
            WriteInfo("Connecting ...");
            try
            {
                using (SshClient SSH = new SshClient(Host.Address.ToString(), Host.Port, User, Password))
                {
                    SSH.Connect();
                    WriteInfo("Connected successful");
                    return ECheck.Ok;
                }
            }
            catch (Exception e)
            {
                WriteError(e.Message);
            }

            return ECheck.Error;
        }
         
        public override bool Run()
        {
            WriteInfo("Connecting ...");

            using (SshClient SSH = new SshClient(Host.Address.ToString(), Host.Port, User, Password))
            {
                SSH.Connect();
                WriteInfo("Connected successful");

               return RunSSHAction(SSH);
            }
        }
        protected virtual bool RunSSHAction(SshClient ssh) { throw new NotImplementedException(); }
    }
}