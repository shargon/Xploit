using System.Collections.Generic;
using XPloit.Core.Enums;

namespace XPloit.Core
{
    public class Target
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Platform
        /// </summary>
        public EPlatform Platform { get; set; }
        /// <summary>
        /// Variables
        /// </summary>
        public Dictionary<string, object> Variables { get; set; }
    }
}