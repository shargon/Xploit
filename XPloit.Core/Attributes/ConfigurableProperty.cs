using System;

namespace XPloit.Core.Attributes
{
    public class ConfigurableProperty : Attribute
    {
        /// <summary>
        /// Contructor
        /// </summary>
        public ConfigurableProperty()
        {
            Required = false;
        }
        /// <summary>
        /// Required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
    }
}