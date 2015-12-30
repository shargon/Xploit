using System;
using XPloit.Core.Attributes;
using XPloit.Core.Collections;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    public class Module : IModule
    {
        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Payload; } }
        /// <summary>
        /// IsLocal
        /// </summary>
        public virtual bool IsLocal { get { return false; } }
        /// <summary>
        /// IsRemote
        /// </summary>
        public virtual bool IsRemote { get { return false; } }
        /// <summary>
        /// References
        /// </summary>
        public virtual Reference[] References { get { return null; } }
        /// <summary>
        /// DisclosureDate
        /// </summary>
        public virtual DateTime DisclosureDate { get { return DateTime.MinValue; } }
        /// <summary>
        /// Targets
        /// </summary>
        public virtual Target[] Targets { get { return new Target[] { new Target() { Platform = EPlatform.None, Name = "All" } }; } }
        /// <summary>
        /// Target
        /// </summary>
        [ConfigurableProperty(Required = true, Description = "Especify the Target")]
        public Target Target { get; set; }
        /// <summary>
        /// Payload
        /// </summary>
        [ConfigurableProperty(Required = true, Description = "Especify the Payload")]
        public Payload Payload { get; set; }
        /// <summary>
        /// Payload Requirements
        /// </summary>
        public virtual IPayloadRequirements PayloadRequirements { get { return null; } }
        /// <summary>
        /// Run Method
        /// </summary>
        /// <param name="cmd">Command</param>
        public virtual bool Run(ICommandLayer cmd)
        {
            return false;
        }
        /// <summary>
        /// Check Method
        /// </summary>
        /// <param name="cmd">Command</param>
        public virtual ECheck Check(ICommandLayer cmd) { return ECheck.CantCheck; }
        /// <summary>
        /// Prepare the current module
        /// </summary>
        public virtual void Prepare()
        {
            if (Target == null)
            {
                Target[] t = Targets;
                if (t != null && t.Length > 0)
                    Target = t[0];
            }
            if (Payload == null)
            {
                Payload[] payloads = PayloadCollection.Current.GetPayloadAvailables(PayloadRequirements);
                if (payloads != null && payloads.Length == 1)
                    Payload = payloads[0];
            }
        }
    }
}