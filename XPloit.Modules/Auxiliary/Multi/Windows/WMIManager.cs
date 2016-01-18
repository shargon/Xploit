using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.Requirements.Payloads;

namespace Auxiliary.Multi.Windows
{
    public class WMIManager : Module
    {
        public interface IWMIPayload
        {
            bool Run(ManagementScope connection);
        }

        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "WMI call"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/windows/desktop/aa394554(v=vs.85).aspx") ,
                };
            }
        }
        public override IPayloadRequirements PayloadRequirements { get { return new InterfacePayload(typeof(IWMIPayload)); } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "The server and namespace for the ManagementScope")]
        public string WMIPath { get; set; }
        [ConfigurableProperty(Description = "Sets the user name to be used for the connection operation")]
        public string UserName { get; set; }
        [ConfigurableProperty(Description = "Sets the password for the specified user")]
        public string Password { get; set; }
        [ConfigurableProperty(Required = true, Description = "Describes the impersonation level to be used to connect to WMI")]
        public ImpersonationLevel ImpersonationLevel { get; set; }
        #endregion

        public WMIManager()
        {
            string computerName = ".";

            WMIPath = String.Format(@"\\{0}\ROOT\CIMV2", computerName);
            UserName = "";
            Password = "";
            ImpersonationLevel = ImpersonationLevel.Impersonate;
        }

        ManagementScope Connect()
        {
            ConnectionOptions connOptions = new ConnectionOptions();
            connOptions.Impersonation = this.ImpersonationLevel;

            if (!string.IsNullOrEmpty(UserName))
            {
                connOptions.Username = UserName;
                connOptions.Password = Password;
            }

            connOptions.EnablePrivileges = true;
            ManagementScope manScope = new ManagementScope(WMIPath, connOptions);

            manScope.Connect();
            return manScope;
        }

        public override ECheck Check()
        {
            ManagementScope cnx = Connect();
            return cnx.IsConnected ? ECheck.Ok : ECheck.Error;
        }

        public override bool Run()
        {
            IWMIPayload check = (IWMIPayload)this.Payload;
            return check.Run(Connect());
        }
    }
}