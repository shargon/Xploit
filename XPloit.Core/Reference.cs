using XPloit.Core.Enums;

namespace XPloit.Core
{
    public class Reference
    {
        /// <summary>
        /// Type
        /// </summary>
        public EReferenceType Type { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="value">Value</param>
        public Reference(EReferenceType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return Type.ToString() + "-" + Value;
        }
    }
}
