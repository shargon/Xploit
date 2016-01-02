using System;
using XPloit.Core;
using XPloit.Core.Attributes;

namespace XPloit.Modules.Payloads.Multi
{
    public class ProcessKillPayload : Payload
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Kill process"; } }
        public override string Name { get { return "ProcessKill"; } }
        public override string Path { get { return "Multi"; } }

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Process ID")]
        public int? PID { get; set; }
        #endregion

        public ProcessKillPayload()
        {
            // Default variables
            PID = null;
        }

        /// <summary>
        /// Payload Value
        /// </summary>
        public override byte[] Value { get { return BitConverter.GetBytes(PID.HasValue ? PID.Value : 0); } }
    }
}