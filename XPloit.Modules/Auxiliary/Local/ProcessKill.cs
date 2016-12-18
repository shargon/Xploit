using System.Diagnostics;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local
{
    public class ProcessKill : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Kill a process in local machine"; } }
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
        [ConfigurableProperty(Description = "Process ID")]
        public int? PID { get; set; }
        #endregion

        public override ECheck Check()
        {
            Process pr = Process.GetProcessById(PID.Value);
            if (pr == null) return ECheck.Error;
            pr.Dispose();
            return ECheck.Ok;
        }

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
            pr.Dispose();
            WriteInfo("Killed process " + PID.ToString());

            return true;
        }
    }
}