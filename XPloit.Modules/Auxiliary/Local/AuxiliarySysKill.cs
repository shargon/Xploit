using System;
using System.Diagnostics;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliarySysKill : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Kill a process in local machine"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override bool IsRemote { get { return false; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Name { get { return "SystemKill"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] 
                { 
                    new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/system.diagnostics.processstartinfo(v=vs.110).aspx") ,
                    new Reference(EReferenceType.URL,"http://referencesource.microsoft.com/#System/services/monitoring/system/diagnosticts/ProcessStartInfo.cs")
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Process ID")]
        public int? PID { get; set; }
        #endregion

        public override bool Run()
        {
            WriteInfo("Search process ...");

            Process pr = Process.GetProcessById(PID.Value);
            if (pr == null)
            {
                WriteInfo("Process not found");
                return false;
            }

            WriteInfo("Trying kill process");
            pr.Kill();
            WriteInfo("Killed process " + PID.ToString());

            return true;
        }
    }
}