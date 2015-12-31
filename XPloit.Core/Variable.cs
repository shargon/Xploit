namespace XPloit.Core
{
    public class Variable
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Variable() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public Variable(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}