using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XPloit.Core.Helpers
{
    public class IniHelper
    {
        bool _SectionsToLowerCase;
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
            internal void Add(string line, bool splitVariables, bool sectionsToLowerCase)
            {
                if (!splitVariables)
                {
                    _Items.Add(line, "");
                    return;
                }
                string iz, dr;
                StringHelper.Split(line, '=', out iz, out dr);
                iz = (sectionsToLowerCase ? iz.ToLowerInvariant() : iz).Trim();

                _Items.Add(iz, dr);
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
            if (_SectionsToLowerCase) section = section.ToLowerInvariant();

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
                if (_SectionsToLowerCase)
                {
                    section = section.ToLowerInvariant();
                    name = name.ToLowerInvariant();
                }

                Section s;
                if (!_Sec.TryGetValue(section, out s)) return defaultValue;
                return s[name, defaultValue, _SectionsToLowerCase];
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">Archivo</param>
        /// <param name="variables">variables</param>
        /// <param name="sectionsToLowerCase">Convertir variables a lowerCase</param>
        /// <param name="trimLines">Trim Lines</param>
        /// <param name="splitVariables">True para convertir variable=value en variables, false para usar la linea entera</param>
        public IniHelper(string file, bool sectionsToLowerCase, bool trimLines, bool splitVariables)
        {
            _SectionsToLowerCase = sectionsToLowerCase;
            string files = File.ReadAllText(file, Encoding.UTF8);

            //if (variables.Count>0)
            //{
            //    foreach (string k in variables.Keys)
            //        files = files.Replace(k, variables[k]);
            //}

            bool primera = true;
            Section step = new Section("", false);

            foreach (string sl in files.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = trimLines ? sl.Trim() : sl;
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//")) continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    primera = false;
                    step = new Section(line.Substring(1, line.Length - 2), sectionsToLowerCase);
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

                try
                {
                    step.Add(line, splitVariables, sectionsToLowerCase);
                }
                catch
                {
                    throw (new Exception("Duplicate key in line '" + line + "'"));
                }
            }
        }
    }
}