using System;
using System.Diagnostics;
using XPloit.Core;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;
using XPloit.Core.PayloadRequirements;
using XPloit.Modules.Encoders;
using XPloit.Modules.Payloads.Cmd;

namespace XPloit.Modules.Auxiliary.Local
{
    public class Sys : Exploit
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
                return new Reference[] { new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/system.diagnostics.processstartinfo(v=vs.110).aspx") };
            }
        }
        public override IPayloadRequirements PayloadRequirements { get { return new UniquePayload(typeof(ProcessStartPayload)); } }
        #endregion

        public override bool Run(ICommandLayer cmd)
        {
            JsonDecoder encoder = new JsonDecoder() { Type = typeof(ProcessStartInfo) };
            ProcessStartInfo info = (ProcessStartInfo)encoder.Run(Payload);

            using (Process pr = new Process())
            {
                pr.StartInfo = info;
                bool dv = pr.Start();

                if (dv)
                {
                    cmd.Write("Executed in pid: " + pr.Id.ToString(), true);
                }
                return dv;
            }
        }
    }
}