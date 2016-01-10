using System.Collections.Generic;
using XPloit.Core.Enums;

namespace XPloit.Core
{
    public class Target
    {
        Dictionary<string, object> _Variables;

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
        /// Arquitecture
        /// </summary>
        public EArquitecture Arquitecture { get; set; }
        /// <summary>
        /// Get Variable value
        /// </summary>
        /// <param name="variable">Variable name</param>
        public object this[string variable]
        {
            get
            {
                object v;
                if (!_Variables.TryGetValue(variable, out v)) return null;
                return v;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Target()
        {
            _Variables = new Dictionary<string, object>();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="variables">Variables</param>
        public Target(string name, params Variable[] variables)
            : this(EPlatform.None, EArquitecture.None, name, variables)
        { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="platform">Platform</param>
        /// <param name="arq">Arquitecture</param>
        /// <param name="name">Name</param>
        /// <param name="variables">Variables</param>
        public Target(EPlatform platform, EArquitecture arq, string name, params Variable[] variables)
            : this()
        {
            Name = name;
            Platform = platform;
            Arquitecture = arq;

            if (variables == null) return;

            foreach (Variable v in variables)
            {
                if (!_Variables.ContainsKey(v.Name))
                    _Variables.Add(v.Name, v.Value);
                else
                    _Variables[v.Name] = v.Value;
            }
        }

        public override string ToString() { return Id + " - " + Name; }
    }
}