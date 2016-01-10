using System;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;

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
        public bool IsInProgress
        {
            get
            {
                if (_IO == null) return false;
                return _IO.IsInProgress;
            }
        }
        public void WriteProgress(double value)
        {
            if (_IO == null) return;
            _IO.WriteProgress(value);
        }
        public void EndProgress()
        {
            if (_IO == null) return;
            _IO.EndProgress();
        }
        public void StartProgress(double max)
        {
            if (_IO == null) return;
            _IO.StartProgress(max);
        }
        public void WriteError(string error)
        {
            if (_IO == null) return;
            _IO.WriteError(error);
        }
        public void WriteInfo(string info)
        {
            if (_IO == null) return;
            _IO.WriteInfo(info);
        }
        public void WriteInfo(string info, string colorText, ConsoleColor color)
        {
            if (_IO == null) return;
            _IO.WriteInfo(info, colorText, color);
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
                    if (m.PayloadRequirements != null && m.PayloadRequirements.ItsRequired())
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