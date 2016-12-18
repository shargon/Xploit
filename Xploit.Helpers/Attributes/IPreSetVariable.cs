using System;

namespace XPloit.Helpers.Attributes
{
    public class IPreSetVariable : Attribute
    {
        /// <summary>
        /// Edit the variable before set
        /// </summary>
        /// <param name="value">Value</param>
        public virtual string PreSetVariable(string value)
        {
            return value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected IPreSetVariable() { }
    }
}