using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XPloit.Core.Helpers
{
    public class IniHelper
    {
        bool _toLowerCase;
        Dictionary<string, Section> _Sec = new Dictionary<string, Section>();

        class Section
        {
            string _Name;
            Dictionary<string, string> _Items;

            /// <summary>
            /// Bombre de la sección
            /// </summary>
            public string Name { get { return _Name; } }
            /// <summary>
            /// Enumerador de las variables
            /// </summary>
            public IEnumerable<string> Variables { get { return _Items.Keys; } }

            public string this[Dictionary<string, string> variables, string name, string defaultValue, bool toLowerCase]
            {
                get
                {
                    string v = this[name, defaultValue, toLowerCase];
                    return StringHelper.ReplaceVariables(variables, v);
                }
            }

            public string this[string name, string defaultValue, bool toLowerCase]
            {
                get
                {
                    if (toLowerCase) name = name.ToLowerInvariant();

                    string s;
                    if (!_Items.TryGetValue(name, out s)) return defaultValue;
                    return s;
                }
            }
            public Section(string name, bool toLowerCase)
            {
                _Name = toLowerCase ? name.ToLowerInvariant() : name;
                _Items = new Dictionary<string, string>();
            }
            internal void Add(string name, string value, bool toLowerCase)
            {
                _Items.Add(toLowerCase ? name.ToLowerInvariant() : name, value);
            }
        }

        /// <summary>
        /// Secciones
        /// </summary>
        public IEnumerable<string> Sections { get { return _Sec.Keys; } }
        /// <summary>
        /// Obtiene la lista de variables de una sección
        /// </summary>
        /// <param name="section">Sección</param>
        public IEnumerable<string> GetVariables(string section)
        {
            if (_toLowerCase) section = section.ToLowerInvariant();

            Section s;
            if (!_Sec.TryGetValue(section, out s)) return null;
            return s.Variables;
        }
        public string this[Dictionary<string, string> variables, string section, string name, string defaultValue]
        {
            get
            {
                string v = this[section, name, defaultValue];
                return StringHelper.ReplaceVariables(variables, v);
            }
        }
        /// <summary>
        /// Obtiene el valor del ini
        /// </summary>
        /// <param name="section">Sección</param>
        /// <param name="name">Nombre</param>
        /// <param name="defaultValue">Valor por defecto</param>
        /// <param name="toLowerCase">A minúsculas</param>
        public string this[string section, string name, string defaultValue]
        {
            get
            {
                if (_toLowerCase)
                {
                    section = section.ToLowerInvariant();
                    name = name.ToLowerInvariant();
                }

                Section s;
                if (!_Sec.TryGetValue(section, out s)) return defaultValue;
                return s[name, defaultValue, _toLowerCase];
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">Archivo</param>
        /// <param name="variables">variables</param>
        /// <param name="toLowerCase">Convertir variables a lowerCase</param>
        public IniHelper(string file, bool toLowerCase)
        {
            _toLowerCase = toLowerCase;
            string files = File.ReadAllText(file, Encoding.UTF8);

            //if (variables.Count>0)
            //{
            //    foreach (string k in variables.Keys)
            //        files = files.Replace(k, variables[k]);
            //}

            bool primera = true;
            Section step = new Section("", false);

            foreach (string sl in files.Split('\n'))
            {
                string line = sl.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//")) continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    primera = false;
                    step = new Section(line.Substring(1, line.Length - 2), toLowerCase);
                    try
                    {
                        _Sec.Add(step.Name, step);
                    }
                    catch
                    {
                        throw (new Exception("Duplicate key '" + step.Name + "'"));
                    }
                    continue;
                }

                if (primera)
                {
                    primera = false;
                    _Sec.Add(step.Name, step);
                }

                string iz, dr;
                StringHelper.Split(line, '=', out iz, out dr);
                iz = toLowerCase ? iz.Trim().ToLowerInvariant() : iz.Trim();

                try
                {
                    step.Add(iz, dr, toLowerCase);
                }
                catch
                {
                    throw (new Exception("Duplicate key '" + step.Name + "." + iz + "'"));
                }
            }
        }
    }
}