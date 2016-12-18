using System;

namespace XPloit.Helpers.Attributes
{
    public class ConfigurableProperty : Attribute
    {
        bool _Optional;

        /// <summary>
        /// Contructor
        /// </summary>
        public ConfigurableProperty()
        {
            _Optional = false;
        }
        /// <summary>
        /// Required
        /// </summary>
        public bool Optional
        {
            get { return _Optional; }
            set { _Optional = value; }
        }
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
    }
}