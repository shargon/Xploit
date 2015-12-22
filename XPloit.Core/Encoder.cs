using System;
using XPloit.Core.Enums;
using XPloit.Core.Interfaces;

namespace XPloit.Core
{
    public class Encoder : IModule
    {
        /// <summary>
        /// ModuleType
        /// </summary>
        internal override EModuleType ModuleType { get { return EModuleType.Encoder; } }
        /// <summary>
        /// Runt
        /// </summary>
        /// <param name="payload">Payload</param>
        /// <returns>Return encoded payload</returns>
        public virtual object Run(Payload payload) { throw (new NotImplementedException()); }
    }
}