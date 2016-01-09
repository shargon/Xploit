using System;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Requirements.Payloads;

namespace XPloit.Core.Interfaces
{
    public class IModule
    {
        ICommandLayer _IO;

        internal ICommandLayer IO { get { return _IO; } }

        internal void SetIO(ICommandLayer io) { _IO = io; }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="job">Job</param>
        /// <returns>Return the job</returns>
        public Job CreateJob(Job.IJobable job)
        {
            return Job.Create(this, job);
        }

        #region Log methods
        void WriteStart(string ch, ConsoleColor color)
        {
            if (_IO == null) return;

            _IO.SetForeColor(ConsoleColor.Gray);
            _IO.Write("[");
            _IO.SetForeColor(color);
            _IO.Write(ch);
            _IO.SetForeColor(ConsoleColor.Gray);
            _IO.Write("] ");

        }
        public void WriteError(string error)
        {
            if (_IO == null) return;

            if (string.IsNullOrEmpty(error)) error = "";
            else error = error.Trim();

            WriteStart("!", ConsoleColor.Red);
            _IO.SetForeColor(ConsoleColor.Red);
            _IO.WriteLine(error.Replace("\n", "\n    "));
        }
        public void WriteInfo(string info)
        {
            if (_IO == null) return;

            if (string.IsNullOrEmpty(info)) info = "";
            else info = info.Trim();

            WriteStart("*", ConsoleColor.Cyan);
            _IO.WriteLine(info.Replace("\n", "\n    "));
        }
        public void WriteInfo(string info, string colorText, ConsoleColor color)
        {
            if (_IO == null) return;

            if (string.IsNullOrEmpty(info)) info = "";
            else info = info.Trim();

            WriteStart("*", ConsoleColor.Cyan);
            _IO.Write(info);

            if (!string.IsNullOrEmpty(colorText))
            {
                _IO.Write(" ... [");
                _IO.SetForeColor(color);
                _IO.Write(colorText);
                _IO.SetForeColor(ConsoleColor.Gray);
                _IO.WriteLine("]");
            }
        }
        public void Beep()
        {
            if (_IO == null) return;
            _IO.Beep();
        }
        #endregion

        /// <summary>
        /// Author
        /// </summary>
        public virtual string Author { get { return null; } }
        /// <summary>
        /// Description
        /// </summary>
        public virtual string Description { get { return null; } }
        /// <summary>
        /// Name
        /// </summary>
        public virtual string Name { get { return null; } }
        /// <summary>
        /// Path
        /// </summary>
        public virtual string Path { get { return null; } }
        /// <summary>
        /// Return full path
        /// </summary>
        public string FullPath
        {
            get
            {
                string p = Path;
                string n = Name;

                if (!string.IsNullOrEmpty(p)) p = p.Trim('/') + "/"; else p = "";
                if (string.IsNullOrEmpty(n)) n = "";

                return p + n;
            }
        }
        ///// <summary>
        /// Type
        /// </summary>
        internal virtual EModuleType ModuleType { get { return EModuleType.Module; } }

        public override string ToString() { return FullPath; }

        /// <summary>
        /// Clone current module
        /// </summary>
        public IModule Clone() { return (IModule)ReflectionHelper.Clone(this, true); }

        /// <summary>
        /// Establece la propiedad del módulo, si el modulo y el payload tienen el mismo nombre la intentan establecer de los dos
        /// </summary>
        /// <param name="propertyName">Property</param>
        /// <param name="value">Value</param>
        /// <returns>Return true if its ok</returns>
        public bool SetProperty(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;

            bool ret = false;

            if (this is Module)
            {
                // Module especify
                Module m = (Module)this;
                switch (propertyName.ToLowerInvariant())
                {
                    case "target":
                        {
                            try
                            {
                                int ix = (int)ConvertHelper.ConvertTo(value.ToString(), typeof(int));
                                m.Target = m.Targets[ix];
                                m.Target.Id = ix;
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                }

                if (m.Payload != null)
                {
                    if (m.Payload.SetProperty(propertyName, value))
                        ret = true;
                }

                if (ReflectionHelper.SetProperty(this, propertyName, value))
                {
                    ret = true;
                    if (string.Compare(propertyName, "Payload", true) == 0 && m.Payload != null)
                    {
                        // Prepare payload for this module
                        m.Payload.SetIO(_IO);
                    }
                }
            }
            else
            {
                if (ReflectionHelper.SetProperty(this, propertyName, value))
                    ret = true;
            }

            return ret;
        }
        /// <summary>
        /// Return true if are in this query
        /// </summary>
        /// <param name="query">Query</param>
        public bool AreInThisSearch(string[] query)
        {
            if (query == null || query.Length == 0) return true;

            string search = FullPath + " " + Description + " " + Author;

            foreach (string a in query)
            {
                if (search.IndexOf(a, StringComparison.InvariantCultureIgnoreCase) < 0) return false;
            }

            return true;
        }
        /// <summary>
        /// Check Required Properties
        /// </summary>
        /// <param name="propertyName">Variable for capture property fail</param>
        /// <returns>Return true if OK, false if not</returns>
        public bool CheckRequiredProperties(out string propertyName)
        {
            propertyName = null;

            foreach (PropertyInfo pi in ReflectionHelper.GetProperties(this, true, true, true))
            {
                ConfigurableProperty c = pi.GetCustomAttribute<ConfigurableProperty>();
                if (c == null)
                    continue;

                if (!c.Required) continue;
                if (pi.GetValue(this) == null)
                {
                    propertyName = pi.Name;
                    return false;
                }
            }

            if (this is Module)
            {
                // Module especify
                Module m = (Module)this;

                if (m.Target == null)
                {
                    propertyName = "Target";
                    return false;
                }

                if (m.Payload == null)
                {
                    if (m.PayloadRequirements != null)
                    {
                        propertyName = "Payload";
                        return false;
                    }
                }

                if (m.Payload != null)
                    foreach (PropertyInfo pi in ReflectionHelper.GetProperties(m.Payload, true, true, true))
                    {
                        ConfigurableProperty c = pi.GetCustomAttribute<ConfigurableProperty>();
                        if (c == null)
                            continue;

                        if (!c.Required) continue;
                        if (pi.GetValue(m.Payload) == null)
                        {
                            propertyName = pi.Name;
                            return false;
                        }
                    }
            }

            return true;
        }
    }
}