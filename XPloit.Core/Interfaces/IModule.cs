using System;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.PayloadRequirements;

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
        internal virtual EModuleType ModuleType { get { return EModuleType.Exploit; } }

        public override string ToString() { return FullPath; }

        /// <summary>
        /// Clone current module
        /// </summary>
        public IModule Clone() { return (IModule)ReflectionHelper.Clone(this, true); }

        /// <summary>
        /// Set property Value
        /// </summary>
        /// <param name="propertyName">Property</param>
        /// <param name="value">Value</param>
        /// <returns>Return true if its ok</returns>
        public bool SetProperty(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName)) return false;

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
                        return true;
                }

                bool dv = ReflectionHelper.SetProperty(this, propertyName, value);

                if (dv && string.Compare(propertyName, "Payload", true) == 0)
                {
                    if (m.Payload != null)
                        m.Payload.SetIO(_IO);
                }

                return dv;
            }
            else
            {
                return ReflectionHelper.SetProperty(this, propertyName, value);
            }
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
                    if (m.PayloadRequirements != null && !(m.PayloadRequirements is NoPayloadRequired))
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