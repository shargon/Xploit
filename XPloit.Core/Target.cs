using System.Collections.Generic;
using XPloit.Core.Enums;

namespace XPloit.Core
{
    public class Target
    {
        /// <summary>
        /// Id
        /// </summary>
        internal int Id { get; set; }
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
        public override string ToString() { return Id + " - " + Name; }
    }
}