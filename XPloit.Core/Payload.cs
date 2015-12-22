using System.Text;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    public class Payload : IModule
    {
        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Payload; } }
        /// <summary>
        /// Encoding value
        /// </summary>
        public virtual Encoding Encoding { get { return Encoding.UTF8; } }
        /// <summary>
        /// Payload value
        /// </summary>
        public virtual byte[] Value { get { return null; } }
        /// <summary>
        /// PayloadString value
        /// </summary>
        public string StringValue { get { return this.Encoding.GetString(Value); } }
        /// <summary>
        /// References
        /// </summary>
        public virtual Reference[] References { get { return null; } }
    }
}