using System;
using System.Diagnostics;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.PayloadRequirements;
using XPloit.Modules.Payloads.Multi;

namespace XPloit.Modules.Auxiliary.Local
{
    public class AuxiliarySys : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Execute a system command in local machine"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override bool IsRemote { get { return false; } }
        public override string Path { get { return "Auxiliary/Local"; } }
        public override string Name { get { return "System"; } }
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
        public override IPayloadRequirements PayloadRequirements { get { return new UniquePayload(typeof(ProcessStartPayload), typeof(ProcessKillPayload)); } }
        #endregion

        public override bool Run()
        {
            if (Payload is ProcessStartPayload)
            {
                //JsonDecoder encoder = new JsonDecoder(typeof(ProcessStartPayload.SerializableProcessStartInfo));
                //ProcessStartPayload.SerializableProcessStartInfo info = (ProcessStartPayload.SerializableProcessStartInfo)encoder.Run(Payload);

                ProcessStartPayload pi = (ProcessStartPayload)Payload;
                ProcessStartInfo info = pi.GetProcessStartInfo();
                if (info == null) return false;

                using (Process pr = new Process())
                {
                    pr.StartInfo = info;

                    if (pr.Start())
                    {
                        WriteInfo("Executed in pid ", pr.Id.ToString(), ConsoleColor.Green);
                        return true;
                    }
                    return false;
                }
            }

            if (Payload is ProcessKillPayload)
            {
                WriteInfo("Search process ...");

                ProcessKillPayload pi = (ProcessKillPayload)Payload;
                Process pr = Process.GetProcessById(pi.PID.Value);
                if (pr == null)
                {
                    WriteInfo("Process not found");
                    return false;
                }

                WriteInfo("Trying kill process");
                pr.Kill();
                WriteInfo("Killed process " + pi.PID.ToString());

                return true;
            }

            return false;
        }
    }
}