using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XPloit.Core.Attributes;
using XPloit.Core.Command;
using XPloit.Core.Enums;
using XPloit.Core.Listeners.Layer;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Helpers.Interfaces;
using XPloit.Res;

namespace XPloit.Core.Interfaces
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Sniffer")]
    public class IModule : IProgress
    {
        string _Name, _ModulePath;
        string _FullPath;

        CommandLayer _IO;

        internal CommandLayer IO { get { return _IO; } }

        internal void SetIO(CommandLayer io) { _IO = io; }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="job">Job</param>
        /// <returns>Return the job</returns>
        public Job CreateJob(IJobable job)
        {
            return Job.Create(this, job);
        }
        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="job">Job</param>
        /// <returns>Return the job</returns>
        public Job CreateJob(Task job)
        {
            job.Start();
            return CreateJob(job, "IsCompleted");
        }
        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="obj">Disposed Object</param>
        /// <param name="isDisposedProperty">Disposed Property</param>
        public Job CreateJob(IDisposable obj, string isDisposedProperty = "IsDisposed")
        {
            return Job.Create(this, obj, isDisposedProperty);
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
        public void WriteTable(CommandTable table)
        {
            if (_IO == null) return;
            table.OutputColored(_IO);
        }
        public void Beep()
        {
            if (_IO == null) return;
            _IO.Beep();
        }
        #endregion

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get { return _Name; } }
        /// <summary>
        /// Path
        /// </summary>
        public string ModulePath { get { return _ModulePath; } }
        /// <summary>
        /// Return full path
        /// </summary>
        public string FullPath { get { return _FullPath; } }
        /// <summary>
        /// DisclosureDate
        /// </summary>
        public virtual DateTime DisclosureDate { get { return DateTime.MinValue; } }
        ///// <summary>
        /// Type
        /// </summary>
        internal virtual EModuleType ModuleType { get { return EModuleType.Module; } }

        public override string ToString() { return FullPath; }

        public IModule()
        {
            GetNames(GetType(), out _ModulePath, out _Name, out _FullPath);
        }

        public static void GetNames(Type t, out string modulePath, out string name, out string fullPath)
        {
            modulePath = t.Namespace.Replace(".", "/");
            name = t.Name;
            fullPath = modulePath + "/" + name;
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
                                if (m.Payload != null && m.PayloadRequirements != null && !m.PayloadRequirements.IsAllowed(new ModuleHeader<Payload>(m.Payload.GetType())))
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
        /// Check Required Properties
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="cmd">Command</param>
        /// <param name="error">Variable for capture the error fail</param>
        /// <returns>Return true if OK, false if not</returns>
        public static bool CheckRequiredProperties(object obj, CommandLayer cmd, out string error)
        {
            error = null;
            Type fileInfoType = typeof(FileInfo);
            Type dirInfoType = typeof(DirectoryInfo);

            foreach (PropertyInfo pi in ReflectionHelper.GetProperties(obj, true, true, true))
            {
                ConfigurableProperty c = pi.GetCustomAttribute<ConfigurableProperty>();
                if (c == null)
                    continue;

                object val = pi.GetValue(obj, null);

                if (val == null)
                {
                    if (c.Optional) continue;

                    error = Lang.Get("Require_Set_Property", pi.Name);
                    return false;
                }
                else
                {
                    if (pi.PropertyType == fileInfoType)
                    {
                        RequireExistsAttribute c2 = pi.GetCustomAttribute<RequireExistsAttribute>();
                        if (c2 != null && !c2.IsValid(val))
                        {
                            error = Lang.Get("File_Defined_Not_Exists", pi.Name);
                            return false;
                        }
                    }
                    else
                    {
                        if (pi.PropertyType == dirInfoType)
                        {
                            // Check directory
                            DirectoryInfo di = (DirectoryInfo)val;
                            di.Refresh();
                            if (!di.Exists)
                            {
                                if (cmd == null)
                                {
                                    // Por si acaso
                                    error = Lang.Get("Folder_Required", di.FullName);
                                    return false;
                                }

                                cmd.WriteLine(Lang.Get("Folder_Required_Ask", di.FullName));
                                if (!(bool)ConvertHelper.ConvertTo(cmd.ReadLine(null, null), typeof(bool)))
                                {
                                    error = Lang.Get("Folder_Required", di.FullName);
                                    return false;
                                }

                                try { di.Create(); }
                                catch (Exception e) { cmd.WriteError(e.ToString()); }
                            }
                        }
                    }
                }
            }

            return error == null;
        }
        /// <summary>
        /// Check Required Properties
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="error">Variable for capture the error fail</param>
        /// <returns>Return true if OK, false if not</returns>
        public bool CheckRequiredProperties(CommandLayer cmd, out string error)
        {
            error = null;

            if (!CheckRequiredProperties(this, cmd, out error)) return false;

            if (this is Module)
            {
                // Module especify
                Module m = (Module)this;

                if (m.Target == null)
                {
                    Target[] t = m.Targets;
                    if (t != null && t.Length > 0)
                    {
                        error = Lang.Get("Require_Set_Property", "Target");
                        return false;
                    }
                }

                if (m.Payload == null)
                {
                    if (m.PayloadRequirements != null && m.PayloadRequirements.ItsRequired())
                    {
                        error = Lang.Get("Require_Set_Property", "Payload");
                        return false;
                    }
                }
                else
                {
                    if (!CheckRequiredProperties(m.Payload, cmd, out error)) return false;
                }
            }

            return true;
        }
    }
}
