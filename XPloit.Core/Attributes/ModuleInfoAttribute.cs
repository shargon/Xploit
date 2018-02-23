using System;

namespace XPloit.Core.Attributes
{
    public class ModuleInfoAttribute : Attribute
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Author
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// DisclosureDate
        /// </summary>
        public DateTime DisclosureDate { get; set; }
    }
}