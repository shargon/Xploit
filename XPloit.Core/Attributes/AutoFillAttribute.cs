using System;

namespace XPloit.Core.Attributes
{
    public class AutoFillAttribute : Attribute
    {
        public string Function { get; private set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="function">Call function</param>
        public AutoFillAttribute(string function) { Function = function; }
    }
}