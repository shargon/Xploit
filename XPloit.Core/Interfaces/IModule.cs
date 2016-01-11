using System;
using System.IO;
using System.Reflection;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Res;

namespace XPloit.Core.Interfaces
{
    public class IModule
    {
        string _Name = null, _Path = null;
        string _FullPath = null;

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
        public string Name { get { return _Name; } }
        /// <summary>
        /// Path
        /// </summary>
        public string Path { get { return _Path; } }
        /// <summary>
        /// Return full path
        /// </summary>
        public string FullPath { get { return _FullPath; } }
        ///// <summary>
        /// Type
        /// </summary>
        internal virtual EModuleType ModuleType { get { return EModuleType.Module; } }

        public override string ToString() { return FullPath; }

        public IModule()
        {
            Type t = GetType();
            _Path = t.Namespace.Replace(".", "/");
            _Name = t.Name;
            _FullPath = _Path + "/" + _Name;
        }

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

                                // Check if the Payload still valid
                                if (m.Payload != null && m.PayloadRequirements != null && !m.PayloadRequirements.IsAllowed(m.Payload))
                                    m.Payload = null;

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
        /// <param name="error">Variable for capture the error fail</param>
        /// <returns>Return true if OK, false if not</returns>
        public bool CheckRequiredProperties(out string error)
        {
            error = null;

            Type fileInfoType = typeof(FileInfo);
            Type dirInfoType = typeof(DirectoryInfo);

            foreach (PropertyInfo pi in ReflectionHelper.GetProperties(this, true, true, true))
            {
                ConfigurableProperty c = pi.GetCustomAttribute<ConfigurableProperty>();
                if (c == null)
                    continue;

                object val = pi.GetValue(this);

                if (val == null)
                {
                    if (!c.Required) continue;

                    error = Lang.Get("Require_Set_Property", pi.Name);
                    return false;
                }
                else
                {
                    if (pi.PropertyType == fileInfoType)
                    {
                        FileRequireExists c2 = pi.GetCustomAttribute<FileRequireExists>();
                        if (!c2.IsValid(val))
                        {
                            error = Lang.Get("File_Defined_Not_Exists", pi.Name);
                            return false;
                        }
                    }
                }
            }

            if (this is Module)
            {
                // Module especify
                Module m = (Module)this;

                if (m.Target == null)
                {
                    error = Lang.Get("Require_Set_Property", "Target");
                    return false;
                }

                if (m.Payload == null)
                {
                    if (m.PayloadRequirements != null && m.PayloadRequirements.ItsRequired())
                    {
                        error = Lang.Get("Require_Set_Property", "Payload");
                        return false;
                    }
                }

                if (m.Payload != null)
                    foreach (PropertyInfo pi in ReflectionHelper.GetProperties(m.Payload, true, true, true))
                    {
                        ConfigurableProperty c = pi.GetCustomAttribute<ConfigurableProperty>();
                        if (c == null)
                            continue;

                        object val = pi.GetValue(m.Payload);

                        if (val == null)
                        {
                            if (!c.Required) continue;

                            error = Lang.Get("Require_Set_Property", pi.Name);
                            return false;
                        }
                        else
                        {
                            if (pi.PropertyType == fileInfoType)
                            {
                                FileRequireExists c2 = pi.GetCustomAttribute<FileRequireExists>();
                                if (!c2.IsValid(val))
                                {
                                    error = Lang.Get("File_Defined_Not_Exists", pi.Name);
                                    return false;
                                }
                            }
                        }
                    }
            }

            return true;
        }
    }
}