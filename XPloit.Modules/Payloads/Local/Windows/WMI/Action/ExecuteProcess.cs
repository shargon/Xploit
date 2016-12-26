using Auxiliary.Local.Windows;
using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.Windows.WMI.Action
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Execute a process in WMI")]
    public class ExecuteProcess : Payload, WMIManager.IWMIPayload
    {
        #region Configure
        public override Reference[] References
        {
            get
            {
                return new Reference[] { new Reference(EReferenceType.URL, "https://msdn.microsoft.com/en-us/library/windows/desktop/aa394372(v=vs.85).aspx") };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Command line parameter")]
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

        public string GetWmicparams()
        {
            // Windows
            return "process call create \"" + CommandLine + "\"";
        }
        public bool ProcessOutPut(string output)
        {
            /* WINDOWS
            Ejecutando (Win32_Process)->Create()
            Ejecución correcta del método.
            Parámetros de salida:
            instance of __PARAMETERS
            {
                    ProcessId = 5692;
                    ReturnValue = 0;
            };
            */
            output = output.Replace(" ", "");

            int ix = output.IndexOf("ProcessId=");
            if (ix != -1 && output.Contains("ReturnValue=0;"))
            {
                int ix2 = output.IndexOf(";", ix);

                WriteInfo("Process executed", output.Substring(ix + 10, ix2 - ix - 10), ConsoleColor.Green);
                return true;
            }

            return false;
        }
        #endregion
    }
}